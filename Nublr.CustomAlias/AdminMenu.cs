using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace Nublr.CustomAlias
{
    [OrchardFeature("Nublr.CustomAlias")]
    public class AdminMenu : INavigationProvider
    {
        public Localizer T { get; set; }

        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder)
        {
            builder
                .Add(T("Custom Aliases"), "4", item => item.Action("Index", "Admin", new { area = "Nublr.CustomAlias" }).Permission(StandardPermissions.SiteOwner));
        }
    }
}