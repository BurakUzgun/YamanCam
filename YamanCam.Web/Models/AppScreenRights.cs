namespace YamanCam.Web.Models;

public enum ScreenActionType
{
    View,
    Create,
    Edit,
    Delete
}

public sealed class ScreenDefinition
{
    public required string Code { get; init; }
    public required string Name { get; init; }
    public bool HasDelete { get; init; } = true;
}

public static class AppScreenRights
{
    public static readonly IReadOnlyList<ScreenActionType> AllActions =
    [
        ScreenActionType.View,
        ScreenActionType.Create,
        ScreenActionType.Edit,
        ScreenActionType.Delete
    ];

    public static readonly IReadOnlyList<ScreenDefinition> All =
    [
        new() { Code = "AppUsers", Name = "Kullanıcı Tanımları", HasDelete = false },
        new() { Code = "AppCompanies", Name = "Şirket Tanımları", HasDelete = false },
        new() { Code = "AppWorkPlaces", Name = "Şube Tanımları", HasDelete = false },
        new() { Code = "AppVatDefinitions", Name = "Kdv Tanımları" },
        new() { Code = "AppVatWithholdingDefinitions", Name = "Tevkifat Kdv Tanımları" },
        new() { Code = "AppStocks", Name = "Stoklar" },
        new() { Code = "AppStockGroups", Name = "Stok Grupları" },
        new() { Code = "AppStockUnits", Name = "Stok Birimleri" },
        new() { Code = "AppAccountPlans", Name = "Hesap Planı" },
        new() { Code = "AppPurchaseInvoices", Name = "Alış Faturaları" },
        new() { Code = "AppSalesInvoices", Name = "Satış Faturaları" },
        new() { Code = "AppCustomsFreightInvoices", Name = "Gümrük Nakliye Faturaları" },
        new() { Code = "AppPaymentVouchers", Name = "Tediye Fişi" },
        new() { Code = "AppReceiptVouchers", Name = "Tahsilat Fişi" },
        new() { Code = "AppJournalVouchers", Name = "Mahsup Fişi" },
    ];

    public static string BuildRightCode(string controllerCode, ScreenActionType action) => $"{controllerCode}.{action}";

    public static string ActionLabel(ScreenActionType action) => action switch
    {
        ScreenActionType.View => "Görüntüleme",
        ScreenActionType.Create => "Kayıt",
        ScreenActionType.Edit => "Düzeltme",
        ScreenActionType.Delete => "Silme",
        _ => action.ToString()
    };

    public static string ActionShortLabel(ScreenActionType action) => action switch
    {
        ScreenActionType.View => "Gör",
        ScreenActionType.Create => "Kayıt",
        ScreenActionType.Edit => "Düzelt",
        ScreenActionType.Delete => "Sil",
        _ => action.ToString()
    };

    public static ScreenActionType ResolveAction(string actionName) => actionName switch
    {
        "Create" => ScreenActionType.Create,
        "Edit" => ScreenActionType.Edit,
        "Delete" => ScreenActionType.Delete,
        "Restore" => ScreenActionType.Delete,
        _ => ScreenActionType.View
    };
}
