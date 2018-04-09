using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using LibGit2Sharp;

namespace BitHub.Models.Repository
{
    public class FileViewModel
    {
        public FileViewModel(
            long size, 
            uint lineCount, 
            string content, 
            string languageShort, 
            string languageFull, 
            Commit latestCommt, string fullPath)
        {
            Size = size;
            LineCount = lineCount;
            Content = content;
            LanguageShort = languageShort;
            LanguageFull = languageFull;
            LatestCommt = latestCommt;
            FullPath = fullPath;
        }


        // size of file, in bytes
        public long Size { get;  }

        // numeber of lines in this file
        public uint LineCount { get;  }

        // the raw content of this file
        public string Content { get;  }

        // short name of language type, e.g. js, cs
        public string LanguageShort { get;  }

        // full name of language type e.g. javascript
        public string LanguageFull { get;  }

        // latest commit where this file changed, does not include rename
        public Commit LatestCommt { get;  }

        // the full (relative to repo root) path
        public string FullPath { get;  }
    }
}
