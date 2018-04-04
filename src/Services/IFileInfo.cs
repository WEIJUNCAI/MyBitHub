using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BitHub.Services
{
    public interface IFileInfoManager
    {
        void SetFilePath(string path);
        long GetLength();
    }
}
