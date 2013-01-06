using System.Web.Mvc;
using Orchard.Localization;
using Orchard;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using Orchard.ContentManagement;
using Nublr.CustomAlias.Services;
using Orchard.Security;
using System;
using System.Linq;
using Nublr.CustomAlias.ViewModels;
using Nublr.CustomAlias.Models;
using Orchard.Core.Contents.Controllers;
using System.Web.Routing;
using System.Collections.Generic;
using Orchard.Environment.Extensions;

namespace Nublr.CustomAlias.Controllers
{
    [OrchardFeature("Nublr.CustomAlias")]
    public class AdminController : Controller, IUpdateModel
    {
        public IOrchardServices Services { get; set; }
        private readonly ICustomAliasService _customAliasService;
        public AdminController(IOrchardServices services, ICustomAliasService customAliasService)
        {
            Services = services;
            T = NullLocalizer.Instance;
            _customAliasService = customAliasService;
        }

        public Localizer T { get; set; }

        public ActionResult Index(AdminIndexOptions options, PagerParameters pagerParameters)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage aliases")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(Services.WorkContext.CurrentSite, pagerParameters);

            // default options
            if (options == null)
                options = new AdminIndexOptions();

            switch (options.Filter)
            {
                case CustomAliasFilter.All:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var links = _customAliasService.GetAll();

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                var invariantSearch = options.Search.ToLowerInvariant();
                links = links.Where(x => x.Alias.ToLowerInvariant().Contains(invariantSearch));
            }

            links = links.ToList();

            var pagerShape = Services.New.Pager(pager).TotalItemCount(links.Count());

            switch (options.Order)
            {
                case CustomAliasOrder.Alias:
                    links = links.OrderBy(x => x.Alias);
                    break;
                case CustomAliasOrder.OriginalUrl:
                    links = links.OrderBy(x => x.OriginalUrl);
                    break;
            }

            if (pager.PageSize != 0)
            {
                links = links.Skip(pager.GetStartIndex()).Take(pager.PageSize);
            }

            var model = new AdminIndexViewModel
            {
                Options = options,
                Pager = pagerShape,
                Aliases = links.Select(x => new CustomAliasEntry() { Record = x, IsChecked = false }).ToList()
            };

            // maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Filter", options.Filter);
            routeData.Values.Add("Options.Search", options.Search);
            routeData.Values.Add("Options.Order", options.Order);

            pagerShape.RouteData(routeData);

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult Index(FormCollection input)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage aliases")))
                return new HttpUnauthorizedResult();

            var viewModel = new AdminIndexViewModel { Aliases = new List<CustomAliasEntry>(), Options = new AdminIndexOptions() };
            UpdateModel(viewModel);

            var checkedItems = viewModel.Aliases.Where(c => c.IsChecked);

            switch (viewModel.Options.BulkAction)
            {
                case CustomAliasBulkAction.None:
                    break;
                case CustomAliasBulkAction.Enable:
                    foreach (var entry in checkedItems)
                    {
                        _customAliasService.GetById(entry.Record.Id).Enabled = true;
                    }
                    break;
                case CustomAliasBulkAction.Disable:
                    foreach (var entry in checkedItems)
                    {
                        _customAliasService.GetById(entry.Record.Id).Enabled = false;
                    }
                    break;
                case CustomAliasBulkAction.Delete:
                    foreach (var checkedItem in checkedItems)
                    {
                        _customAliasService.Delete(checkedItem.Record.Id);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return RedirectToAction("Index");
        }

        public ActionResult Create()
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage aliases")))
                return new HttpUnauthorizedResult();

            return View(new AdminCreateViewModel());
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePOST(AdminCreateViewModel viewModel)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage aliases")))
                return new HttpUnauthorizedResult();

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();
            }
            else
            {
                var alias = _customAliasService.Create(viewModel.Alias, viewModel.OriginalUrl, viewModel.Permanent, true);
                if (alias != null)
                    return RedirectToAction("Edit", new { id = alias.Id });
            }

            return View(viewModel);
        }

        public ActionResult Edit(int id)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage aliases")))
                return new HttpUnauthorizedResult();
            
            var alias = _customAliasService.GetById(id);
            if(alias == null)
                return RedirectToAction("Index");

            AdminEditViewModel model = new AdminEditViewModel()
            {
                Id = alias.Id,
                Alias = alias.Alias,
                OriginalUrl = alias.OriginalUrl,
                Permanent = alias.Permanent,
                Enabled = alias.Enabled
            };

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Save")]
        public ActionResult EditPost(AdminEditViewModel viewModel)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage aliases")))
                return new HttpUnauthorizedResult();

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();
            }
            else
            {
                var record = _customAliasService.GetById(viewModel.Id);
                record.Alias = viewModel.Alias;
                record.OriginalUrl = viewModel.OriginalUrl;
                record.Permanent = viewModel.Permanent;
                record.Enabled = viewModel.Enabled;
                _customAliasService.Update(record);
                Services.Notifier.Information(T("Custom Alias Saved"));
                return RedirectToAction("Edit", new { id = record.Id });
            }

            return View(viewModel);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.SaveAndEnable")]
        public ActionResult EditAndEnablePost(AdminEditViewModel viewModel)
        {
            viewModel.Enabled = true;
            return EditPost(viewModel);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage aliases")))
                return new HttpUnauthorizedResult();

            var record = _customAliasService.GetById(id);

            if (record != null)
            {
                _customAliasService.Delete(id);
                Services.Notifier.Information(T("Custom Alias {0} deleted", record.Alias));
            }

            return RedirectToAction("Index");
        }

        public ActionResult Enable(int id)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage aliases")))
                return new HttpUnauthorizedResult();

            var record = _customAliasService.GetById(id);

            if (record != null)
            {
                record.Enabled = true;
                _customAliasService.Update(record);
                Services.Notifier.Information(T("Custom Alias enabled"));
            }

            return RedirectToAction("Index");
        }

        public ActionResult Disable(int id)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage aliases")))
                return new HttpUnauthorizedResult();

            var record = _customAliasService.GetById(id);

            if (record != null)
            {
                record.Enabled = false;
                _customAliasService.Update(record);
                Services.Notifier.Information(T("Custom Alias disabled"));
            }

            return RedirectToAction("Index");
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties)
        {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage)
        {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}
