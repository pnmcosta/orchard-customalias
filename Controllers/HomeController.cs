using System.Web.Mvc;
using Orchard.Localization;
using Orchard;
using Nublr.CustomAlias.Services;
using Orchard.Security;
using Orchard.Environment.Extensions;

namespace Nublr.CustomAlias.Controllers
{
    [OrchardFeature("Nublr.CustomAlias")]
    public class HomeController : Controller
    {
        public IOrchardServices Services { get; set; }
        private readonly ICustomAliasService _customAliasService;
        public HomeController(IOrchardServices services, ICustomAliasService customAliasService)
        {
            Services = services;
            T = NullLocalizer.Instance;
            _customAliasService = customAliasService;
        }

        public Localizer T { get; set; }

        public ActionResult Go(string customAlias)
        {
            var link = _customAliasService.GetByAlias(customAlias);
            if (link != null && link.Enabled)
                return new RedirectResult(link.OriginalUrl, link.Permanent);
            return HttpNotFound();
        }
    }
}
