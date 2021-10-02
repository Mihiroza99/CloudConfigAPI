using CloudConfiguration.WebAPI.Constants;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Serialization.Configuration;

namespace CloudConfiguration.WebAPI.Extentions
{
    public static class StringExtension
    {
        public static string ReplaceLibraryPath(this string path)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                if (Buildpath.isLocal)
                {
                    return path.Replace(Buildpath.LibraryPathText, Buildpath.LibraryLocalPath);
                }
                else
                {
                    return path.Replace(Buildpath.LibraryPathText, Buildpath.Program);
                }
            }
            else
            {
                return path;
            }
        }

        public static string ReplaceInstallPath(this string path)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                //if (Buildpath.isLocal)
                //{
                //    return path.Replace(Buildpath.InstallPathText, Buildpath.InstallLocalPath).Replace(Buildpath.TimeStampText, timeStamp.ToString());
                //}
                //else
                //{
                //    return path.Replace(Buildpath.InstallPathText, Buildpath.InstallRootPath).Replace(Buildpath.TimeStampText, timeStamp.ToString());
                //}
                return path.Replace(Buildpath.InstallPathText, Buildpath.InstallRootPath);
            }
            else
            {
                return path;
            }
        }
    }
}