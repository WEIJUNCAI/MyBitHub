using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BitHub.Models.Repository
{
    // This class associates tag and repo

    public class RepoTagmentModel
    {
        public int ID { get; set; }

        public int RepoID { get; set; }

        public int TagID { get; set; }

        // Navigation properties

        public RepositoryInfoModel Repo { get; set; }
        public RepoTagModel Tag { get; set; }
    }
}
