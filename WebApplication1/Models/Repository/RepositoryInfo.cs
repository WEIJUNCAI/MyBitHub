using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BitHub.Models.Repository
{
    public class RepositoryInfoModel
    {
        public int ID { get; set; }

        public string Owner { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z0-9_\-]{1,}$", ErrorMessage = "Cannot contain special or space charactors.")]
        [StringLength(100, ErrorMessage = "Invalid repository name.", MinimumLength = 1)]
        public string RepoName { get; set; }

        [StringLength(200, ErrorMessage = "Invalid description.")]
        public string Description { get; set; }

        public string RootPath { get; set; }

        public int WatchCount { get; set; }
        
        public int StarCount { get; set; }

        public int ForkCount { get; set; }

        public string ReadMeFilePath { get; set; }

        public string LicenseFilePath { get; set; }

        // Navigation property
        public ICollection<RepoTagmentModel> Tags { get; set; }
    }
}
