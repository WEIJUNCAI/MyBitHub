using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BitHub.Models.Repository
{
    public class RepoTagModel
    {
        public int ID { get; set; }

        [Required]
        [RegularExpression(@"^[a-z0-9_\-]{1,}$", ErrorMessage = "Tag name can only contain lower case letters, numbers, underscore and hyphen.")]
        public string TagName { get; set; }

        // Navigation property
        public ICollection<RepoTagmentModel> Tagments { get; set; }
    }
}
