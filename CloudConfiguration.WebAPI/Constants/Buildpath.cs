using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudConfiguration.WebAPI.Constants
{
    public static class Buildpath
    {
        public static string azureConnectionString = "DefaultEndpointsProtocol=https;AccountName=ixconfigtemplate;AccountKey=yApTue8cPgOMTMwO3t84F8Fueirtgzq3/89r/9ckJXsWqb7ZSa3JSYlWy8r5TAoe1OD4565nQZ8TTtlF8QUMxg==;EndpointSuffix=core.windows.net";
        public static bool isLocal = false;
        public static string LibraryLocalPath = @"C:\Library\";
        public static string AzureContainerName  { get; set; }

        public static string ZipSourcePath { get; set; }
        public static string ZipDestinationPath { get; set; }
        public static string PackageDestinationPath { get; set; }
        public static string SummaryDestinationPath { get; set; }
        public static string TemplateErrorPath { get; set; }
        public static string ExeFileName = "CloudConfigExtension.exe";
        public static string SummaryJSONFileName = "Summary.json";

        public static string Program { get; set; }
        public static string InstallRootPath { get; set; }
        public static string TempFolderRootPath { get; set; }

        public static string LibraryPathText = "LIBRARYPATH";
        public static string InstallPathText = "INSTALLPATH";
        public static string TimeStampText = "TIMESTAMP";

    }
}