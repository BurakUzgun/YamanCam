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
/// Satış Faturası giriş ekranı. Fatura carisi App_AccountPlan'da AccountType = "Cari"
/// olan hesaplardan seçilir, satırlar App_Stock (malzeme kartları) referans alır.
/// Döviz ile girilen tutarlar, fatura kuru ile TL karşılığına çevrilip ayrıca saklanır.
/// Alış Faturası ekranıyla (AppPurchaseInvoicesController) aynı yapıyı izler; tek fark
/// satır KDV varsayılanının stok kartındaki Satış KDV'sinden gelmesidir.
/// </summary>
public class AppSalesInvoicesController : Controller
{
    private const string CariAccountType = "Cari";
    private static readonly string[] AllowedCurrencyCodes = { "TRY", "USD", "EUR", "GBP" };

    private readonly ApplicationDbContext _context;
    private readonly IAppLogService _appLogService;
    private readonly IUserRightService _userRightService;
    private readonly IAppSettingService _appSettingService;

    public AppSalesInvoicesController(
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

        var query = _context.AppSalesInvoices.AsNoTracking();

        query = showDeleted
            ? query.Where(x => x.IsActive == false)
            : query.Where(x => x.IsActive != false);

        var items = await query
            .OrderByDescending(x => x.InvoiceDate)
            .ThenByDescending(x => x.RecId)
            .Select(x => new AppSalesInvoiceListItemViewModel
            {
                RecId = x.RecId,
                InvoiceNo = x.InvoiceNo,
                InvoiceDate = x.InvoiceDate,
                CompanyName = x.Company != null ? x.Company.CompanyName : null,
                WorkPlaceName = x.WorkPlace != null ? x.WorkPlace.WorkPlaceName : null,
                AccountName = x.Account != null ? x.Account.AccountName : null,
                Description = x.Description,
                CurrencyCode = x.CurrencyCode,
                ExchangeRate = x.ExchangeRate,
                TotalAmount = x.TotalAmount,
                TotalAmountTRY = x.TotalAmountTRY
            })
            .ToListAsync();

        ViewData["Kusurat"] = await _appSettingService.GetKusuratMapAsync();

        return View(new AppSalesInvoiceListViewModel
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

        var vm = new AppSalesInvoiceEditViewModel();
        await PopulateSelectListsAsync(vm);
        ViewData["Kusurat"] = await _appSettingService.GetKusuratMapAsync();
        return View("Edit", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppSalesInvoiceEditViewModel vm)
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

        var entity = new AppSalesInvoice { IsActive = true };
        ApplyViewModel(entity, vm);
        entity.CreatedDate = DateTime.UtcNow;
        ApplyLines(entity, vm);

        _context.AppSalesInvoices.Add(entity);
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "SalesInvoice",
            "Created",
            $"Satış Faturası oluşturuldu. No={entity.InvoiceNo}",
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = "Satış Faturası kaydedildi.";
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

        var entity = await _context.AppSalesInvoices
            .AsNoTracking()
            .Include(x => x.Lines)
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
    public async Task<IActionResult> Edit(int id, AppSalesInvoiceEditViewModel vm)
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

        var entity = await _context.AppSalesInvoices
            .Include(x => x.Lines)
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
        _context.AppSalesInvoiceLines.RemoveRange(entity.Lines);
        entity.Lines.Clear();
        ApplyLines(entity, vm);

        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "SalesInvoice",
            "Updated",
            $"Satış Faturası güncellendi. No={entity.InvoiceNo}",
            oldValue: oldAudit,
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = "Satış Faturası güncellendi.";
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

        var entity = await _context.AppSalesInvoices
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.RecId == id);

        if (entity is null)
        {
            return NotFound();
        }

        var oldAudit = BuildAuditValue(entity);
        entity.IsActive = false;
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "SalesInvoice",
            "Deleted",
            $"Satış Faturası silindi. No={entity.InvoiceNo}",
            oldValue: oldAudit,
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = "Satış Faturası silindi.";
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

        var entity = await _context.AppSalesInvoices
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.RecId == id);

        if (entity is null)
        {
            return NotFound();
        }

        var oldAudit = BuildAuditValue(entity);
        entity.IsActive = true;
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "SalesInvoice",
            "Restored",
            $"Satış Faturası geri yüklendi. No={entity.InvoiceNo}",
            oldValue: oldAudit,
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = "Satış Faturası geri yüklendi.";
        return RedirectToAction(nameof(Index), new { showDeleted = true });
    }

    private async Task<IActionResult?> EnsureAdmin()
    {
        var action = AppScreenRights.ResolveAction(ControllerContext.ActionDescriptor.ActionName);
        var allowed = await _userRightService.HasScreenAccessAsync(User, ControllerContext.ActionDescriptor.ControllerName, action);
        return allowed ? null : RedirectToAction("Index", "Home");
    }

    private async Task PopulateSelectListsAsync(AppSalesInvoiceEditViewModel vm)
    {
        var companies = await _context.AppCompanies.AsNoTracking()
            .Where(x => x.IsActive != false)
            .OrderBy(x => x.CompanyCode)
            .Select(x => new { x.RecId, x.CompanyCode, x.CompanyName })
            .ToListAsync();

        vm.CompanyOptions = companies
            .Select(x => new SelectListItem($"{x.CompanyCode} - {x.CompanyName}", x.RecId.ToString(CultureInfo.InvariantCulture)))
            .ToList();

        var workPlaces = await _context.AppWorkPlaces.AsNoTracking()
            .Where(x => x.IsActive != false)
            .OrderBy(x => x.WorkPlaceCode)
            .Select(x => new { x.RecId, x.WorkPlaceCode, x.WorkPlaceName })
            .ToListAsync();

        vm.WorkPlaceOptions = workPlaces
            .Select(x => new SelectListItem($"{x.WorkPlaceCode} - {x.WorkPlaceName}", x.RecId.ToString(CultureInfo.InvariantCulture)))
            .ToList();

        var accounts = await _context.AppAccountPlans.AsNoTracking()
            .Where(x => x.IsActive != false && x.IsDetail != false && x.AccountType == CariAccountType)
            .OrderBy(x => x.AccountCode)
            .Select(x => new { x.RecId, x.AccountCode, x.AccountName })
            .ToListAsync();

        vm.AccountOptions = accounts
            .Select(x => new SelectListItem($"{x.AccountCode} - {x.AccountName}", x.RecId.ToString(CultureInfo.InvariantCulture)))
            .ToList();

        var stocks = await _context.AppStocks.AsNoTracking()
            .Where(x => x.IsActive != false)
            .OrderBy(x => x.StockCode)
            .Select(x => new
            {
                x.RecId,
                x.StockCode,
                x.StockName,
                VatRate = x.SalesVat != null ? x.SalesVat.VatRate : (decimal?)null
            })
            .ToListAsync();

        vm.StockOptions = stocks
            .Select(x => new SelectListItem($"{x.StockCode} - {x.StockName}", x.RecId.ToString(CultureInfo.InvariantCulture)))
            .ToList();

        ViewData["StockVatRates"] = stocks.ToDictionary(
            x => x.RecId.ToString(CultureInfo.InvariantCulture),
            x => x.VatRate ?? 0m);

        var withholdingDefinitions = await _context.AppVatWithholdingDefinitions.AsNoTracking()
            .Where(x => x.IsActive != false)
            .OrderBy(x => x.WithholdingCode)
            .Select(x => new { x.RecId, x.WithholdingCode, x.WithholdingName, x.VatRate, x.WithholdingRate })
            .ToListAsync();

        vm.WithholdingDefinitionOptions = withholdingDefinitions
            .Select(x => new SelectListItem($"{x.WithholdingCode} - {x.WithholdingName} (KDV %{x.VatRate:0.##}, Tevkifat %{x.WithholdingRate:0.##})", x.RecId.ToString(CultureInfo.InvariantCulture)))
            .ToList();

        ViewData["WithholdingDefinitionRates"] = withholdingDefinitions.ToDictionary(
            x => x.RecId.ToString(CultureInfo.InvariantCulture),
            x => x.WithholdingRate);
    }

    private async Task ValidateInvoiceAsync(AppSalesInvoiceEditViewModel vm)
    {
        vm.InvoiceNo = vm.InvoiceNo.Trim().ToUpperInvariant();
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

        var codeExists = await _context.AppSalesInvoices.AnyAsync(x =>
            x.InvoiceNo == vm.InvoiceNo && x.RecId != vm.RecId);
        if (codeExists)
        {
            ModelState.AddModelError(nameof(vm.InvoiceNo), "Bu fatura no zaten kullanılıyor.");
        }

        if (vm.CompanyId.HasValue &&
            !await _context.AppCompanies.AnyAsync(x => x.RecId == vm.CompanyId.Value))
        {
            ModelState.AddModelError(nameof(vm.CompanyId), "Geçersiz şirket seçildi.");
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
            .Where(x => x.StockId > 0 || x.Quantity != 0 || x.UnitPrice != 0)
            .ToList();

        if (vm.Lines.Count < 1)
        {
            ModelState.AddModelError(string.Empty, "En az 1 malzeme satırı girilmelidir.");
        }

        for (var i = 0; i < vm.Lines.Count; i++)
        {
            var line = vm.Lines[i];
            line.LineNo = i + 1;

            if (line.StockId <= 0 || !await _context.AppStocks.AnyAsync(x => x.RecId == line.StockId))
            {
                ModelState.AddModelError($"Lines[{i}].StockId", "Geçerli bir malzeme seçilmelidir.");
            }

            if (line.Quantity <= 0)
            {
                ModelState.AddModelError($"Lines[{i}].Quantity", "Miktar sıfırdan büyük olmalıdır.");
            }

            if (line.UnitPrice < 0)
            {
                ModelState.AddModelError($"Lines[{i}].UnitPrice", "Birim fiyat negatif olamaz.");
            }

            if (line.VatRate < 0)
            {
                ModelState.AddModelError($"Lines[{i}].VatRate", "KDV oranı negatif olamaz.");
            }

            if (line.WithholdingDefinitionId.HasValue &&
                !await _context.AppVatWithholdingDefinitions.AnyAsync(x => x.RecId == line.WithholdingDefinitionId.Value))
            {
                ModelState.AddModelError($"Lines[{i}].WithholdingDefinitionId", "Geçersiz tevkifat tanımı seçildi.");
            }

            if (line.WithholdingRate is < 0 or > 100)
            {
                ModelState.AddModelError($"Lines[{i}].WithholdingRate", "Tevkifat oranı 0-100 arasında olmalıdır.");
            }

            line.NetAmount = Math.Round(line.Quantity * line.UnitPrice, 2, MidpointRounding.AwayFromZero);
            line.VatAmount = Math.Round(line.NetAmount * line.VatRate / 100m, 2, MidpointRounding.AwayFromZero);
            line.WithholdingAmount = Math.Round(line.VatAmount * line.WithholdingRate / 100m, 2, MidpointRounding.AwayFromZero);
            line.NetVatAmount = line.VatAmount - line.WithholdingAmount;
            line.TotalAmount = line.NetAmount + line.NetVatAmount;
        }

        vm.NetAmount = vm.Lines.Sum(x => x.NetAmount);
        vm.VatAmount = vm.Lines.Sum(x => x.VatAmount);
        vm.WithholdingAmount = vm.Lines.Sum(x => x.WithholdingAmount);
        vm.NetVatAmount = vm.VatAmount - vm.WithholdingAmount;
        vm.TotalAmount = vm.NetAmount + vm.NetVatAmount;

        vm.NetAmountTRY = Math.Round(vm.NetAmount * vm.ExchangeRate, 2, MidpointRounding.AwayFromZero);
        vm.VatAmountTRY = Math.Round(vm.VatAmount * vm.ExchangeRate, 2, MidpointRounding.AwayFromZero);
        vm.WithholdingAmountTRY = Math.Round(vm.WithholdingAmount * vm.ExchangeRate, 2, MidpointRounding.AwayFromZero);
        vm.NetVatAmountTRY = vm.VatAmountTRY - vm.WithholdingAmountTRY;
        vm.TotalAmountTRY = vm.NetAmountTRY + vm.NetVatAmountTRY;

        if (vm.TotalAmount <= 0)
        {
            ModelState.AddModelError(string.Empty, "Fatura tutarı sıfırdan büyük olmalıdır.");
        }
    }

    private AppSalesInvoiceEditViewModel MapToViewModel(AppSalesInvoice entity)
    {
        return new AppSalesInvoiceEditViewModel
        {
            RecId = entity.RecId,
            InvoiceNo = entity.InvoiceNo,
            InvoiceDate = entity.InvoiceDate,
            CompanyId = entity.CompanyId,
            WorkPlaceId = entity.WorkPlaceId,
            AccountId = entity.AccountId,
            Description = entity.Description,
            CurrencyCode = entity.CurrencyCode,
            ExchangeRate = entity.ExchangeRate,
            NetAmount = entity.NetAmount,
            VatAmount = entity.VatAmount,
            WithholdingAmount = entity.WithholdingAmount,
            NetVatAmount = entity.NetVatAmount,
            TotalAmount = entity.TotalAmount,
            NetAmountTRY = entity.NetAmountTRY,
            VatAmountTRY = entity.VatAmountTRY,
            WithholdingAmountTRY = entity.WithholdingAmountTRY,
            NetVatAmountTRY = entity.NetVatAmountTRY,
            TotalAmountTRY = entity.TotalAmountTRY,
            Lines = entity.Lines
                .OrderBy(x => x.LineNo)
                .Select(x => new AppSalesInvoiceLineEditViewModel
                {
                    RecId = x.RecId,
                    LineNo = x.LineNo,
                    StockId = x.StockId,
                    Description = x.Description,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,
                    VatRate = x.VatRate,
                    WithholdingDefinitionId = x.WithholdingDefinitionId,
                    WithholdingRate = x.WithholdingRate,
                    NetAmount = x.NetAmount,
                    VatAmount = x.VatAmount,
                    WithholdingAmount = x.WithholdingAmount,
                    NetVatAmount = x.NetVatAmount,
                    TotalAmount = x.TotalAmount
                })
                .ToList()
        };
    }

    private static void ApplyViewModel(AppSalesInvoice entity, AppSalesInvoiceEditViewModel vm)
    {
        entity.InvoiceNo = vm.InvoiceNo.Trim().ToUpperInvariant();
        entity.InvoiceDate = vm.InvoiceDate;
        entity.CompanyId = vm.CompanyId;
        entity.WorkPlaceId = vm.WorkPlaceId;
        entity.AccountId = vm.AccountId;
        entity.Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim();
        entity.CurrencyCode = vm.CurrencyCode;
        entity.ExchangeRate = vm.ExchangeRate;
        entity.NetAmount = vm.NetAmount;
        entity.VatAmount = vm.VatAmount;
        entity.WithholdingAmount = vm.WithholdingAmount;
        entity.NetVatAmount = vm.NetVatAmount;
        entity.TotalAmount = vm.TotalAmount;
        entity.NetAmountTRY = vm.NetAmountTRY;
        entity.VatAmountTRY = vm.VatAmountTRY;
        entity.WithholdingAmountTRY = vm.WithholdingAmountTRY;
        entity.NetVatAmountTRY = vm.NetVatAmountTRY;
        entity.TotalAmountTRY = vm.TotalAmountTRY;
    }

    private static void ApplyLines(AppSalesInvoice entity, AppSalesInvoiceEditViewModel vm)
    {
        foreach (var line in vm.Lines)
        {
            entity.Lines.Add(new AppSalesInvoiceLine
            {
                LineNo = line.LineNo,
                StockId = line.StockId,
                Description = string.IsNullOrWhiteSpace(line.Description) ? null : line.Description.Trim(),
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                VatRate = line.VatRate,
                WithholdingDefinitionId = line.WithholdingDefinitionId,
                WithholdingRate = line.WithholdingRate,
                NetAmount = line.NetAmount,
                VatAmount = line.VatAmount,
                WithholdingAmount = line.WithholdingAmount,
                NetVatAmount = line.NetVatAmount,
                TotalAmount = line.TotalAmount,
                CreatedDate = DateTime.UtcNow
            });
        }
    }

    private static string BuildAuditValue(AppSalesInvoice entity)
    {
        var sb = new StringBuilder();
        sb.Append($"No={entity.InvoiceNo}, Tarih={entity.InvoiceDate:yyyy-MM-dd}");
        sb.Append($", CariId={entity.AccountId}, SubeId={entity.WorkPlaceId}");
        sb.Append($", Doviz={entity.CurrencyCode}, Kur={entity.ExchangeRate}");
        sb.Append($", Tevkifat={entity.WithholdingAmount}");
        sb.Append($", ToplamDoviz={entity.TotalAmount}, ToplamTL={entity.TotalAmountTRY}");
        sb.Append($", SatirSayisi={entity.Lines.Count}");
        sb.Append($", IsActive={entity.IsActive == true}");
        return sb.ToString();
    }
}
