using System.Globalization;
using System.Text;
using YamanCam.Web.Data;
using YamanCam.Web.Models;
using YamanCam.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace YamanCam.Web.Controllers;

public class AppStocksController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IAppLogService _appLogService;
    private readonly IUserRightService _userRightService;

    public AppStocksController(ApplicationDbContext context, IAppLogService appLogService, IUserRightService userRightService)
    {
        _context = context;
        _appLogService = appLogService;
        _userRightService = userRightService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(bool showPassive = false)
    {
        var denied = await EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        var query = _context.AppStocks.AsNoTracking();

        if (showPassive)
        {
            query = query.Where(x => x.IsActive == false);
        }
        else
        {
            query = query.Where(x => x.IsActive != false);
        }

        var items = await query
            .OrderBy(x => x.StockCode)
            .Select(x => new AppStockListItemViewModel
            {
                RecId = x.RecId,
                StockCode = x.StockCode,
                StockName = x.StockName,
                StockGroupName = x.StockGroup != null ? x.StockGroup.GroupName : null,
                StockUnitCode = x.StockUnit != null ? x.StockUnit.UnitCode : null,
                PurchaseVatLabel = x.PurchaseVat != null ? x.PurchaseVat.VatCode : null,
                SalesVatLabel = x.SalesVat != null ? x.SalesVat.VatCode : null,
                Barcode = x.Barcode,
                StockType = x.StockType,
                ProductionWeight = x.ProductionWeight,
                IsActive = x.IsActive != false
            })
            .ToListAsync();

        return View(new AppStockListViewModel
        {
            ShowPassive = showPassive,
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

        var vm = new AppStockEditViewModel();
        await PopulateSelectListsAsync(vm);
        return View("Edit", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppStockEditViewModel vm)
    {
        var denied = await EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        vm.RecId = 0;
        NormalizeCheckboxes(vm);
        await ValidateStockAsync(vm);

        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync(vm);
            return View("Edit", vm);
        }

        var entity = MapToEntity(vm);
        entity.CreatedDate = DateTime.UtcNow;
        _context.AppStocks.Add(entity);
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "Stock",
            "Created",
            $"Stok oluşturuldu. Code={entity.StockCode}",
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = "Stok kaydedildi.";
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

        var entity = await _context.AppStocks.AsNoTracking().FirstOrDefaultAsync(x => x.RecId == id);
        if (entity is null)
        {
            return NotFound();
        }

        var vm = MapToViewModel(entity);
        await PopulateSelectListsAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AppStockEditViewModel vm)
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

        NormalizeCheckboxes(vm);
        var entity = await _context.AppStocks.FirstOrDefaultAsync(x => x.RecId == id);
        if (entity is null)
        {
            return NotFound();
        }

        var oldAudit = BuildAuditValue(entity);
        await ValidateStockAsync(vm);

        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync(vm);
            return View(vm);
        }

        ApplyViewModel(entity, vm);
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "Stock",
            "Updated",
            $"Stok güncellendi. Code={entity.StockCode}",
            oldValue: oldAudit,
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = "Stok güncellendi.";
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

        var entity = await _context.AppStocks.FirstOrDefaultAsync(x => x.RecId == id);
        if (entity is null)
        {
            return NotFound();
        }

        var stockCode = entity.StockCode;
        var oldAudit = BuildAuditValue(entity);

        _context.AppStocks.Remove(entity);
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "Stock",
            "Deleted",
            $"Stok silindi. Code={stockCode}",
            oldValue: oldAudit);

        TempData["SuccessMessage"] = "Stok silindi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id, int? workPlaceId, string? transactionType)
    {
        var denied = await EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        var stock = await _context.AppStocks.AsNoTracking()
            .Include(x => x.StockGroup)
            .Include(x => x.StockUnit)
            .FirstOrDefaultAsync(x => x.RecId == id);

        if (stock is null)
        {
            return NotFound();
        }

        var movements = await BuildStockMovementsAsync(id);

        var branchSummaries = movements
            .GroupBy(x => x.WorkPlaceName)
            .Select(g => new AppStockDetailBranchSummaryViewModel
            {
                WorkPlaceName = g.Key,
                TotalIn = g.Where(x => x.Quantity > 0).Sum(x => x.Quantity),
                TotalOut = g.Where(x => x.Quantity < 0).Sum(x => x.Quantity),
                Balance = g.Sum(x => x.Quantity)
            })
            .OrderBy(x => x.WorkPlaceName)
            .ToList();

        var filtered = movements.AsEnumerable();
        if (workPlaceId.HasValue)
        {
            filtered = filtered.Where(x => x.WorkPlaceId == workPlaceId.Value);
        }

        if (!string.IsNullOrWhiteSpace(transactionType))
        {
            filtered = filtered.Where(x => x.TransactionType == transactionType);
        }

        var branchGroups = filtered
            .GroupBy(x => new { x.WorkPlaceId, x.WorkPlaceName })
            .OrderBy(g => g.Key.WorkPlaceName)
            .Select(g =>
            {
                var branchLines = g
                    .OrderBy(x => x.TransactionDate)
                    .ThenBy(x => AppStockMovementTypes.GetSortOrder(x.TransactionType))
                    .ThenBy(x => x.SourceRecId)
                    .ToList();

                ComputeRunningTotals(branchLines);

                return new AppStockDetailBranchGroupViewModel
                {
                    WorkPlaceName = g.Key.WorkPlaceName,
                    Lines = branchLines
                };
            })
            .ToList();

        var workPlaces = await _context.AppWorkPlaces.AsNoTracking()
            .Where(x => x.IsActive != false)
            .OrderBy(x => x.WorkPlaceCode)
            .Select(x => new { x.RecId, x.WorkPlaceCode, x.WorkPlaceName })
            .ToListAsync();

        var vm = new AppStockDetailViewModel
        {
            StockId = stock.RecId,
            StockCode = stock.StockCode,
            StockName = stock.StockName,
            StockGroupName = stock.StockGroup?.GroupName,
            StockUnitCode = stock.StockUnit?.UnitCode,
            WorkPlaceId = workPlaceId,
            TransactionType = transactionType,
            WorkPlaceOptions = workPlaces
                .Select(x => new SelectListItem($"{x.WorkPlaceCode} - {x.WorkPlaceName}", x.RecId.ToString(CultureInfo.InvariantCulture)))
                .ToList(),
            TransactionTypeOptions = new List<SelectListItem>
            {
                new(AppStockMovementTypes.Opening, AppStockMovementTypes.Opening),
                new(AppStockMovementTypes.Purchase, AppStockMovementTypes.Purchase),
                new(AppStockMovementTypes.SalesReturn, AppStockMovementTypes.SalesReturn),
                new(AppStockMovementTypes.TransferIn, AppStockMovementTypes.TransferIn),
                new(AppStockMovementTypes.ProductionIn, AppStockMovementTypes.ProductionIn),
                new(AppStockMovementTypes.TransferOut, AppStockMovementTypes.TransferOut),
                new(AppStockMovementTypes.PurchaseReturn, AppStockMovementTypes.PurchaseReturn),
                new(AppStockMovementTypes.Sales, AppStockMovementTypes.Sales),
                new(AppStockMovementTypes.ProductionOut, AppStockMovementTypes.ProductionOut),
                new(AppStockMovementTypes.Issue, AppStockMovementTypes.Issue)
            },
            BranchSummaries = branchSummaries,
            BranchGroups = branchGroups
        };

        return View(vm);
    }

    /// <summary>
    /// Stok hareketlerini Açılış Fişi, Alış Faturası, Alış İade Faturası, Satış Faturası,
    /// Satış İade Faturası, Stok Çıkış Fişi, Şubeler Arası Transfer Fişi ve Stok Ürün
    /// Üretim Fişi satırlarından hesaplar. Alış satırlarına, ilgili faturaya bağlı Gümrük
    /// Nakliye faturalarının (varsa) net tutarı, fatura satırlarının net tutar payı
    /// oranında dağıtılarak maliyete eklenir. Satış, Alış İade ve Çıkış Fişi satırları
    /// stoktan çıkışı ifade ettiği için miktar ve tutar negatif tutulur. Satış İade
    /// Faturası stoğa geri giriş olduğu için Alış ile aynı mantıkla (pozitif
    /// miktar/tutar), Alış İade Faturası ise stoktan çıkış olduğu için Satış ile aynı
    /// mantıkla (negatif miktar/tutar) ele alınır. Transfer fişleri çıkan şubede negatif,
    /// giren şubede pozitif olmak üzere iki ayrı hareket üretir. Üretim Fişi satırları da
    /// benzer şekilde iki ayrı hareket üretir: tüketilen hammadde negatif (Çıkış Fişi
    /// mantığı), üretilen mamül pozitif (Alış mantığı).
    /// </summary>
    private async Task<List<AppStockDetailLineViewModel>> BuildStockMovementsAsync(int stockId)
    {
        var movements = new List<AppStockDetailLineViewModel>();

        var openingLines = await _context.AppStockOpeningLines
            .AsNoTracking()
            .Include(l => l.Opening).ThenInclude(o => o!.WorkPlace)
            .Where(l => l.StockId == stockId && l.Opening != null && l.Opening.IsActive != false)
            .ToListAsync();

        foreach (var line in openingLines)
        {
            var opening = line.Opening!;

            movements.Add(new AppStockDetailLineViewModel
            {
                WorkPlaceId = opening.WorkPlaceId ?? 0,
                WorkPlaceName = opening.WorkPlace?.WorkPlaceName ?? "-",
                TransactionDate = opening.VoucherDate,
                DocumentNo = opening.VoucherNo,
                TransactionType = AppStockMovementTypes.Opening,
                AccountName = opening.TransactionType,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                NetAmount = line.TotalPrice,
                CurrencyCode = "TRY",
                ExchangeRate = 1m,
                CurrencyUnitPrice = line.UnitPrice,
                CurrencyNetAmount = line.TotalPrice,
                SourceRecId = opening.RecId
            });
        }

        var purchaseLines = await _context.AppPurchaseInvoiceLines
            .AsNoTracking()
            .Include(l => l.Invoice).ThenInclude(i => i!.WorkPlace)
            .Include(l => l.Invoice).ThenInclude(i => i!.Account)
            .Where(l => l.StockId == stockId && l.Invoice != null && l.Invoice.IsActive != false && l.Invoice.IsReturn != true)
            .ToListAsync();

        var purchaseInvoiceIds = purchaseLines.Select(l => l.InvoiceId).Distinct().ToList();

        var freightByInvoice = await _context.AppCustomsFreightInvoices
            .AsNoTracking()
            .Where(f => f.LinkedPurchaseInvoiceId != null
                && purchaseInvoiceIds.Contains(f.LinkedPurchaseInvoiceId.Value)
                && f.IsActive != false)
            .GroupBy(f => f.LinkedPurchaseInvoiceId!.Value)
            .Select(g => new { InvoiceId = g.Key, TotalFreight = g.Sum(x => x.NetAmount) })
            .ToDictionaryAsync(x => x.InvoiceId, x => x.TotalFreight);

        var invoiceNetTotals = purchaseLines
            .GroupBy(l => l.InvoiceId)
            .ToDictionary(g => g.Key, g => g.Sum(l => l.NetAmount));

        foreach (var line in purchaseLines)
        {
            var invoice = line.Invoice!;
            var invoiceNetTotal = invoiceNetTotals.TryGetValue(line.InvoiceId, out var netTotal) ? netTotal : 0m;
            var freightTotal = freightByInvoice.TryGetValue(line.InvoiceId, out var freight) ? freight : 0m;
            var allocatedFreight = invoiceNetTotal != 0 && freightTotal != 0
                ? Math.Round(line.NetAmount / invoiceNetTotal * freightTotal, 10)
                : 0m;

            var effectiveNetAmount = line.NetAmount + allocatedFreight;
            var effectiveUnitPrice = line.Quantity != 0 ? effectiveNetAmount / line.Quantity : line.UnitPrice;

            movements.Add(new AppStockDetailLineViewModel
            {
                WorkPlaceId = invoice.WorkPlaceId ?? 0,
                WorkPlaceName = invoice.WorkPlace?.WorkPlaceName ?? "-",
                TransactionDate = invoice.InvoiceDate,
                DocumentNo = invoice.InvoiceNo,
                TransactionType = AppStockMovementTypes.Purchase,
                AccountName = invoice.Account?.AccountName,
                Quantity = line.Quantity,
                UnitPrice = effectiveUnitPrice,
                NetAmount = effectiveNetAmount,
                CurrencyCode = invoice.CurrencyCode,
                ExchangeRate = invoice.ExchangeRate,
                CurrencyUnitPrice = invoice.ExchangeRate != 0 ? effectiveUnitPrice / invoice.ExchangeRate : effectiveUnitPrice,
                CurrencyNetAmount = invoice.ExchangeRate != 0 ? effectiveNetAmount / invoice.ExchangeRate : effectiveNetAmount,
                SourceRecId = invoice.RecId
            });
        }

        /* Alış İade Faturası: stoktan çıkış olduğu için Satış ile aynı mantıkla
           (negatif miktar, o ana kadarki ortalama maliyet üzerinden düşülen tutar)
           ele alınır. Gümrük Nakliye tarzı bir maliyet dağıtımı söz konusu değildir. */
        var purchaseReturnLines = await _context.AppPurchaseInvoiceLines
            .AsNoTracking()
            .Include(l => l.Invoice).ThenInclude(i => i!.WorkPlace)
            .Include(l => l.Invoice).ThenInclude(i => i!.Account)
            .Where(l => l.StockId == stockId && l.Invoice != null && l.Invoice.IsActive != false && l.Invoice.IsReturn == true)
            .ToListAsync();

        foreach (var line in purchaseReturnLines)
        {
            var invoice = line.Invoice!;

            movements.Add(new AppStockDetailLineViewModel
            {
                WorkPlaceId = invoice.WorkPlaceId ?? 0,
                WorkPlaceName = invoice.WorkPlace?.WorkPlaceName ?? "-",
                TransactionDate = invoice.InvoiceDate,
                DocumentNo = invoice.InvoiceNo,
                TransactionType = AppStockMovementTypes.PurchaseReturn,
                AccountName = invoice.Account?.AccountName,
                Quantity = -line.Quantity,
                UnitPrice = line.UnitPrice,
                NetAmount = -line.NetAmount,
                CurrencyCode = invoice.CurrencyCode,
                ExchangeRate = invoice.ExchangeRate,
                CurrencyUnitPrice = invoice.ExchangeRate != 0 ? line.UnitPrice / invoice.ExchangeRate : line.UnitPrice,
                CurrencyNetAmount = invoice.ExchangeRate != 0 ? -line.NetAmount / invoice.ExchangeRate : -line.NetAmount,
                SourceRecId = invoice.RecId
            });
        }

        var salesLines = await _context.AppSalesInvoiceLines
            .AsNoTracking()
            .Include(l => l.Invoice).ThenInclude(i => i!.WorkPlace)
            .Include(l => l.Invoice).ThenInclude(i => i!.Account)
            .Where(l => l.StockId == stockId && l.Invoice != null && l.Invoice.IsActive != false && l.Invoice.IsReturn != true)
            .ToListAsync();

        foreach (var line in salesLines)
        {
            var invoice = line.Invoice!;

            movements.Add(new AppStockDetailLineViewModel
            {
                WorkPlaceId = invoice.WorkPlaceId ?? 0,
                WorkPlaceName = invoice.WorkPlace?.WorkPlaceName ?? "-",
                TransactionDate = invoice.InvoiceDate,
                DocumentNo = invoice.InvoiceNo,
                TransactionType = AppStockMovementTypes.Sales,
                AccountName = invoice.Account?.AccountName,
                Quantity = -line.Quantity,
                UnitPrice = line.UnitPrice,
                NetAmount = -line.NetAmount,
                CurrencyCode = invoice.CurrencyCode,
                ExchangeRate = invoice.ExchangeRate,
                CurrencyUnitPrice = invoice.ExchangeRate != 0 ? line.UnitPrice / invoice.ExchangeRate : line.UnitPrice,
                CurrencyNetAmount = invoice.ExchangeRate != 0 ? -line.NetAmount / invoice.ExchangeRate : -line.NetAmount,
                SourceRecId = invoice.RecId
            });
        }

        /* Satış İade Faturası: stoğa geri giriş oldugu için Alış ile aynı mantıkla
           (pozitif miktar, maliyet havuzuna eklenen net tutar) ele alınır. Gümrük
           Nakliye tarzı bir maliyet dağıtımı söz konusu değildir. */
        var salesReturnLines = await _context.AppSalesInvoiceLines
            .AsNoTracking()
            .Include(l => l.Invoice).ThenInclude(i => i!.WorkPlace)
            .Include(l => l.Invoice).ThenInclude(i => i!.Account)
            .Where(l => l.StockId == stockId && l.Invoice != null && l.Invoice.IsActive != false && l.Invoice.IsReturn == true)
            .ToListAsync();

        foreach (var line in salesReturnLines)
        {
            var invoice = line.Invoice!;

            movements.Add(new AppStockDetailLineViewModel
            {
                WorkPlaceId = invoice.WorkPlaceId ?? 0,
                WorkPlaceName = invoice.WorkPlace?.WorkPlaceName ?? "-",
                TransactionDate = invoice.InvoiceDate,
                DocumentNo = invoice.InvoiceNo,
                TransactionType = AppStockMovementTypes.SalesReturn,
                AccountName = invoice.Account?.AccountName,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                NetAmount = line.NetAmount,
                CurrencyCode = invoice.CurrencyCode,
                ExchangeRate = invoice.ExchangeRate,
                CurrencyUnitPrice = invoice.ExchangeRate != 0 ? line.UnitPrice / invoice.ExchangeRate : line.UnitPrice,
                CurrencyNetAmount = invoice.ExchangeRate != 0 ? line.NetAmount / invoice.ExchangeRate : line.NetAmount,
                SourceRecId = invoice.RecId
            });
        }

        var issueLines = await _context.AppStockIssueLines
            .AsNoTracking()
            .Include(l => l.Issue).ThenInclude(i => i!.WorkPlace)
            .Where(l => l.StockId == stockId && l.Issue != null && l.Issue.IsActive != false)
            .ToListAsync();

        foreach (var line in issueLines)
        {
            var issue = line.Issue!;

            movements.Add(new AppStockDetailLineViewModel
            {
                WorkPlaceId = issue.WorkPlaceId ?? 0,
                WorkPlaceName = issue.WorkPlace?.WorkPlaceName ?? "-",
                TransactionDate = issue.VoucherDate,
                DocumentNo = issue.VoucherNo,
                TransactionType = AppStockMovementTypes.Issue,
                AccountName = issue.TransactionType,
                Quantity = -line.Quantity,
                UnitPrice = line.UnitPrice,
                NetAmount = -line.TotalPrice,
                CurrencyCode = "TRY",
                ExchangeRate = 1m,
                CurrencyUnitPrice = line.UnitPrice,
                CurrencyNetAmount = -line.TotalPrice,
                SourceRecId = issue.RecId
            });
        }

        var transferLines = await _context.AppStockTransferLines
            .AsNoTracking()
            .Include(l => l.Transfer).ThenInclude(t => t!.InWorkPlace)
            .Include(l => l.Transfer).ThenInclude(t => t!.OutWorkPlace)
            .Where(l => l.StockId == stockId && l.Transfer != null && l.Transfer.IsActive != false)
            .ToListAsync();

        foreach (var line in transferLines)
        {
            var transfer = line.Transfer!;
            var inWorkPlaceName = transfer.InWorkPlace?.WorkPlaceName ?? "-";
            var outWorkPlaceName = transfer.OutWorkPlace?.WorkPlaceName ?? "-";

            movements.Add(new AppStockDetailLineViewModel
            {
                WorkPlaceId = transfer.OutWorkPlaceId ?? 0,
                WorkPlaceName = outWorkPlaceName,
                TransactionDate = transfer.VoucherDate,
                DocumentNo = transfer.VoucherNo,
                TransactionType = AppStockMovementTypes.TransferOut,
                AccountName = $"→ {inWorkPlaceName}",
                Quantity = -line.Quantity,
                UnitPrice = line.UnitPrice,
                NetAmount = -line.TotalPrice,
                CurrencyCode = "TRY",
                ExchangeRate = 1m,
                CurrencyUnitPrice = line.UnitPrice,
                CurrencyNetAmount = -line.TotalPrice,
                SourceRecId = transfer.RecId
            });

            movements.Add(new AppStockDetailLineViewModel
            {
                WorkPlaceId = transfer.InWorkPlaceId ?? 0,
                WorkPlaceName = inWorkPlaceName,
                TransactionDate = transfer.VoucherDate,
                DocumentNo = transfer.VoucherNo,
                TransactionType = AppStockMovementTypes.TransferIn,
                AccountName = $"← {outWorkPlaceName}",
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                NetAmount = line.TotalPrice,
                CurrencyCode = "TRY",
                ExchangeRate = 1m,
                CurrencyUnitPrice = line.UnitPrice,
                CurrencyNetAmount = line.TotalPrice,
                SourceRecId = transfer.RecId
            });
        }

        /* Üretim Fişi: hammadde tüketimi Çıkış Fişi ile aynı mantıkla (negatif
           miktar/tutar), üretilen mamül ise Alış ile aynı mantıkla (pozitif
           miktar/tutar) ele alınır. Tarih olarak fişin Üretim Tarihi kullanılır. */
        var productionLines = await _context.AppProductionVoucherLines
            .AsNoTracking()
            .Include(l => l.Voucher).ThenInclude(v => v!.WorkPlace)
            .Include(l => l.RawMaterialStock)
            .Include(l => l.ProductStock)
            .Where(l => l.Voucher != null && l.Voucher.IsActive != false
                && (l.RawMaterialStockId == stockId || l.ProductStockId == stockId))
            .ToListAsync();

        foreach (var line in productionLines)
        {
            var voucher = line.Voucher!;

            if (line.RawMaterialStockId == stockId)
            {
                movements.Add(new AppStockDetailLineViewModel
                {
                    WorkPlaceId = voucher.WorkPlaceId ?? 0,
                    WorkPlaceName = voucher.WorkPlace?.WorkPlaceName ?? "-",
                    TransactionDate = voucher.ProductionDate,
                    DocumentNo = voucher.VoucherNo,
                    TransactionType = AppStockMovementTypes.ProductionOut,
                    AccountName = line.ProductStock != null ? $"→ {line.ProductStock.StockName}" : null,
                    Quantity = -line.RawMaterialQuantity,
                    UnitPrice = line.RawMaterialUnitPrice,
                    NetAmount = -line.RawMaterialNetAmount,
                    CurrencyCode = "TRY",
                    ExchangeRate = 1m,
                    CurrencyUnitPrice = line.RawMaterialUnitPrice,
                    CurrencyNetAmount = -line.RawMaterialNetAmount,
                    SourceRecId = voucher.RecId
                });
            }
            else if (line.ProductStockId == stockId)
            {
                movements.Add(new AppStockDetailLineViewModel
                {
                    WorkPlaceId = voucher.WorkPlaceId ?? 0,
                    WorkPlaceName = voucher.WorkPlace?.WorkPlaceName ?? "-",
                    TransactionDate = voucher.ProductionDate,
                    DocumentNo = voucher.VoucherNo,
                    TransactionType = AppStockMovementTypes.ProductionIn,
                    AccountName = line.RawMaterialStock != null ? $"← {line.RawMaterialStock.StockName}" : null,
                    Quantity = line.ProductQuantity,
                    UnitPrice = line.ProductUnitPrice,
                    NetAmount = line.ProductNetAmount,
                    CurrencyCode = "TRY",
                    ExchangeRate = 1m,
                    CurrencyUnitPrice = line.ProductUnitPrice,
                    CurrencyNetAmount = line.ProductNetAmount,
                    SourceRecId = voucher.RecId
                });
            }
        }

        return movements;
    }

    /// <summary>
    /// Bakiye Adet ve Yürüyen Maliyet'i, verilen satır listesi üzerinde baştan sona
    /// kümülatif olarak hesaplar. Her şube kendi maliyet havuzunu taşıdığından bu metot
    /// şube bazında (tek bir şubenin, tarih sırasına göre sıralanmış hareketleri) çağrılır;
    /// böylece bir transfer fişi diğer şubelerin ortalama maliyetini etkilemez. Giriş
    /// satırlarında (Açılış, Alış, Transfer Giriş) satırın kendi net tutarı maliyete
    /// eklenir; çıkış satırlarında (Satış, Çıkış Fişi, Transfer Çıkış) çıkan miktar, o ana
    /// kadarki hareketli ağırlıklı ortalama birim maliyet üzerinden düşülür (satış fiyatı
    /// maliyeti etkilemez).
    /// </summary>
    private static void ComputeRunningTotals(List<AppStockDetailLineViewModel> lines)
    {
        decimal runningQuantity = 0;
        decimal runningCostValue = 0;

        foreach (var line in lines)
        {
            if (line.Quantity > 0)
            {
                runningCostValue += line.NetAmount;
                runningQuantity += line.Quantity;
            }
            else if (line.Quantity < 0)
            {
                var unitCostBefore = runningQuantity != 0 ? runningCostValue / runningQuantity : 0m;
                runningCostValue += line.Quantity * unitCostBefore;
                runningQuantity += line.Quantity;
            }

            line.BalanceQuantity = runningQuantity;
            line.RunningUnitCost = runningQuantity != 0 ? runningCostValue / runningQuantity : 0m;
        }
    }

    private async Task<IActionResult?> EnsureAdmin()
    {
        var action = AppScreenRights.ResolveAction(ControllerContext.ActionDescriptor.ActionName);
        var allowed = await _userRightService.HasScreenAccessAsync(User, ControllerContext.ActionDescriptor.ControllerName, action);
        return allowed ? null : RedirectToAction("Index", "Home");
    }

    private void NormalizeCheckboxes(AppStockEditViewModel vm)
    {
        vm.IsActive = string.Equals(Request.Form["IsActive"], "true", StringComparison.Ordinal);
    }

    private async Task PopulateSelectListsAsync(AppStockEditViewModel vm)
    {
        var groups = await _context.AppStockGroups.AsNoTracking()
            .Where(x => x.IsActive != false)
            .OrderBy(x => x.GroupCode)
            .Select(x => new { x.RecId, x.GroupCode, x.GroupName })
            .ToListAsync();

        vm.StockGroupOptions = groups
            .Select(x => new SelectListItem($"{x.GroupCode} - {x.GroupName}", x.RecId.ToString(CultureInfo.InvariantCulture)))
            .ToList();

        var units = await _context.AppStockUnits.AsNoTracking()
            .Where(x => x.IsActive != false)
            .OrderBy(x => x.UnitCode)
            .Select(x => new { x.RecId, x.UnitCode, x.UnitName })
            .ToListAsync();

        vm.StockUnitOptions = units
            .Select(x => new SelectListItem($"{x.UnitCode} - {x.UnitName}", x.RecId.ToString(CultureInfo.InvariantCulture)))
            .ToList();

        var vats = await _context.AppVatDefinitions.AsNoTracking()
            .Where(x => x.IsActive != false)
            .OrderBy(x => x.VatCode)
            .Select(x => new { x.RecId, x.VatCode, x.VatName, x.VatRate })
            .ToListAsync();

        vm.VatOptions = vats
            .Select(x => new SelectListItem(
                $"{x.VatCode} - {x.VatName} (%{x.VatRate.ToString("0.##", CultureInfo.InvariantCulture)})",
                x.RecId.ToString(CultureInfo.InvariantCulture)))
            .ToList();
    }

    private async Task ValidateStockAsync(AppStockEditViewModel vm)
    {
        var code = vm.StockCode.Trim().ToUpperInvariant();
        vm.StockCode = code;

        var codeExists = await _context.AppStocks.AnyAsync(x =>
            x.StockCode == code && x.RecId != vm.RecId);
        if (codeExists)
        {
            ModelState.AddModelError(nameof(vm.StockCode), "Bu stok kodu zaten kullanılıyor.");
        }

        if (vm.StockGroupId.HasValue &&
            !await _context.AppStockGroups.AnyAsync(x => x.RecId == vm.StockGroupId.Value))
        {
            ModelState.AddModelError(nameof(vm.StockGroupId), "Geçersiz stok grubu seçildi.");
        }

        if (vm.StockUnitId.HasValue &&
            !await _context.AppStockUnits.AnyAsync(x => x.RecId == vm.StockUnitId.Value))
        {
            ModelState.AddModelError(nameof(vm.StockUnitId), "Geçersiz stok birimi seçildi.");
        }

        if (vm.PurchaseVatId.HasValue &&
            !await _context.AppVatDefinitions.AnyAsync(x => x.RecId == vm.PurchaseVatId.Value))
        {
            ModelState.AddModelError(nameof(vm.PurchaseVatId), "Geçersiz alış KDV seçildi.");
        }

        if (vm.SalesVatId.HasValue &&
            !await _context.AppVatDefinitions.AnyAsync(x => x.RecId == vm.SalesVatId.Value))
        {
            ModelState.AddModelError(nameof(vm.SalesVatId), "Geçersiz satış KDV seçildi.");
        }

        if (vm.ProductionWeight.HasValue && vm.ProductionWeight.Value < 0)
        {
            ModelState.AddModelError(nameof(vm.ProductionWeight), "Üretim ağırlığı negatif olamaz.");
        }
    }

    private static AppStockEditViewModel MapToViewModel(AppStock entity)
    {
        return new AppStockEditViewModel
        {
            RecId = entity.RecId,
            StockCode = entity.StockCode,
            StockName = entity.StockName,
            StockGroupId = entity.StockGroupId,
            StockUnitId = entity.StockUnitId,
            PurchaseVatId = entity.PurchaseVatId,
            SalesVatId = entity.SalesVatId,
            Barcode = entity.Barcode,
            StockType = entity.StockType,
            ProductionWeight = entity.ProductionWeight,
            IsActive = entity.IsActive != false
        };
    }

    private static AppStock MapToEntity(AppStockEditViewModel vm)
    {
        var entity = new AppStock();
        ApplyViewModel(entity, vm);
        return entity;
    }

    private static void ApplyViewModel(AppStock entity, AppStockEditViewModel vm)
    {
        entity.StockCode = vm.StockCode.Trim().ToUpperInvariant();
        entity.StockName = vm.StockName.Trim();
        entity.StockGroupId = vm.StockGroupId;
        entity.StockUnitId = vm.StockUnitId;
        entity.PurchaseVatId = vm.PurchaseVatId;
        entity.SalesVatId = vm.SalesVatId;
        entity.Barcode = string.IsNullOrWhiteSpace(vm.Barcode) ? null : vm.Barcode.Trim();
        entity.StockType = string.IsNullOrWhiteSpace(vm.StockType) ? null : vm.StockType.Trim();
        entity.ProductionWeight = vm.ProductionWeight;
        entity.IsActive = vm.IsActive;
    }

    private static string BuildAuditValue(AppStock entity)
    {
        var sb = new StringBuilder();
        sb.Append($"Code={entity.StockCode}, Name={entity.StockName}");
        sb.Append($", GroupId={entity.StockGroupId}, UnitId={entity.StockUnitId}");
        sb.Append($", PurchaseVatId={entity.PurchaseVatId}, SalesVatId={entity.SalesVatId}");
        sb.Append($", Barcode={entity.Barcode}, Type={entity.StockType}");
        sb.Append($", ProductionWeight={entity.ProductionWeight}");
        sb.Append($", IsActive={entity.IsActive == true}");
        return sb.ToString();
    }
}
