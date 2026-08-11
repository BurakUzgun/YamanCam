using YamanCam.Web.Data;
using YamanCam.Web.Models;
using YamanCam.Web.Services;

namespace YamanCam.Web.Controllers;

public class AppJournalVouchersController : AppVoucherControllerBase
{
    public AppJournalVouchersController(ApplicationDbContext context, IAppLogService appLogService)
        : base(context, appLogService)
    {
    }

    protected override string VoucherType => AppJournalVoucherTypes.Mahsup;
    protected override string EntityLabel => "Mahsup Fişi";
    protected override string EntityLabelPlural => "Mahsup Fişleri";
}
