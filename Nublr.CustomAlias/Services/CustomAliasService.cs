using Nublr.CustomAlias.Models;
using Nublr.CustomAlias.Routing;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nublr.CustomAlias.Services
{
    public interface ICustomAliasService : IDependency
    {
        IEnumerable<CustomAliasRecord> GetAll();
        IEnumerable<string> GetAliases();
        CustomAliasRecord Create(string alias, string originalUrl, bool permanent, bool enabled);
        CustomAliasRecord GetById(int id);
        void Delete(int id);
        CustomAliasRecord GetByAlias(string alias);
        void Update(CustomAliasRecord entity);
    }

    public class CustomAliasService : ICustomAliasService
    {
        private readonly IRepository<CustomAliasRecord> _repository;
        private readonly INotifier _notifier;
        private readonly IOrchardServices _services;
        private readonly ICustomAliasConstraint _customAliasConstraint;

        public CustomAliasService(
            IRepository<CustomAliasRecord> repository,
            INotifier notifier,
            IOrchardServices services,
            ICustomAliasConstraint customAliasConstraint)
        {
            _repository = repository;
            _notifier = notifier;
            _services = services;
            _customAliasConstraint = customAliasConstraint;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public IEnumerable<CustomAliasRecord> GetAll()
        {
            return _repository.Table.ToList();
        }

        public CustomAliasRecord Create(string alias, string originalUrl, bool permanent, bool enabled)
        {
            if (GetByAlias(alias) != null)
            {
                _notifier.Warning(T("The alias {0} already exists", alias));
                return null;
            }

            CustomAliasRecord record = new CustomAliasRecord()
            {
                Alias = alias,
                OriginalUrl = originalUrl,
                Permanent = permanent,
                Enabled = enabled
            };

            _repository.Create(record);
            _customAliasConstraint.AddAlias(alias);
            return record;
        }

        public CustomAliasRecord GetById(int id)
        {
            return _repository.Get(id);
        }

        public CustomAliasRecord GetByAlias(string alias)
        {
            return _repository.Get(r => r.Alias == alias);
        }

        public void Delete(int id)
        {
            var alias = GetById(id);
            _repository.Delete(alias);
            _customAliasConstraint.RemoveAlias(alias.Alias);
        }

        public void Update(CustomAliasRecord entity)
        {
            if (!entity.Enabled)
                _customAliasConstraint.RemoveAlias(entity.Alias);
            else if(!string.IsNullOrEmpty(_customAliasConstraint.FindAlias(entity.Alias)))
                _customAliasConstraint.AddAlias(entity.Alias);
        }
        public IEnumerable<string> GetAliases()
        {
            return _repository.Table.Where(i => i.Enabled).Select(i => i.Alias);
        }
    }


}