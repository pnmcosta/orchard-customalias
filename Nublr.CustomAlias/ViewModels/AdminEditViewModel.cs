using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Nublr.CustomAlias.ViewModels
{
    public class AdminEditViewModel
    {
        public int Id { get; set; }
        [Required, StringLength(2048)]
        public virtual string Alias { get; set; }
        [Required, StringLength(2048)]
        public virtual string OriginalUrl { get; set; }
        public bool Permanent { get; set; }

        public bool Enabled { get; set; }
    }
}