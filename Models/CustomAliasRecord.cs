using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Records;
using Orchard.Core.Title.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Nublr.CustomAlias.Models
{
    public class CustomAliasRecord
    {

        public virtual int Id { get; set; }
        public virtual bool Enabled { get; set; }
        public virtual bool Permanent { get; set; }

        [StringLength(2048)]
        public virtual string Alias { get; set; }
        [StringLength(2048)]
        public virtual string OriginalUrl { get; set; }

    }
}