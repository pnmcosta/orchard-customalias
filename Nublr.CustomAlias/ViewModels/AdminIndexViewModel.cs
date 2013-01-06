using System.Collections.Generic;
using Nublr.CustomAlias.Models;

namespace Nublr.CustomAlias.ViewModels
{
    public class AdminIndexViewModel {
        public IList<CustomAliasEntry> Aliases { get; set; }
        public AdminIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class CustomAliasEntry {
        public CustomAliasRecord Record { get; set; }
        public bool IsChecked { get; set; }
    }
    public class AdminIndexOptions {
        public string Search { get; set; }
        public CustomAliasOrder Order { get; set; }
        public CustomAliasFilter Filter { get; set; }
        public CustomAliasBulkAction BulkAction { get; set; }
    }

    public enum CustomAliasOrder
    {
        Alias,
        OriginalUrl
    }

    public enum CustomAliasFilter
    {
        All
    }

    public enum CustomAliasBulkAction
    {
        None,
        Delete,
        Enable,
        Disable
    }
}