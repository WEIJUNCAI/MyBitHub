using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using LibGit2Sharp;

namespace BitHub.Models.Repository
{
    public class FileViewModel
    {
        public long Size { get; set; }
        public uint LineCount { get; set; }
        public string Content { get; set; }
        public string Language { get; set; }
        public Commit LatestCommt { get; set; }
    }
}
