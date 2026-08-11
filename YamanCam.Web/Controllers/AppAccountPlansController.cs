using System.Text;
using YamanCam.Web.Data;
using YamanCam.Web.Models;
using YamanCam.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace YamanCam.Web.Controllers;

public class AppAccountPlansController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IAppLogService _appLogService;

    public AppAccountPlansController(ApplicationDbContext context, IAppLogService appLogService)
    {
        _context = context;
        _appLogService = appLogService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(bool showPassive = false)
    {
        var denied = EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        var query = _context.AppAccountPlans.AsNoTracking();

        if (showPassive)
        {
            query = query.Where(x => x.IsActive == false);
        }
        else
        {
            query = query.Where(x => x.IsActive != false);
        }

        var accounts = await query
            .OrderBy(x => x.AccountCode)
            .Select(x => new AppAccountPlanListItemViewModel
            {
                RecId = x.RecId,
                AccountCode = x.AccountCode,
                AccountName = x.AccountName,
                CurrencyCode = x.CurrencyCode,
                AccountType = x.AccountType,
                BalanceType = x.BalanceType,
                IsActive = x.IsActive != false
            })
            .ToListAsync();

        var balances = await BuildAccountBalancesAsync();

        foreach (var account in accounts)
        {
            account.Balance = balances.TryGetValue(account.RecId, out var balance) ? balance : 0m;
        }

        return View(new AppAccountPlanListViewModel
        {
            ShowPassive = showPassive,
            Items = accounts
        });
    }

    [HttpGet]
    public async Task<IActionResult> Extre(int id)
    {
        var denied = EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        var account = await _context.AppAccountPlans.AsNoTracking().FirstOrDefaultAsync(x => x.RecId == id);
        if (account is null)
        {
            return NotFound();
        }

        // Ust hesap ekstresi: kodu bu hesabin koduyla baslayan tum alt hesaplarin
        // (kendisi dahil) hareketlerini kapsar (bakiye hesaplamasiyla ayni mantik).
        var scopeAccountIds = await _context.AppAccountPlans.AsNoTracking()
            .Where(x => x.AccountCode.StartsWith(account.AccountCode))
            .Select(x => x.RecId)
            .ToListAsync();

        var lines = await _context.AppJournalVoucherLines.AsNoTracking()
            .Where(l => scopeAccountIds.Contains(l.AccountId) && l.Voucher != null && l.Voucher.IsActive != false)
            .OrderBy(l => l.Voucher!.VoucherDate)
            .ThenBy(l => l.Voucher!.RecId)
            .ThenBy(l => l.LineNo)
            .Select(l => new
            {
                l.Voucher!.VoucherDate,
                l.Voucher.VoucherType,
                l.Voucher.RecId,
                l.Voucher.VoucherNo,
                CompanyName = l.Voucher.Company != null ? l.Voucher.Company.CompanyName : null,
                WorkPlaceName = l.Voucher.WorkPlace != null ? l.Voucher.WorkPlace.WorkPlaceName : null,
                LineDescription = l.Description,
                VoucherDescription = l.Voucher.Description,
                LineAccountCode = l.Account != null ? l.Account.AccountCode : null,
                LineAccountName = l.Account != null ? l.Account.AccountName : null,
                l.DebitAmount,
                l.CreditAmount
            })
            .ToListAsync();

        var items = new List<AppAccountStatementItemViewModel>();
        var running = 0m;
        foreach (var line in lines)
        {
            running += line.DebitAmount - line.CreditAmount;
            items.Add(new AppAccountStatementItemViewModel
            {
                VoucherDate = line.VoucherDate,
                VoucherType = line.VoucherType,
                VoucherTypeLabel = GetVoucherTypeLabel(line.VoucherType),
                VoucherControllerName = GetVoucherControllerName(line.VoucherType),
                VoucherRecId = line.RecId,
                VoucherNo = line.VoucherNo,
                CompanyName = line.CompanyName,
                WorkPlaceName = line.WorkPlaceName,
                AccountCode = line.LineAccountCode ?? account.AccountCode,
                AccountName = line.LineAccountName ?? account.AccountName,
                Description = line.LineDescription ?? line.VoucherDescription,
                DebitAmount = line.DebitAmount,
                CreditAmount = line.CreditAmount,
                RunningBalance = running
            });
        }

        return View(new AppAccountStatementViewModel
        {
            AccountId = account.RecId,
            AccountCode = account.AccountCode,
            AccountName = account.AccountName,
            IsGroupAccount = scopeAccountIds.Count > 1,
            TotalDebit = items.Sum(x => x.DebitAmount),
            TotalCredit = items.Sum(x => x.CreditAmount),
            ClosingBalance = running,
            Items = items
        });
    }

    private static string GetVoucherTypeLabel(string voucherType) => voucherType switch
    {
        AppJournalVoucherTypes.Tediye => "Tediye Fişi",
        AppJournalVoucherTypes.Tahsilat => "Tahsilat Fişi",
        _ => "Mahsup Fişi"
    };

    private static string GetVoucherControllerName(string voucherType) => voucherType switch
    {
        AppJournalVoucherTypes.Tediye => "AppPaymentVouchers",
        AppJournalVoucherTypes.Tahsilat => "AppReceiptVouchers",
        _ => "AppJournalVouchers"
    };

    /// <summary>
    /// Her hesabın bakiyesini, kodu bu hesabın kodu ile başlayan tüm hesapların (kendisi dahil)
    /// doğrudan hareketlerini toplayarak hesaplar. Üst hesap - alt hesap ilişkisi ParentAccountId
    /// kolonuna değil, hesap kodu önekine (ör. "100" hesabı "100.01", "100.02.01" ... hesaplarını
    /// kapsar; "1" hesabı "10", "100", "100.01" ... hepsini kapsar) dayandığı için, ParentAccountId
    /// eksik/eski kayıtlarda boş olsa bile üst hesap bakiyeleri doğru hesaplanır.
    /// </summary>
    private async Task<Dictionary<int, decimal>> BuildAccountBalancesAsync()
    {
        var directBalances = await _context.AppJournalVoucherLines.AsNoTracking()
            .Where(l => l.Voucher != null && l.Voucher.IsActive != false)
            .GroupBy(l => l.AccountId)
            .Select(g => new { AccountId = g.Key, Balance = g.Sum(x => x.DebitAmount) - g.Sum(x => x.CreditAmount) })
            .ToListAsync();

        var accounts = await _context.AppAccountPlans.AsNoTracking()
            .Select(x => new { x.RecId, x.AccountCode })
            .ToListAsync();

        var codeByAccountId = accounts.ToDictionary(x => x.RecId, x => x.AccountCode);

        var balancesByCode = directBalances
            .Where(x => codeByAccountId.ContainsKey(x.AccountId))
            .Select(x => new { Code = codeByAccountId[x.AccountId], x.Balance })
            .ToList();

        var totals = new Dictionary<int, decimal>();
        foreach (var account in accounts)
        {
            totals[account.RecId] = balancesByCode
                .Where(x => x.Code.StartsWith(account.AccountCode, StringComparison.Ordinal))
                .Sum(x => x.Balance);
        }

        return totals;
    }

    [HttpGet]
    public IActionResult Create()
    {
        var denied = EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        return View("Edit", new AppAccountPlanEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppAccountPlanEditViewModel vm)
    {
        var denied = EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        vm.RecId = 0;
        NormalizeCheckboxes(vm);
        await ValidateAccountPlanAsync(vm);

        if (!ModelState.IsValid)
        {
            return View("Edit", vm);
        }

        var account = MapToEntity(vm);
        account.CreatedDate = DateTime.UtcNow;
        await ApplyHierarchyAsync(account);
        await ApplyDetailFlagAsync(account);
        _context.AppAccountPlans.Add(account);
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "AccountPlan",
            "Created",
            $"Hesap planı oluşturuldu. Code={account.AccountCode}",
            newValue: BuildAuditValue(account));

        TempData["SuccessMessage"] = "Hesap planı kaydedildi.";
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

        var account = await _context.AppAccountPlans.AsNoTracking().FirstOrDefaultAsync(x => x.RecId == id);
        if (account is null)
        {
            return NotFound();
        }

        return View(MapToViewModel(account));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AppAccountPlanEditViewModel vm)
    {
        var denied = EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        if (id != vm.RecId)
        {
            return NotFound();
        }

        NormalizeCheckboxes(vm);
        var account = await _context.AppAccountPlans.FirstOrDefaultAsync(x => x.RecId == id);
        if (account is null)
        {
            return NotFound();
        }

        var oldAudit = BuildAuditValue(account);
        await ValidateAccountPlanAsync(vm);

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        ApplyViewModel(account, vm);
        await ApplyHierarchyAsync(account);
        await ApplyDetailFlagAsync(account);
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "AccountPlan",
            "Updated",
            $"Hesap planı güncellendi. Code={account.AccountCode}",
            oldValue: oldAudit,
            newValue: BuildAuditValue(account));

        TempData["SuccessMessage"] = "Hesap planı güncellendi.";
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

        var account = await _context.AppAccountPlans.FirstOrDefaultAsync(x => x.RecId == id);
        if (account is null)
        {
            return NotFound();
        }

        var hasChildren = await _context.AppAccountPlans.AnyAsync(x => x.ParentAccountId == id);
        if (hasChildren)
        {
            TempData["ErrorMessage"] = $"{account.AccountCode} hesabı silinemez. Önce alt hesapları silin.";
            return RedirectToAction(nameof(Index));
        }

        var accountCode = account.AccountCode;
        var usedInVat = await _context.AppVatDefinitions.AnyAsync(x =>
            x.PurchaseAccountCode == accountCode
            || x.SalesAccountCode == accountCode
            || x.PurchaseReturnAccountCode == accountCode
            || x.SalesReturnAccountCode == accountCode);
        if (usedInVat)
        {
            TempData["ErrorMessage"] = $"{accountCode} hesabı KDV tanımlarında kullanıldığı için silinemez.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        var parentId = account.ParentAccountId;
        var oldAudit = BuildAuditValue(account);

        _context.AppAccountPlans.Remove(account);
        await _context.SaveChangesAsync();

        if (parentId.HasValue)
        {
            var parent = await _context.AppAccountPlans.FirstOrDefaultAsync(x => x.RecId == parentId.Value);
            if (parent is not null)
            {
                var parentHasChildren = await _context.AppAccountPlans.AnyAsync(x => x.ParentAccountId == parent.RecId);
                parent.IsDetail = !parentHasChildren;
                await _context.SaveChangesAsync();
            }
        }

        await _appLogService.WriteInfoAsync(
            "AccountPlan",
            "Deleted",
            $"Hesap planı silindi. Code={accountCode}",
            oldValue: oldAudit);

        TempData["SuccessMessage"] = "Hesap silindi.";
        return RedirectToAction(nameof(Index));
    }

    private IActionResult? EnsureAdmin()
    {
        if (!string.Equals(User.FindFirst("IsRight")?.Value, "true", StringComparison.Ordinal))
        {
            return RedirectToAction("Index", "Home");
        }

        return null;
    }

    private void NormalizeCheckboxes(AppAccountPlanEditViewModel vm)
    {
        vm.IsActive = string.Equals(Request.Form["IsActive"], "true", StringComparison.Ordinal);
    }

    private async Task ValidateAccountPlanAsync(AppAccountPlanEditViewModel vm)
    {
        var code = AccountPlanCodeHelper.NormalizeCode(vm.AccountCode);
        vm.AccountCode = code;

        var codeExists = await _context.AppAccountPlans.AnyAsync(x =>
            x.AccountCode == code && x.RecId != vm.RecId);
        if (codeExists)
        {
            ModelState.AddModelError(nameof(vm.AccountCode), "Bu hesap kodu zaten kullanılıyor.");
        }

        var level = AccountPlanCodeHelper.GetLevelNo(code);
        var parentCode = AccountPlanCodeHelper.GetParentAccountCode(code);
        if (level > 1 && !string.IsNullOrEmpty(parentCode))
        {
            var parentExists = await _context.AppAccountPlans.AnyAsync(x =>
                x.AccountCode == parentCode && x.RecId != vm.RecId);
            if (!parentExists)
            {
                ModelState.AddModelError(
                    nameof(vm.AccountCode),
                    $"Üst hesap bulunamadı: {parentCode}. Önce üst hesabı tanımlayın.");
            }
        }
    }

    private async Task ApplyHierarchyAsync(AppAccountPlan account)
    {
        var code = AccountPlanCodeHelper.NormalizeCode(account.AccountCode);
        account.AccountCode = code;
        account.LevelNo = AccountPlanCodeHelper.GetLevelNo(code);

        var parentCode = AccountPlanCodeHelper.GetParentAccountCode(code);
        if (string.IsNullOrEmpty(parentCode))
        {
            account.ParentAccountId = null;
            return;
        }

        var parent = await _context.AppAccountPlans.AsNoTracking()
            .FirstOrDefaultAsync(x => x.AccountCode == parentCode && x.RecId != account.RecId);

        account.ParentAccountId = parent?.RecId;
    }

    private async Task ApplyDetailFlagAsync(AppAccountPlan account)
    {
        if (account.RecId > 0)
        {
            var hasChildren = await _context.AppAccountPlans.AnyAsync(x => x.ParentAccountId == account.RecId);
            account.IsDetail = !hasChildren;
        }
        else
        {
            account.IsDetail = true;
        }

        if (!account.ParentAccountId.HasValue)
        {
            return;
        }

        var parent = await _context.AppAccountPlans.FirstOrDefaultAsync(x => x.RecId == account.ParentAccountId.Value);
        if (parent is not null)
        {
            parent.IsDetail = false;
        }
    }

    private static AppAccountPlanEditViewModel MapToViewModel(AppAccountPlan account)
    {
        return new AppAccountPlanEditViewModel
        {
            RecId = account.RecId,
            AccountCode = account.AccountCode,
            AccountName = account.AccountName,
            CurrencyCode = account.CurrencyCode,
            AccountType = account.AccountType,
            BalanceType = account.BalanceType,
            SpecialCode = account.SpecialCode,
            Tax = account.Tax,
            TaxNo = account.TaxNo,
            Address = account.Address,
            City = account.City,
            Country = account.Country,
            EMail = account.EMail,
            Person = account.Person,
            Tel = account.Tel,
            Fax = account.Fax,
            Gsm = account.Gsm,
            IsActive = account.IsActive != false
        };
    }

    private static AppAccountPlan MapToEntity(AppAccountPlanEditViewModel vm)
    {
        var account = new AppAccountPlan();
        ApplyViewModel(account, vm);
        return account;
    }

    private static void ApplyViewModel(AppAccountPlan account, AppAccountPlanEditViewModel vm)
    {
        account.AccountCode = AccountPlanCodeHelper.NormalizeCode(vm.AccountCode);
        account.AccountName = vm.AccountName.Trim();
        account.CurrencyCode = vm.CurrencyCode.Trim().ToUpperInvariant();
        account.AccountType = vm.AccountType?.Trim();
        account.BalanceType = string.IsNullOrWhiteSpace(vm.BalanceType) ? null : vm.BalanceType.Trim().ToUpperInvariant();
        account.SpecialCode = TrimOrNull(vm.SpecialCode);
        account.Tax = TrimOrNull(vm.Tax);
        account.TaxNo = TrimOrNull(vm.TaxNo);
        account.Address = TrimOrNull(vm.Address);
        account.City = TrimOrNull(vm.City);
        account.Country = TrimOrNull(vm.Country);
        account.EMail = TrimOrNull(vm.EMail);
        account.Person = TrimOrNull(vm.Person);
        account.Tel = TrimOrNull(vm.Tel);
        account.Fax = TrimOrNull(vm.Fax);
        account.Gsm = TrimOrNull(vm.Gsm);
        account.IsActive = vm.IsActive;
    }

    private static string? TrimOrNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string BuildAuditValue(AppAccountPlan account)
    {
        var sb = new StringBuilder();
        sb.Append($"Code={account.AccountCode}, Name={account.AccountName}, Currency={account.CurrencyCode}");
        sb.Append($", Level={account.LevelNo}, ParentId={account.ParentAccountId?.ToString() ?? "-"}");
        sb.Append($", Type={account.AccountType ?? "-"}, Balance={account.BalanceType ?? "-"}");
        sb.Append($", SpecialCode={account.SpecialCode ?? "-"}, TaxNo={account.TaxNo ?? "-"}, Person={account.Person ?? "-"}");
        sb.Append($", IsDetail={account.IsDetail == true}, IsActive={account.IsActive == true}");
        return sb.ToString();
    }
}
