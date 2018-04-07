using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using LibGit2Sharp;

namespace BitHub.Models.Repository
{
    public class FileViewModel
    {
        // size of file, in bytes
        public long Size { get; set; }

        // numeber of lines in this file
        public uint LineCount { get; set; }

        // the raw content of this file
        public string Content { get; set; }

        // short name of language type, e.g. js, cs
        public string LanguageShort { get; set; }

        // full name of language type e.g. javascript
        public string LanguageFull { get; set; }

        // latest commit where this file changed, does not include rename
        public Commit LatestCommt { get; set; }

        // the full (relative to repo root) path
        public string FullPath { get; set; }
    }
}
