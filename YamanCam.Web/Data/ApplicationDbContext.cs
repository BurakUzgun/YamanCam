using YamanCam.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace YamanCam.Web.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<AppSchemaVersion> AppSchemaVersions => Set<AppSchemaVersion>();
    public DbSet<AppCompany> AppCompanies => Set<AppCompany>();
    public DbSet<AppWorkPlace> AppWorkPlaces => Set<AppWorkPlace>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<AppUserWorkPlace> AppUserWorkPlaces => Set<AppUserWorkPlace>();
    public DbSet<AppUserRight> AppUserRights => Set<AppUserRight>();
    public DbSet<AppLog> AppLogs => Set<AppLog>();
    public DbSet<AppAccountPlan> AppAccountPlans => Set<AppAccountPlan>();
    public DbSet<AppVatDefinition> AppVatDefinitions => Set<AppVatDefinition>();
    public DbSet<AppVatWithholdingDefinition> AppVatWithholdingDefinitions => Set<AppVatWithholdingDefinition>();
    public DbSet<AppStockGroup> AppStockGroups => Set<AppStockGroup>();
    public DbSet<AppStockUnit> AppStockUnits => Set<AppStockUnit>();
    public DbSet<AppStock> AppStocks => Set<AppStock>();
    public DbSet<AppJournalVoucher> AppJournalVouchers => Set<AppJournalVoucher>();
    public DbSet<AppJournalVoucherLine> AppJournalVoucherLines => Set<AppJournalVoucherLine>();
    public DbSet<AppPurchaseInvoice> AppPurchaseInvoices => Set<AppPurchaseInvoice>();
    public DbSet<AppPurchaseInvoiceLine> AppPurchaseInvoiceLines => Set<AppPurchaseInvoiceLine>();
    public DbSet<AppSalesInvoice> AppSalesInvoices => Set<AppSalesInvoice>();
    public DbSet<AppSalesInvoiceLine> AppSalesInvoiceLines => Set<AppSalesInvoiceLine>();
    public DbSet<AppCustomsFreightInvoice> AppCustomsFreightInvoices => Set<AppCustomsFreightInvoice>();
    public DbSet<AppCustomsFreightInvoiceLine> AppCustomsFreightInvoiceLines => Set<AppCustomsFreightInvoiceLine>();
    public DbSet<AppStockOpening> AppStockOpenings => Set<AppStockOpening>();
    public DbSet<AppStockOpeningLine> AppStockOpeningLines => Set<AppStockOpeningLine>();
    public DbSet<AppStockIssue> AppStockIssues => Set<AppStockIssue>();
    public DbSet<AppStockIssueLine> AppStockIssueLines => Set<AppStockIssueLine>();
    public DbSet<AppStockTransfer> AppStockTransfers => Set<AppStockTransfer>();
    public DbSet<AppStockTransferLine> AppStockTransferLines => Set<AppStockTransferLine>();
    public DbSet<AppProductTransfer> AppProductTransfers => Set<AppProductTransfer>();
    public DbSet<AppProductTransferLine> AppProductTransferLines => Set<AppProductTransferLine>();
    public DbSet<AppProductionVoucher> AppProductionVouchers => Set<AppProductionVoucher>();
    public DbSet<AppProductionVoucherLine> AppProductionVoucherLines => Set<AppProductionVoucherLine>();
    public DbSet<AppStockCount> AppStockCounts => Set<AppStockCount>();
    public DbSet<AppStockCountLine> AppStockCountLines => Set<AppStockCountLine>();
    public DbSet<AppStockMerge> AppStockMerges => Set<AppStockMerge>();
    public DbSet<AppProductionDefinition> AppProductionDefinitions => Set<AppProductionDefinition>();
}
