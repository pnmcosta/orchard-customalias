using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Nublr.CustomAlias.ViewModels
{
    public class AdminCreateViewModel
    {
        [Required, StringLength(2048)]
        public string Alias { get; set; }
        [Required, StringLength(2048)]
        public string OriginalUrl { get; set; }
        public bool Permanent { get; set; }
    }
}