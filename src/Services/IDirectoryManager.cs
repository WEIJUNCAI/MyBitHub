using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace BitHub.Services
{
    public interface IDirectoryManager
    {
        bool Exists(string path);
        DirectoryInfo CreateDirectory(string path);
        void Delete(string path);
        string[] GetDirectories(string path);
        string[] GetFiles(string path);
    }
}
