using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace BitHub.Services
{
    public interface IFileManager
    {
        FileStream Create(string path);
        bool Exists(string path);
        void Delete(string path);
        FileStream Open(string path, FileMode mode);

    }
}
