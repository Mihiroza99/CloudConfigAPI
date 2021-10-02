using CloudConfiguation.DB;
using CloudConfiguration.WebAPI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace CloudConfiguration.WebAPI.Helpersmani
{
    public static class FileHelpers
    {
        public static bool CopyDirectories(string sourcePath, string destinationPath)
        {
            bool returnValue;
            try
            {
                if (!Directory.Exists(destinationPath))
                {
                    Directory.CreateDirectory(destinationPath);
                }

                // Create subdirectory structure in destination    
                foreach (string dir in System.IO.Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                {
                    var directoryPath = System.IO.Path.Combine(destinationPath, dir.Substring(sourcePath.Length + 1));
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                }

                foreach (string file_name in Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories))
                {
                    File.Copy(file_name, Path.Combine(destinationPath, file_name.Substring(sourcePath.Length + 1)), true);
                }

                returnValue = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
           
            return returnValue;
        }

        public static void SetCABPackageFiles(string cabfolderPath, string packageFolderPath)
        {
            var ext = new List<string> { "inf", "dat" };
            var infDatFile = Directory
                .EnumerateFiles(cabfolderPath, "*.*", SearchOption.AllDirectories)
                .Where(s => ext.Contains(Path.GetExtension(s).TrimStart('.').ToLowerInvariant()));

            foreach (string file_name in infDatFile)
            {
                File.Delete(file_name);
            }

            foreach (string file_name in Directory.GetFiles(packageFolderPath, "nk.bin", SearchOption.AllDirectories))
            {
                var nkbinFilePath = cabfolderPath + "\\nk.bin";
                File.Copy(file_name, nkbinFilePath);
            }

            var maniList = new ManifestList();
            maniList.files = new List<ManifestModel>();
            foreach (string file_name in Directory.GetFiles(cabfolderPath, "*.cab", SearchOption.AllDirectories))
            {
                var fileName = Path.GetFileName(file_name);
                using (var dbContext = new DatabaseContext())
                {
                    var manifestInfoRecord = dbContext.ManifestInfo.SingleOrDefault(f => f.FileName == fileName);
                    if (manifestInfoRecord != null)
                    {
                        var manifestModel = new ManifestModel();
                        manifestModel.componentType = manifestInfoRecord.ComponentType;
                        manifestModel.fileName = manifestInfoRecord.FileName;
                        maniList.files.Add(manifestModel);
                    }
                }
            }

            using (var dbContext = new DatabaseContext())
            {
                var manifestInfoRecord = dbContext.ManifestInfo.SingleOrDefault(f => f.FileName == "nk.bin");
                if (manifestInfoRecord != null)
                {
                    var manifestModel = new ManifestModel();
                    manifestModel.componentType = manifestInfoRecord.ComponentType;
                    manifestModel.fileName = manifestInfoRecord.FileName;
                    maniList.files.Add(manifestModel);
                }
            }

            var jsonString = JsonConvert.SerializeObject(maniList);
            var manifestFile = cabfolderPath + "\\Manifest.json";
            File.WriteAllText(manifestFile, jsonString);
        }

        //public static bool CreateDirectory(string directoryPath) {

        //}
    }
}