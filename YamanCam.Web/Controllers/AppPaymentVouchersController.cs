using YamanCam.Web.Data;
using YamanCam.Web.Models;
using YamanCam.Web.Services;

namespace YamanCam.Web.Controllers;

public class AppPaymentVouchersController : AppVoucherControllerBase
{
    public AppPaymentVouchersController(ApplicationDbContext context, IAppLogService appLogService)
        : base(context, appLogService)
    {
    }

    protected override string VoucherType => AppJournalVoucherTypes.Tediye;
    protected override string EntityLabel => "Tediye Fişi";
    protected override string EntityLabelPlural => "Tediye Fişleri";
    protected override bool RequiresCashBankAccount => true;
}
