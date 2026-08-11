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
/// Mahsup / Tediye / Tahsilat fişleri App_JournalVoucher tablosunu VoucherType alanıyla
/// paylaşır. Ortak liste/kayıt/silme/geri yükleme mantığı burada, ekrana özel etiketler
/// somut controller'larda tanımlanır.
///
/// Mahsup Fişi'nde her satır serbestçe borç ya da alacak hesabı olabilir (çift taraflı
/// mahsup). Tediye/Tahsilat Fişi'nde ise tek bir Kasa/Banka hesabı sabittir; satırlar
/// yalnızca karşı hesap + tutar içerir, borç/alacak yönü fiş tipine göre otomatik
/// belirlenir (Tahsilat: Kasa/Banka borçlanır, karşı hesap alacaklanır. Tediye: tersi).
/// </summary>
public abstract class AppVoucherControllerBase : Controller
{
    private static readonly string[] AllowedCurrencyCodes = { "TRY", "USD", "EUR", "GBP" };

    private readonly ApplicationDbContext _context;
    private readonly IAppLogService _appLogService;

    protected AppVoucherControllerBase(ApplicationDbContext context, IAppLogService appLogService)
    {
        _context = context;
        _appLogService = appLogService;
    }

    protected abstract string VoucherType { get; }
    protected abstract string EntityLabel { get; }
    protected abstract string EntityLabelPlural { get; }
    protected virtual bool RequiresCashBankAccount => false;
    private bool IsTahsilat => VoucherType == AppJournalVoucherTypes.Tahsilat;

    [HttpGet]
    public async Task<IActionResult> Index(bool showDeleted = false)
    {
        var denied = EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        SetLabels();

        var query = _context.AppJournalVouchers
            .AsNoTracking()
            .Where(x => x.VoucherType == VoucherType);

        query = showDeleted
            ? query.Where(x => x.IsActive == false)
            : query.Where(x => x.IsActive != false);

        var items = await query
            .OrderByDescending(x => x.VoucherDate)
            .ThenByDescending(x => x.RecId)
            .Select(x => new AppJournalVoucherListItemViewModel
            {
                RecId = x.RecId,
                VoucherNo = x.VoucherNo,
                VoucherDate = x.VoucherDate,
                CompanyName = x.Company != null ? x.Company.CompanyName : null,
                WorkPlaceName = x.WorkPlace != null ? x.WorkPlace.WorkPlaceName : null,
                Description = x.Description,
                TotalDebit = x.TotalDebit,
                TotalCredit = x.TotalCredit,
                CurrencyCode = x.CurrencyCode
            })
            .ToListAsync();

        return View("VoucherIndex", new AppJournalVoucherListViewModel
        {
            ShowDeleted = showDeleted,
            Items = items
        });
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var denied = EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        SetLabels();
        var vm = new AppJournalVoucherEditViewModel();
        if (RequiresCashBankAccount)
        {
            vm.Lines = new List<AppJournalVoucherLineEditViewModel> { new() };
        }

        await PopulateSelectListsAsync(vm);
        return View("VoucherEdit", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppJournalVoucherEditViewModel vm)
    {
        var denied = EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        SetLabels();
        vm.RecId = 0;
        await ValidateVoucherAsync(vm);

        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync(vm);
            return View("VoucherEdit", vm);
        }

        var entity = new AppJournalVoucher
        {
            VoucherType = VoucherType,
            IsActive = true
        };
        ApplyViewModel(entity, vm);
        entity.CreatedDate = DateTime.UtcNow;
        ApplyLines(entity, vm);

        _context.AppJournalVouchers.Add(entity);
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            VoucherType,
            "Created",
            $"{EntityLabel} oluşturuldu. No={entity.VoucherNo}",
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = $"{EntityLabel} kaydedildi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var denied = EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        SetLabels();
        var entity = await _context.AppJournalVouchers
            .AsNoTracking()
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.RecId == id && x.VoucherType == VoucherType);

        if (entity is null)
        {
            return NotFound();
        }

        var vm = MapToViewModel(entity);
        await PopulateSelectListsAsync(vm);
        return View("VoucherEdit", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AppJournalVoucherEditViewModel vm)
    {
        var denied = EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        SetLabels();
        if (id != vm.RecId)
        {
            return NotFound();
        }

        var entity = await _context.AppJournalVouchers
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.RecId == id && x.VoucherType == VoucherType);

        if (entity is null)
        {
            return NotFound();
        }

        var oldAudit = BuildAuditValue(entity);
        await ValidateVoucherAsync(vm);

        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync(vm);
            return View("VoucherEdit", vm);
        }

        ApplyViewModel(entity, vm);
        _context.AppJournalVoucherLines.RemoveRange(entity.Lines);
        entity.Lines.Clear();
        ApplyLines(entity, vm);

        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            VoucherType,
            "Updated",
            $"{EntityLabel} güncellendi. No={entity.VoucherNo}",
            oldValue: oldAudit,
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = $"{EntityLabel} güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var denied = EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        var entity = await _context.AppJournalVouchers
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.RecId == id && x.VoucherType == VoucherType);

        if (entity is null)
        {
            return NotFound();
        }

        var oldAudit = BuildAuditValue(entity);
        entity.IsActive = false;
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            VoucherType,
            "Deleted",
            $"{EntityLabel} silindi. No={entity.VoucherNo}",
            oldValue: oldAudit,
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = $"{EntityLabel} silindi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id)
    {
        var denied = EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        var entity = await _context.AppJournalVouchers
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.RecId == id && x.VoucherType == VoucherType);

        if (entity is null)
        {
            return NotFound();
        }

        var oldAudit = BuildAuditValue(entity);
        entity.IsActive = true;
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            VoucherType,
            "Restored",
            $"{EntityLabel} geri yüklendi. No={entity.VoucherNo}",
            oldValue: oldAudit,
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = $"{EntityLabel} geri yüklendi.";
        return RedirectToAction(nameof(Index), new { showDeleted = true });
    }

    private void SetLabels()
    {
        ViewData["EntityLabel"] = EntityLabel;
        ViewData["EntityLabelPlural"] = EntityLabelPlural;
        ViewData["VoucherType"] = VoucherType;
        ViewData["RequiresCashBankAccount"] = RequiresCashBankAccount;
    }

    private IActionResult? EnsureAdmin()
    {
        if (!string.Equals(User.FindFirst("IsRight")?.Value, "true", StringComparison.Ordinal))
        {
            return RedirectToAction("Index", "Home");
        }

        return null;
    }

    private async Task PopulateSelectListsAsync(AppJournalVoucherEditViewModel vm)
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
            .Where(x => x.IsActive != false && x.IsDetail != false)
            .OrderBy(x => x.AccountCode)
            .Select(x => new { x.RecId, x.AccountCode, x.AccountName })
            .ToListAsync();

        vm.AccountOptions = accounts
            .Select(x => new SelectListItem($"{x.AccountCode} - {x.AccountName}", x.RecId.ToString(CultureInfo.InvariantCulture)))
            .ToList();
    }

    private async Task ValidateVoucherAsync(AppJournalVoucherEditViewModel vm)
    {
        vm.VoucherNo = vm.VoucherNo.Trim().ToUpperInvariant();
        vm.CurrencyCode = string.IsNullOrWhiteSpace(vm.CurrencyCode) ? "TRY" : vm.CurrencyCode.Trim().ToUpperInvariant();

        ModelState.Remove(nameof(vm.CurrencyCode));
        if (!AllowedCurrencyCodes.Contains(vm.CurrencyCode))
        {
            ModelState.AddModelError(nameof(vm.CurrencyCode), "Geçersiz döviz kodu.");
        }

        var codeExists = await _context.AppJournalVouchers.AnyAsync(x =>
            x.VoucherType == VoucherType && x.VoucherNo == vm.VoucherNo && x.RecId != vm.RecId);
        if (codeExists)
        {
            ModelState.AddModelError(nameof(vm.VoucherNo), "Bu fiş no zaten kullanılıyor.");
        }

        if (vm.CompanyId.HasValue &&
            !await _context.AppCompanies.AnyAsync(x => x.RecId == vm.CompanyId.Value))
        {
            ModelState.AddModelError(nameof(vm.CompanyId), "Geçersiz şirket seçildi.");
        }

        if (vm.WorkPlaceId.HasValue &&
            !await _context.AppWorkPlaces.AnyAsync(x => x.RecId == vm.WorkPlaceId.Value))
        {
            ModelState.AddModelError(nameof(vm.WorkPlaceId), "Geçersiz şube seçildi.");
        }

        if (RequiresCashBankAccount)
        {
            await ValidateCashBankLinesAsync(vm);
        }
        else
        {
            await ValidateJournalLinesAsync(vm);
        }
    }

    private async Task ValidateCashBankLinesAsync(AppJournalVoucherEditViewModel vm)
    {
        if (!vm.CashBankAccountId.HasValue || vm.CashBankAccountId.Value <= 0)
        {
            ModelState.AddModelError(nameof(vm.CashBankAccountId), "Kasa/Banka hesabı seçilmelidir.");
        }
        else if (!await _context.AppAccountPlans.AnyAsync(x => x.RecId == vm.CashBankAccountId.Value))
        {
            ModelState.AddModelError(nameof(vm.CashBankAccountId), "Geçersiz kasa/banka hesabı seçildi.");
        }

        vm.Lines = vm.Lines
            .Where(x => x.AccountId > 0 || x.DebitAmount != 0 || x.CreditAmount != 0)
            .ToList();

        if (vm.Lines.Count < 1)
        {
            ModelState.AddModelError(string.Empty, "En az 1 karşı hesap satırı girilmelidir.");
        }

        var amountField = IsTahsilat ? nameof(AppJournalVoucherLineEditViewModel.CreditAmount) : nameof(AppJournalVoucherLineEditViewModel.DebitAmount);

        for (var i = 0; i < vm.Lines.Count; i++)
        {
            var line = vm.Lines[i];
            line.LineNo = i + 1;
            var amount = IsTahsilat ? line.CreditAmount : line.DebitAmount;

            if (line.AccountId <= 0)
            {
                ModelState.AddModelError($"Lines[{i}].AccountId", "Karşı hesap seçilmelidir.");
            }
            else if (!await _context.AppAccountPlans.AnyAsync(x => x.RecId == line.AccountId))
            {
                ModelState.AddModelError($"Lines[{i}].AccountId", "Geçersiz hesap seçildi.");
            }
            else if (vm.CashBankAccountId.HasValue && line.AccountId == vm.CashBankAccountId.Value)
            {
                ModelState.AddModelError($"Lines[{i}].AccountId", "Karşı hesap, kasa/banka hesabından farklı olmalıdır.");
            }

            if (amount <= 0)
            {
                ModelState.AddModelError($"Lines[{i}].{amountField}", "Tutar sıfırdan büyük olmalıdır.");
            }
        }
    }

    private async Task ValidateJournalLinesAsync(AppJournalVoucherEditViewModel vm)
    {
        vm.Lines = vm.Lines
            .Where(x => x.AccountId > 0 || x.DebitAmount != 0 || x.CreditAmount != 0)
            .ToList();

        if (vm.Lines.Count < 2)
        {
            ModelState.AddModelError(string.Empty, "Fişte en az 2 satır bulunmalıdır.");
        }

        for (var i = 0; i < vm.Lines.Count; i++)
        {
            var line = vm.Lines[i];
            line.LineNo = i + 1;

            if (line.AccountId <= 0)
            {
                ModelState.AddModelError($"Lines[{i}].AccountId", "Hesap seçilmelidir.");
            }
            else if (!await _context.AppAccountPlans.AnyAsync(x => x.RecId == line.AccountId))
            {
                ModelState.AddModelError($"Lines[{i}].AccountId", "Geçersiz hesap seçildi.");
            }

            if (line.DebitAmount < 0 || line.CreditAmount < 0)
            {
                ModelState.AddModelError($"Lines[{i}].DebitAmount", "Tutar negatif olamaz.");
            }
            else if (line.DebitAmount == 0 && line.CreditAmount == 0)
            {
                ModelState.AddModelError($"Lines[{i}].DebitAmount", "Borç veya alacak tutarı girilmelidir.");
            }
            else if (line.DebitAmount != 0 && line.CreditAmount != 0)
            {
                ModelState.AddModelError($"Lines[{i}].DebitAmount", "Bir satırda hem borç hem alacak girilemez.");
            }
        }

        var totalDebit = vm.Lines.Sum(x => x.DebitAmount);
        var totalCredit = vm.Lines.Sum(x => x.CreditAmount);

        if (totalDebit == 0 && totalCredit == 0)
        {
            ModelState.AddModelError(string.Empty, "Fiş tutarı sıfır olamaz.");
        }
        else if (totalDebit != totalCredit)
        {
            ModelState.AddModelError(
                string.Empty,
                $"Fiş dengede değil. Toplam Borç: {totalDebit.ToString("N2", CultureInfo.InvariantCulture)}, Toplam Alacak: {totalCredit.ToString("N2", CultureInfo.InvariantCulture)}");
        }
    }

    private AppJournalVoucherEditViewModel MapToViewModel(AppJournalVoucher entity)
    {
        var vm = new AppJournalVoucherEditViewModel
        {
            RecId = entity.RecId,
            VoucherNo = entity.VoucherNo,
            VoucherDate = entity.VoucherDate,
            CompanyId = entity.CompanyId,
            WorkPlaceId = entity.WorkPlaceId,
            Description = entity.Description,
            CurrencyCode = entity.CurrencyCode
        };

        if (RequiresCashBankAccount)
        {
            var cashBankLine = entity.Lines.FirstOrDefault(x => IsTahsilat ? x.DebitAmount > 0 : x.CreditAmount > 0);
            vm.CashBankAccountId = cashBankLine?.AccountId;

            vm.Lines = entity.Lines
                .Where(x => cashBankLine is null || x.RecId != cashBankLine.RecId)
                .OrderBy(x => x.LineNo)
                .Select(x => new AppJournalVoucherLineEditViewModel
                {
                    RecId = x.RecId,
                    LineNo = x.LineNo,
                    AccountId = x.AccountId,
                    Description = x.Description,
                    DebitAmount = x.DebitAmount,
                    CreditAmount = x.CreditAmount
                })
                .ToList();
        }
        else
        {
            vm.Lines = entity.Lines
                .OrderBy(x => x.LineNo)
                .Select(x => new AppJournalVoucherLineEditViewModel
                {
                    RecId = x.RecId,
                    LineNo = x.LineNo,
                    AccountId = x.AccountId,
                    Description = x.Description,
                    DebitAmount = x.DebitAmount,
                    CreditAmount = x.CreditAmount
                })
                .ToList();
        }

        return vm;
    }

    private void ApplyViewModel(AppJournalVoucher entity, AppJournalVoucherEditViewModel vm)
    {
        entity.VoucherNo = vm.VoucherNo.Trim().ToUpperInvariant();
        entity.VoucherDate = vm.VoucherDate;
        entity.CompanyId = vm.CompanyId;
        entity.WorkPlaceId = vm.WorkPlaceId;
        entity.Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim();
        entity.CurrencyCode = vm.CurrencyCode;

        if (RequiresCashBankAccount)
        {
            var counterTotal = vm.Lines.Sum(x => IsTahsilat ? x.CreditAmount : x.DebitAmount);
            entity.TotalDebit = counterTotal;
            entity.TotalCredit = counterTotal;
        }
        else
        {
            entity.TotalDebit = vm.Lines.Sum(x => x.DebitAmount);
            entity.TotalCredit = vm.Lines.Sum(x => x.CreditAmount);
        }
    }

    private void ApplyLines(AppJournalVoucher entity, AppJournalVoucherEditViewModel vm)
    {
        if (RequiresCashBankAccount)
        {
            var counterTotal = vm.Lines.Sum(x => IsTahsilat ? x.CreditAmount : x.DebitAmount);

            entity.Lines.Add(new AppJournalVoucherLine
            {
                LineNo = 1,
                AccountId = vm.CashBankAccountId!.Value,
                Description = "Kasa/Banka",
                DebitAmount = IsTahsilat ? counterTotal : 0,
                CreditAmount = IsTahsilat ? 0 : counterTotal,
                CreatedDate = DateTime.UtcNow
            });

            var lineNo = 2;
            foreach (var line in vm.Lines)
            {
                var amount = IsTahsilat ? line.CreditAmount : line.DebitAmount;
                entity.Lines.Add(new AppJournalVoucherLine
                {
                    LineNo = lineNo++,
                    AccountId = line.AccountId,
                    Description = string.IsNullOrWhiteSpace(line.Description) ? null : line.Description.Trim(),
                    DebitAmount = IsTahsilat ? 0 : amount,
                    CreditAmount = IsTahsilat ? amount : 0,
                    CreatedDate = DateTime.UtcNow
                });
            }

            return;
        }

        foreach (var line in vm.Lines)
        {
            entity.Lines.Add(new AppJournalVoucherLine
            {
                LineNo = line.LineNo,
                AccountId = line.AccountId,
                Description = string.IsNullOrWhiteSpace(line.Description) ? null : line.Description.Trim(),
                DebitAmount = line.DebitAmount,
                CreditAmount = line.CreditAmount,
                CreatedDate = DateTime.UtcNow
            });
        }
    }

    private static string BuildAuditValue(AppJournalVoucher entity)
    {
        var sb = new StringBuilder();
        sb.Append($"Tip={entity.VoucherType}, No={entity.VoucherNo}, Tarih={entity.VoucherDate:yyyy-MM-dd}");
        sb.Append($", ToplamBorc={entity.TotalDebit}, ToplamAlacak={entity.TotalCredit}");
        sb.Append($", SatirSayisi={entity.Lines.Count}");
        sb.Append($", IsActive={entity.IsActive == true}");
        return sb.ToString();
    }
}
