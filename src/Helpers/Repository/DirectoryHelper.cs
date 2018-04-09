using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using BitHub.Services;


namespace BitHub.Helpers.Repository
{
    public static class DirectoryHelper
    {
        // get the FULL paths of all directories and files under a specific directory

        public static void GetDirsAndFiles(this IDirectoryManager manager,
            string rootFullPath, out IEnumerable<string> dirs, out IEnumerable<string> files)
        {
            try
            {
                dirs = manager.GetDirectories(rootFullPath);
                files = manager.GetFiles(rootFullPath);
            }
            catch (Exception ex)
            {
                dirs = null; files = null;
                throw new ApplicationException($"Cannot get dorectory info in \"{rootFullPath}\"", ex);
            }
        }

        // get all RELATIVE directories and files under a specific root
        // will skip dirs and files that start with ".", which is assumed to be hidden
        // *** TODO *** 
        // This skip logic might be incorrect

        public static void GetRelativeDirsAndFiles(this IDirectoryManager manager,
            string rootFullPath, out IEnumerable<string> relativeDirs, out IEnumerable<string> relativeFiles)
        {
            IEnumerable<string> fullPathDirs, fullPathFiles;

            try
            {
                manager.GetDirsAndFiles(rootFullPath, out fullPathDirs, out fullPathFiles);
            }
            catch(Exception)
            {
                relativeDirs = null; relativeFiles = null;
                throw;
            }

            relativeDirs = fullPathDirs
                 .Select(dir => GetRelativePath(rootFullPath, dir))
                 .Where(dir => !dir.StartsWith('.')).ToArray();

            relativeFiles = fullPathFiles
                 .Select(file => GetRelativePath(rootFullPath, file))
                 .Where(file => !file.StartsWith('.')).ToArray();
        }

        public static string GetRelativePath(string roottFullPath, string childFullPath)
        {
            if (!childFullPath.StartsWith(roottFullPath))
                return string.Empty;

            return childFullPath.Substring(roottFullPath.Length + 1);
        }

        // split the directory into individual levels
        // returns the parent levels and the last level
        public static Tuple<ICollection<string>, string> SplitDir(string dir)
        {
            string[] allLevels = dir.Split('\\');

            string lastLevel = allLevels.Last();
            ICollection<string> parentLevels = allLevels.Take(allLevels.Length - 1).ToArray();
            return new Tuple<ICollection<string>, string>(parentLevels, lastLevel);
        }

        // used by razor view to reconstruct the path from repository root to a
        // specific sub-directory, to fill out the route parameter of breadcrumb link

        public static string ReconstructPath(int lastLevel, IEnumerable<string> parentDirectories, string currentPathName)
        {
            string result = string.Empty;
            int curLevel = 0;
            foreach (string dir in parentDirectories)
            {
                result = Path.Combine(result, dir);
                if (curLevel++ == lastLevel)
                    break;
            }

            // if true, the desired last level is the current level
            if (curLevel == lastLevel)
                result = Path.Combine(result, currentPathName);

            return result;
        }
    }
}
