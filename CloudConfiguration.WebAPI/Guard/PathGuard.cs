using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using CloudConfiguration.WebAPI.Constants;

namespace CloudConfiguration.WebAPI.Guard
{
    public static class PathGuard
    {
        public static bool IsFile(string path) {
            return Path.HasExtension(path);
        }

        public static void CreateDirectoryIfNotExists(string path)
        {
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
        }

        public static bool areFromAndToDifferent(string fromPath, string toPath)
        {
            var removedLibraryPath = fromPath.Replace(Buildpath.LibraryPathText, "");
            var removedInstallPath = toPath.Replace(Buildpath.InstallPathText, "");
            return removedLibraryPath != removedInstallPath;
        }
    }
}