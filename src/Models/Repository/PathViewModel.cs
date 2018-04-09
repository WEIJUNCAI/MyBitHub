using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BitHub.Models.Repository
{
    public class PathViewModel
    {
        public PathViewModel(string currentPath, ICollection<string> parentDirectories)
        {
            CurrentPath = currentPath;
            ParentDirectories = parentDirectories;
        }

        // TODO: add custom attribute to validate the path
        // The name of current directory / file (e.g., the last level in directory tree)
        // the full path can be reconstructed using ParentDirectories

        public string CurrentPath { get; }
        public ICollection<string> ParentDirectories { get; }

    }
}
