using YamanCam.Web.Data;
using YamanCam.Web.Models;
using YamanCam.Web.Services;

namespace YamanCam.Web.Controllers;

public class AppReceiptVouchersController : AppVoucherControllerBase
{
    public AppReceiptVouchersController(ApplicationDbContext context, IAppLogService appLogService)
        : base(context, appLogService)
    {
    }

    protected override string VoucherType => AppJournalVoucherTypes.Tahsilat;
    protected override string EntityLabel => "Tahsilat Fişi";
    protected override string EntityLabelPlural => "Tahsilat Fişleri";
    protected override bool RequiresCashBankAccount => true;
}
