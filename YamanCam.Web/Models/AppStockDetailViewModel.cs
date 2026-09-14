using Microsoft.AspNetCore.Mvc.Rendering;

namespace YamanCam.Web.Models;

public static class AppStockMovementTypes
{
    public const string Opening = "Açılış";
    public const string Purchase = "Alış";
    public const string SalesReturn = "Satış İade";
    public const string TransferIn = "Transfer Giriş";
    public const string ProductionIn = "Üretim Giriş";
    public const string TransferOut = "Transfer Çıkış";
    public const string PurchaseReturn = "Alış İade";
    public const string Sales = "Satış";
    public const string ProductionOut = "Üretim Çıkış";
    public const string Issue = "Çıkış";

    /// <summary>
    /// Aynı tarihteki işlemler arasındaki sıralama önceliği: önce şubeye giren
    /// işlemler (Açılış, Alış, Satış İade, Transfer Giriş, Üretim Giriş), sonra
    /// şubeden çıkan işlemler (Transfer Çıkış, Alış İade, Satış, Üretim Çıkış,
    /// Çıkış Fişi). Tarih sırasını bozmaz, yalnızca aynı güne denk gelen işlemleri
    /// sıralar. Satış İade stoğa geri giriş olduğu için Alış ile aynı mantıkla
    /// (miktar/maliyet artışı), Alış İade ise stoktan çıkış olduğu için Satış ile
    /// aynı mantıkla (miktar/maliyet azalışı) ele alınır. Üretim Fişi'nde hammadde
    /// tüketimi (Üretim Çıkış) Çıkış Fişi ile, üretilen mamül (Üretim Giriş) ise
    /// Alış ile aynı mantıkla ele alınır.
    /// </summary>
    public static int GetSortOrder(string transactionType) => transactionType switch
    {
        Opening => 0,
        Purchase => 1,
        SalesReturn => 2,
        TransferIn => 3,
        ProductionIn => 4,
        TransferOut => 5,
        PurchaseReturn => 6,
        Sales => 7,
        ProductionOut => 8,
        Issue => 9,
        _ => 10
    };
}

public class AppStockDetailViewModel
{
    public int StockId { get; set; }
    public string StockCode { get; set; } = string.Empty;
    public string StockName { get; set; } = string.Empty;
    public string? StockGroupName { get; set; }
    public string? StockUnitCode { get; set; }

    public int? WorkPlaceId { get; set; }
    public string? TransactionType { get; set; }

    public List<SelectListItem> WorkPlaceOptions { get; set; } = new();
    public List<SelectListItem> TransactionTypeOptions { get; set; } = new();

    public List<AppStockDetailBranchSummaryViewModel> BranchSummaries { get; set; } = new();
    public List<AppStockDetailBranchGroupViewModel> BranchGroups { get; set; } = new();

    public int LineCount => BranchGroups.Sum(g => g.Lines.Count);
    public decimal TotalQuantity => BranchGroups.Sum(g => g.TotalQuantity);
    public decimal TotalNetAmount => BranchGroups.Sum(g => g.TotalNetAmount);
}

public class AppStockDetailBranchSummaryViewModel
{
    public string WorkPlaceName { get; set; } = string.Empty;
    public decimal TotalIn { get; set; }
    public decimal TotalOut { get; set; }
    public decimal Balance { get; set; }
}

/// <summary>
/// Detay ızgarasında bir şubeye ait hareketleri gruplar. Bakiye Adet ve Yürüyen Maliyet
/// bu grup içindeki satırlar üzerinden, şubeye özel olarak hesaplanır (bkz.
/// AppStocksController.ComputeRunningTotals).
/// </summary>
public class AppStockDetailBranchGroupViewModel
{
    public string WorkPlaceName { get; set; } = string.Empty;
    public List<AppStockDetailLineViewModel> Lines { get; set; } = new();

    public decimal TotalQuantity => Lines.Sum(x => x.Quantity);
    public decimal TotalNetAmount => Lines.Sum(x => x.NetAmount);
}

public class AppStockDetailLineViewModel
{
    public int WorkPlaceId { get; set; }
    public string WorkPlaceName { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string DocumentNo { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public string? AccountName { get; set; }

    public decimal Quantity { get; set; }
    public decimal BalanceQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal NetAmount { get; set; }

    /// <summary>
    /// Bu satır dahil, o ana kadarki hareketli ağırlıklı ortalama birim maliyet.
    /// </summary>
    public decimal RunningUnitCost { get; set; }

    public string CurrencyCode { get; set; } = "TRY";
    public decimal ExchangeRate { get; set; } = 1m;
    public decimal CurrencyUnitPrice { get; set; }
    public decimal CurrencyNetAmount { get; set; }

    public int SourceRecId { get; set; }
}
