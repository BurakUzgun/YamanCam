using System.Globalization;
using System.Text;
using YamanCam.Web.Data;
using YamanCam.Web.Models;
using YamanCam.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace YamanCam.Web.Controllers;

/// <summary>
/// Gümrük Nakliye Faturası giriş ekranı. Fatura carisi App_AccountPlan'da AccountType = "Cari"
/// olan hesaplardan, satırlardaki gider hesapları ise tüm aktif detay hesaplardan seçilir.
/// Her satır kendi KDV oranı + tevkifat (KDV indirim) oranıyla Matrah/KDV/Net KDV/Toplam
/// hesaplar. Faturaya isteğe bağlı olarak birden fazla malzeme (App_Stock) ve miktarı
/// eklenebilir; bu seçim ileride malzeme maliyet hesabında nakliye/gümrük giderini
/// malzeme değerine eklemek için kullanılacaktır.
/// </summary>
public class AppCustomsFreightInvoicesController : Controller
{
    private const string CariAccountType = "Cari";
    private static readonly string[] AllowedCurrencyCodes = { "TRY", "USD", "EUR", "GBP" };

    private readonly ApplicationDbContext _context;
    private readonly IAppLogService _appLogService;
    private readonly IUserRightService _userRightService;
    private readonly IAppSettingService _appSettingService;

    public AppCustomsFreightInvoicesController(
        ApplicationDbContext context,
        IAppLogService appLogService,
        IUserRightService userRightService,
        IAppSettingService appSettingService)
    {
        _context = context;
        _appLogService = appLogService;
        _userRightService = userRightService;
        _appSettingService = appSettingService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(bool showDeleted = false)
    {
        var denied = await EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        var query = _context.AppCustomsFreightInvoices.AsNoTracking();

        query = showDeleted
            ? query.Where(x => x.IsActive == false)
            : query.Where(x => x.IsActive != false);

        var items = await query
            .OrderByDescending(x => x.TransactionDate)
            .ThenByDescending(x => x.RecId)
            .Select(x => new AppCustomsFreightInvoiceListItemViewModel
            {
                RecId = x.RecId,
                InvoiceNo = x.InvoiceNo,
                InvoiceDate = x.InvoiceDate,
                TransactionDate = x.TransactionDate,
                WorkPlaceName = x.WorkPlace != null ? x.WorkPlace.WorkPlaceName : null,
                AccountName = x.Account != null ? x.Account.AccountName : null,
                MaterialCount = x.Materials.Count,
                NetAmount = x.NetAmount,
                VatAmount = x.VatAmount,
                WithholdingAmount = x.WithholdingAmount,
                NetVatAmount = x.NetVatAmount,
                TotalAmount = x.TotalAmount,
                CurrencyCode = x.CurrencyCode,
                ExchangeRate = x.ExchangeRate,
                CurrencyAmount = x.CurrencyAmount
            })
            .ToListAsync();

        ViewData["Kusurat"] = await _appSettingService.GetKusuratMapAsync();

        return View(new AppCustomsFreightInvoiceListViewModel
        {
            ShowDeleted = showDeleted,
            Items = items
        });
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var denied = await EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        var vm = new AppCustomsFreightInvoiceEditViewModel();
        await PopulateSelectListsAsync(vm);
        ViewData["Kusurat"] = await _appSettingService.GetKusuratMapAsync();
        return View("Edit", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppCustomsFreightInvoiceEditViewModel vm)
    {
        var denied = await EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        vm.RecId = 0;
        await ValidateInvoiceAsync(vm);

        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync(vm);
            ViewData["Kusurat"] = await _appSettingService.GetKusuratMapAsync();
            return View("Edit", vm);
        }

        var entity = new AppCustomsFreightInvoice { IsActive = true };
        ApplyViewModel(entity, vm);
        entity.CreatedDate = DateTime.UtcNow;
        ApplyLines(entity, vm);
        ApplyMaterials(entity, vm);

        _context.AppCustomsFreightInvoices.Add(entity);
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "CustomsFreightInvoice",
            "Created",
            $"Gümrük Nakliye Faturası oluşturuldu. No={entity.InvoiceNo}",
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = "Gümrük Nakliye Faturası kaydedildi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var denied = await EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        var entity = await _context.AppCustomsFreightInvoices
            .AsNoTracking()
            .Include(x => x.Lines)
            .Include(x => x.Materials)
            .FirstOrDefaultAsync(x => x.RecId == id);

        if (entity is null)
        {
            return NotFound();
        }

        var vm = MapToViewModel(entity);
        await PopulateSelectListsAsync(vm);
        ViewData["Kusurat"] = await _appSettingService.GetKusuratMapAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AppCustomsFreightInvoiceEditViewModel vm)
    {
        var denied = await EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        if (id != vm.RecId)
        {
            return NotFound();
        }

        var entity = await _context.AppCustomsFreightInvoices
            .Include(x => x.Lines)
            .Include(x => x.Materials)
            .FirstOrDefaultAsync(x => x.RecId == id);

        if (entity is null)
        {
            return NotFound();
        }

        var oldAudit = BuildAuditValue(entity);
        await ValidateInvoiceAsync(vm);

        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync(vm);
            ViewData["Kusurat"] = await _appSettingService.GetKusuratMapAsync();
            return View(vm);
        }

        ApplyViewModel(entity, vm);
        _context.AppCustomsFreightInvoiceLines.RemoveRange(entity.Lines);
        entity.Lines.Clear();
        ApplyLines(entity, vm);
        _context.AppCustomsFreightInvoiceMaterials.RemoveRange(entity.Materials);
        entity.Materials.Clear();
        ApplyMaterials(entity, vm);

        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "CustomsFreightInvoice",
            "Updated",
            $"Gümrük Nakliye Faturası güncellendi. No={entity.InvoiceNo}",
            oldValue: oldAudit,
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = "Gümrük Nakliye Faturası güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var denied = await EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        var entity = await _context.AppCustomsFreightInvoices
            .Include(x => x.Lines)
            .Include(x => x.Materials)
            .FirstOrDefaultAsync(x => x.RecId == id);

        if (entity is null)
        {
            return NotFound();
        }

        var oldAudit = BuildAuditValue(entity);
        entity.IsActive = false;
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "CustomsFreightInvoice",
            "Deleted",
            $"Gümrük Nakliye Faturası silindi. No={entity.InvoiceNo}",
            oldValue: oldAudit,
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = "Gümrük Nakliye Faturası silindi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id)
    {
        var denied = await EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        var entity = await _context.AppCustomsFreightInvoices
            .Include(x => x.Lines)
            .Include(x => x.Materials)
            .FirstOrDefaultAsync(x => x.RecId == id);

        if (entity is null)
        {
            return NotFound();
        }

        var oldAudit = BuildAuditValue(entity);
        entity.IsActive = true;
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "CustomsFreightInvoice",
            "Restored",
            $"Gümrük Nakliye Faturası geri yüklendi. No={entity.InvoiceNo}",
            oldValue: oldAudit,
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = "Gümrük Nakliye Faturası geri yüklendi.";
        return RedirectToAction(nameof(Index), new { showDeleted = true });
    }

    private async Task<IActionResult?> EnsureAdmin()
    {
        var action = AppScreenRights.ResolveAction(ControllerContext.ActionDescriptor.ActionName);
        var allowed = await _userRightService.HasScreenAccessAsync(User, ControllerContext.ActionDescriptor.ControllerName, action);
        return allowed ? null : RedirectToAction("Index", "Home");
    }

    private async Task PopulateSelectListsAsync(AppCustomsFreightInvoiceEditViewModel vm)
    {
        var workPlaces = await _context.AppWorkPlaces.AsNoTracking()
            .Where(x => x.IsActive != false)
            .OrderBy(x => x.WorkPlaceCode)
            .Select(x => new { x.RecId, x.WorkPlaceCode, x.WorkPlaceName })
            .ToListAsync();

        vm.WorkPlaceOptions = workPlaces
            .Select(x => new SelectListItem($"{x.WorkPlaceCode} - {x.WorkPlaceName}", x.RecId.ToString(CultureInfo.InvariantCulture)))
            .ToList();

        var cariAccounts = await _context.AppAccountPlans.AsNoTracking()
            .Where(x => x.IsActive != false && x.IsDetail != false && x.AccountType == CariAccountType)
            .OrderBy(x => x.AccountCode)
            .Select(x => new { x.RecId, x.AccountCode, x.AccountName })
            .ToListAsync();

        vm.CariAccountOptions = cariAccounts
            .Select(x => new SelectListItem($"{x.AccountCode} - {x.AccountName}", x.RecId.ToString(CultureInfo.InvariantCulture)))
            .ToList();

        var expenseAccounts = await _context.AppAccountPlans.AsNoTracking()
            .Where(x => x.IsActive != false && x.IsDetail != false)
            .OrderBy(x => x.AccountCode)
            .Select(x => new { x.RecId, x.AccountCode, x.AccountName })
            .ToListAsync();

        vm.ExpenseAccountOptions = expenseAccounts
            .Select(x => new SelectListItem($"{x.AccountCode} - {x.AccountName}", x.RecId.ToString(CultureInfo.InvariantCulture)))
            .ToList();

        var stocks = await _context.AppStocks.AsNoTracking()
            .Where(x => x.IsActive != false)
            .OrderBy(x => x.StockCode)
            .Select(x => new { x.RecId, x.StockCode, x.StockName })
            .ToListAsync();

        vm.StockOptions = stocks
            .Select(x => new SelectListItem($"{x.StockCode} - {x.StockName}", x.RecId.ToString(CultureInfo.InvariantCulture)))
            .ToList();

        ViewData["StockMeta"] = stocks
            .Select(x => new { id = x.RecId, code = x.StockCode, name = x.StockName })
            .ToList();

        ViewData["ExpenseAccountMeta"] = expenseAccounts
            .Select(x => new { id = x.RecId, code = x.AccountCode, name = x.AccountName })
            .ToList();

        var withholdingDefinitions = await _context.AppVatWithholdingDefinitions.AsNoTracking()
            .Where(x => x.IsActive != false)
            .OrderBy(x => x.WithholdingCode)
            .Select(x => new { x.RecId, x.WithholdingCode, x.WithholdingName, x.VatRate, x.WithholdingRate })
            .ToListAsync();

        vm.WithholdingDefinitionOptions = withholdingDefinitions
            .Select(x => new SelectListItem($"{x.WithholdingCode} - {x.WithholdingName} (KDV %{x.VatRate:0.##}, Tevkifat %{x.WithholdingRate:0.##})", x.RecId.ToString(CultureInfo.InvariantCulture)))
            .ToList();

        ViewData["WithholdingDefinitionMeta"] = withholdingDefinitions
            .Select(x => new { id = x.RecId, code = x.WithholdingCode, withholdingRate = x.WithholdingRate })
            .ToList();
    }

    private async Task ValidateInvoiceAsync(AppCustomsFreightInvoiceEditViewModel vm)
    {
        vm.InvoiceNo = vm.InvoiceNo.Trim().ToUpperInvariant();
        vm.InvoiceKind = string.IsNullOrWhiteSpace(vm.InvoiceKind) ? null : vm.InvoiceKind.Trim();
        vm.SpecialCode = string.IsNullOrWhiteSpace(vm.SpecialCode) ? null : vm.SpecialCode.Trim();
        vm.CurrencyCode = string.IsNullOrWhiteSpace(vm.CurrencyCode) ? "TRY" : vm.CurrencyCode.Trim().ToUpperInvariant();

        ModelState.Remove(nameof(vm.CurrencyCode));
        if (!AllowedCurrencyCodes.Contains(vm.CurrencyCode))
        {
            ModelState.AddModelError(nameof(vm.CurrencyCode), "Geçersiz döviz kodu.");
        }

        if (vm.CurrencyCode == "TRY")
        {
            vm.ExchangeRate = 1m;
        }
        else if (vm.ExchangeRate <= 0)
        {
            ModelState.AddModelError(nameof(vm.ExchangeRate), "Döviz kuru sıfırdan büyük olmalıdır.");
        }

        var codeExists = await _context.AppCustomsFreightInvoices.AnyAsync(x =>
            x.InvoiceNo == vm.InvoiceNo && x.RecId != vm.RecId);
        if (codeExists)
        {
            ModelState.AddModelError(nameof(vm.InvoiceNo), "Bu fatura no zaten kullanılıyor.");
        }

        if (!vm.WorkPlaceId.HasValue ||
            !await _context.AppWorkPlaces.AnyAsync(x => x.RecId == vm.WorkPlaceId.Value))
        {
            ModelState.AddModelError(nameof(vm.WorkPlaceId), "Geçerli bir şube seçilmelidir.");
        }

        if (!await _context.AppAccountPlans.AnyAsync(x => x.RecId == vm.AccountId && x.AccountType == CariAccountType))
        {
            ModelState.AddModelError(nameof(vm.AccountId), "Geçerli bir cari hesap seçilmelidir.");
        }

        vm.Lines = vm.Lines
            .Where(x => x.AccountId > 0 || x.Amount != 0)
            .ToList();

        if (vm.Lines.Count < 1)
        {
            ModelState.AddModelError(string.Empty, "En az 1 hesap satırı girilmelidir.");
        }

        for (var i = 0; i < vm.Lines.Count; i++)
        {
            var line = vm.Lines[i];
            line.LineNo = i + 1;

            if (line.AccountId <= 0 || !await _context.AppAccountPlans.AnyAsync(x => x.RecId == line.AccountId))
            {
                ModelState.AddModelError($"Lines[{i}].AccountId", "Geçerli bir hesap seçilmelidir.");
            }

            if (line.Amount <= 0)
            {
                ModelState.AddModelError($"Lines[{i}].Amount", "Tutar sıfırdan büyük olmalıdır.");
            }

            if (line.VatRate < 0)
            {
                ModelState.AddModelError($"Lines[{i}].VatRate", "KDV oranı negatif olamaz.");
            }

            if (line.WithholdingRate is < 0 or > 100)
            {
                ModelState.AddModelError($"Lines[{i}].WithholdingRate", "Tevkifat (KDV indirim) oranı 0-100 arasında olmalıdır.");
            }

            if (line.WithholdingDefinitionId.HasValue &&
                !await _context.AppVatWithholdingDefinitions.AnyAsync(x => x.RecId == line.WithholdingDefinitionId.Value))
            {
                ModelState.AddModelError($"Lines[{i}].WithholdingDefinitionId", "Geçersiz tevkifat tanımı seçildi.");
            }

            line.VatAmount = Math.Round(line.Amount * line.VatRate / 100m, 10, MidpointRounding.AwayFromZero);
            line.WithholdingAmount = Math.Round(line.VatAmount * line.WithholdingRate / 100m, 10, MidpointRounding.AwayFromZero);
            line.NetVatAmount = line.VatAmount - line.WithholdingAmount;
            line.TotalAmount = line.Amount + line.NetVatAmount;
        }

        vm.Materials = vm.Materials
            .Where(x => x.StockId > 0 || x.Quantity != 0)
            .ToList();

        for (var i = 0; i < vm.Materials.Count; i++)
        {
            var material = vm.Materials[i];
            material.LineNo = i + 1;

            if (material.StockId <= 0 || !await _context.AppStocks.AnyAsync(x => x.RecId == material.StockId))
            {
                ModelState.AddModelError($"Materials[{i}].StockId", "Geçerli bir malzeme seçilmelidir.");
            }

            if (material.Quantity <= 0)
            {
                ModelState.AddModelError($"Materials[{i}].Quantity", "Miktar sıfırdan büyük olmalıdır.");
            }
        }

        vm.NetAmount = vm.Lines.Sum(x => x.Amount);
        vm.VatAmount = vm.Lines.Sum(x => x.VatAmount);
        vm.WithholdingAmount = vm.Lines.Sum(x => x.WithholdingAmount);
        vm.NetVatAmount = vm.VatAmount - vm.WithholdingAmount;
        vm.TotalAmount = vm.NetAmount + vm.NetVatAmount;
        vm.CurrencyAmount = vm.ExchangeRate > 0
            ? Math.Round(vm.TotalAmount / vm.ExchangeRate, 10, MidpointRounding.AwayFromZero)
            : 0m;

        if (vm.TotalAmount <= 0)
        {
            ModelState.AddModelError(string.Empty, "Fatura tutarı sıfırdan büyük olmalıdır.");
        }
    }

    private AppCustomsFreightInvoiceEditViewModel MapToViewModel(AppCustomsFreightInvoice entity)
    {
        return new AppCustomsFreightInvoiceEditViewModel
        {
            RecId = entity.RecId,
            InvoiceDate = entity.InvoiceDate,
            TransactionDate = entity.TransactionDate,
            WorkPlaceId = entity.WorkPlaceId,
            InvoiceKind = entity.InvoiceKind,
            AccountId = entity.AccountId,
            InvoiceNo = entity.InvoiceNo,
            SpecialCode = entity.SpecialCode,
            NetAmount = entity.NetAmount,
            VatAmount = entity.VatAmount,
            WithholdingAmount = entity.WithholdingAmount,
            NetVatAmount = entity.NetVatAmount,
            TotalAmount = entity.TotalAmount,
            CurrencyAmount = entity.CurrencyAmount,
            ExchangeRate = entity.ExchangeRate,
            CurrencyCode = entity.CurrencyCode,
            Lines = entity.Lines
                .OrderBy(x => x.LineNo)
                .Select(x => new AppCustomsFreightInvoiceLineEditViewModel
                {
                    RecId = x.RecId,
                    LineNo = x.LineNo,
                    AccountId = x.AccountId,
                    Amount = x.Amount,
                    VatRate = x.VatRate,
                    WithholdingDefinitionId = x.WithholdingDefinitionId,
                    WithholdingRate = x.WithholdingRate,
                    VatAmount = x.VatAmount,
                    WithholdingAmount = x.WithholdingAmount,
                    NetVatAmount = x.NetVatAmount,
                    TotalAmount = x.TotalAmount
                })
                .ToList(),
            Materials = entity.Materials
                .OrderBy(x => x.LineNo)
                .Select(x => new AppCustomsFreightInvoiceMaterialEditViewModel
                {
                    RecId = x.RecId,
                    LineNo = x.LineNo,
                    StockId = x.StockId,
                    Quantity = x.Quantity
                })
                .ToList()
        };
    }

    private static void ApplyViewModel(AppCustomsFreightInvoice entity, AppCustomsFreightInvoiceEditViewModel vm)
    {
        entity.InvoiceDate = vm.InvoiceDate;
        entity.TransactionDate = vm.TransactionDate;
        entity.WorkPlaceId = vm.WorkPlaceId;
        entity.InvoiceKind = vm.InvoiceKind;
        entity.AccountId = vm.AccountId;
        entity.InvoiceNo = vm.InvoiceNo.Trim().ToUpperInvariant();
        entity.SpecialCode = vm.SpecialCode;
        entity.NetAmount = vm.NetAmount;
        entity.VatAmount = vm.VatAmount;
        entity.WithholdingAmount = vm.WithholdingAmount;
        entity.NetVatAmount = vm.NetVatAmount;
        entity.TotalAmount = vm.TotalAmount;
        entity.CurrencyAmount = vm.CurrencyAmount;
        entity.ExchangeRate = vm.ExchangeRate;
        entity.CurrencyCode = vm.CurrencyCode;
    }

    private static void ApplyLines(AppCustomsFreightInvoice entity, AppCustomsFreightInvoiceEditViewModel vm)
    {
        foreach (var line in vm.Lines)
        {
            entity.Lines.Add(new AppCustomsFreightInvoiceLine
            {
                LineNo = line.LineNo,
                AccountId = line.AccountId,
                Amount = line.Amount,
                VatRate = line.VatRate,
                WithholdingDefinitionId = line.WithholdingDefinitionId,
                WithholdingRate = line.WithholdingRate,
                VatAmount = line.VatAmount,
                WithholdingAmount = line.WithholdingAmount,
                NetVatAmount = line.NetVatAmount,
                TotalAmount = line.TotalAmount,
                CreatedDate = DateTime.UtcNow
            });
        }
    }

    private static void ApplyMaterials(AppCustomsFreightInvoice entity, AppCustomsFreightInvoiceEditViewModel vm)
    {
        foreach (var material in vm.Materials)
        {
            entity.Materials.Add(new AppCustomsFreightInvoiceMaterial
            {
                LineNo = material.LineNo,
                StockId = material.StockId,
                Quantity = material.Quantity,
                CreatedDate = DateTime.UtcNow
            });
        }
    }

    private static string BuildAuditValue(AppCustomsFreightInvoice entity)
    {
        var sb = new StringBuilder();
        sb.Append($"No={entity.InvoiceNo}, Tarih={entity.InvoiceDate:yyyy-MM-dd}, IslemTarihi={entity.TransactionDate:yyyy-MM-dd}");
        sb.Append($", CariId={entity.AccountId}, SubeId={entity.WorkPlaceId}");
        sb.Append($", MalzemeSayisi={entity.Materials.Count}");
        sb.Append($", Matrah={entity.NetAmount}, KDV={entity.VatAmount}, Tevkifat={entity.WithholdingAmount}");
        sb.Append($", GenelTutar={entity.TotalAmount}, DovizTutar={entity.CurrencyAmount} {entity.CurrencyCode}");
        sb.Append($", SatirSayisi={entity.Lines.Count}");
        sb.Append($", IsActive={entity.IsActive == true}");
        return sb.ToString();
    }
}
