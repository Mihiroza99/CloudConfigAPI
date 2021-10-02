using Azure.Storage.Blobs;
using CloudConfiguation.DB;
using CloudConfiguration.WebAPI.Constants;
using CloudConfiguration.WebAPI.Extentions;
using CloudConfiguration.WebAPI.Guard;
using CloudConfiguration.WebAPI.Helpers;
using CloudConfiguration.WebAPI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Xml;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Azure;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Microsoft.SqlServer.Server;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using Syroot.Windows.IO;
using Newtonsoft.Json;
using CloudConfiguration.WebAPI.Helpersmani;

namespace CloudConfiguration.WebAPI.Controllers
{
    [RoutePrefix("api/configuration")]
    [KnownType(typeof(CloudConfiguration.WebAPI.Controllers.ConfigurationController))]
    public class ConfigurationController : ApiController
    {
        // GET api/values

        [AcceptVerbs("GET")]
        [Route("list")]
        public ConfigurationResponseModel List()
        {
            var returnValue = new ConfigurationResponseModel();
            try
            {
                using (var dbContext = new DatabaseContext())
                {
                    returnValue.ResponseStatus = true;
                    returnValue.configurations = dbContext.Configuration.Where(s => s.IsActive).Select(s => new ConfigurationModel
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Version = s.Version,
                        CreatedDate = s.CreatedDate,
                        TemplatePath = s.TemplatePath,
                        IsProduction = s.IsProduction,
                        IsActive = s.IsActive
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                returnValue.ResponseStatus = false;
                returnValue.Exception = ex;
                returnValue.Message = this.LogException(ex, "List", null);
            }

            return returnValue;
        }

        [AcceptVerbs("POST")]
        [Route("gettemplate")]
        public TemplateResponseModel GetTemplate(TemplateModel template)
        {
            var returnValue = new TemplateResponseModel();
            var templateName = template.templateName;
            try
            {
                if (template.isProduction == 1)
                {
                    Buildpath.AzureContainerName = "production";
                    //Buildpath.AzureContainerName = "development";
                }
                else if (template.isProduction == 0)
                {
                    Buildpath.AzureContainerName = "development";
                }
                else if (template.isProduction == 2)
                {
                    Buildpath.AzureContainerName = "preproduction";
                }
                else if (template.isProduction == 3)
                {
                    Buildpath.AzureContainerName = "cab";
                }
                else if (template.isProduction == 4)
                {
                    Buildpath.AzureContainerName = "sst";
                }

                using (var dbContext = new DatabaseContext())
                {
                    var config = dbContext.Configuration.FirstOrDefault(c => c.Id == template.templateId);
                    if (config != null)
                    {
                        returnValue.ResponseStatus = true;
                        templateName = config.Name;
                        var blobServiceClient = new BlobServiceClient(Buildpath.azureConnectionString);
                        var containerClient = blobServiceClient.GetBlobContainerClient(Buildpath.AzureContainerName);
                        var templatePath = string.Format("{0}\\template.xml", config.TemplatePath);
                        var blobClient = containerClient.GetBlobClient(templatePath);

                        // this.CopyContainerFromAzureToTempFolder(config);


                        if (blobClient.Exists())
                        {
                            var response = blobClient.Download();
                            using (var streamReader = new StreamReader(response.Value.Content))
                            {
                                returnValue.templateContent = streamReader.ReadToEnd();
                            }
                        }
                    }
                    else
                    {
                        returnValue.ResponseStatus = false;
                        returnValue.Exception = new Exception(string.Format("Selected template not found"));
                        returnValue.Message = "Selected template not found";
                    }
                }
            }
            catch (Exception ex)
            {
                returnValue.ResponseStatus = false;
                returnValue.Exception = ex;
                returnValue.Message = this.LogException(ex, templateName, null);
            }

            return returnValue;
        }

        [AcceptVerbs("POST")]
        [Route("insertuserbuild")]
        public BaseResponseModel InsertUserBuild(UserBuildMasterModel userBuildMasterModel)
        {
            var returnValue = new BaseResponseModel();
            var templatename = "";
            if (!string.IsNullOrEmpty(userBuildMasterModel.Template))
                templatename = userBuildMasterModel.Template.Substring(0, userBuildMasterModel.Template.IndexOf("\\template.xml"));
            try
            {
                using (var dbContext = new DatabaseContext())
                {
                    var userbuildModel = new UserBuildMaster();
                    userbuildModel.CreatedDate = System.DateTime.Now;
                    userbuildModel.SessionId = System.DateTime.Now.Ticks.ToString();
                    userbuildModel.UserId = userBuildMasterModel.UserId;
                    userbuildModel.Template = userBuildMasterModel.Template;
                    userbuildModel.BuildFile = userBuildMasterModel.BuildFile;
                    dbContext.UserBuildMaster.Add(userbuildModel);
                    dbContext.SaveChanges();
                    returnValue.ResponseStatus = true;
                    returnValue.Message = "Build JSON has been inserted successfully";
                }
            }
            catch (Exception ex)
            {
                returnValue.ResponseStatus = false;
                returnValue.Exception = ex;
                returnValue.Message = this.LogException(ex, templatename, null);
            }

            return returnValue;
        }


        [AcceptVerbs("POST")]
        [Route("setuserbuild")]
        public BuildResponseModel SetUserBuild(BuildModel buildModel)
        {
            var returnValue = new BuildResponseModel();
            var templateName = "";
            var timeStamp = DateTime.Now.Ticks;
            var rootInstallPath = System.Web.Hosting.HostingEnvironment.MapPath("/Install/");
            var tempInstallPath = System.Web.Hosting.HostingEnvironment.MapPath("/TempAzureFolder/");
            if (string.IsNullOrEmpty(rootInstallPath))
            {
                var dirPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
                var rootPath = dirPath.Substring(0, dirPath.IndexOf(".Tests"));
                rootInstallPath = Path.Combine(rootPath, "Install");
                tempInstallPath = Path.Combine(rootPath, "TempAzureFolder");

            }
            PathGuard.CreateDirectoryIfNotExists(rootInstallPath);


            if (buildModel.isProduction == 1)
            {
                Buildpath.AzureContainerName = "production";
                //Buildpath.AzureContainerName = "development";
            }
            else if (buildModel.isProduction == 0)
            {
                Buildpath.AzureContainerName = "development";
            }
            else if (buildModel.isProduction == 2)
            {
                Buildpath.AzureContainerName = "preproduction";
            }
            else if (buildModel.isProduction == 3)
            {
                Buildpath.AzureContainerName = "cab";
            }
            else if (buildModel.isProduction == 4)
            {
                Buildpath.AzureContainerName = "sst";
            }

            try
            {
                using (var dbContext = new DatabaseContext())
                {
                    var config = dbContext.Configuration.FirstOrDefault(c => c.Id == buildModel.programId);
                    if (config != null)
                    {
                        templateName = config.Name;
                        var userbuildModel = new UserBuildMaster();
                        userbuildModel.CreatedDate = DateTime.Now;
                        userbuildModel.SessionId = timeStamp.ToString();
                        userbuildModel.UserId = buildModel.UserId;
                        userbuildModel.Template = string.Format("{0}\\template.xml", config.Name);
                        userbuildModel.BuildFile = buildModel.BuildFile;
                        Buildpath.Program = config.Name;
                        Buildpath.InstallRootPath = Path.Combine(rootInstallPath, timeStamp.ToString(), config.Name);
                        Buildpath.TempFolderRootPath = Path.Combine(tempInstallPath, timeStamp.ToString(), config.Name);
                        //Buildpath.ZipDestinationPath = Path.Combine(rootInstallPath, timeStamp.ToString(), config.Name + ".zip");
                        Buildpath.ZipDestinationPath = Path.Combine(rootInstallPath, timeStamp.ToString(), "default.zip");
                        Buildpath.PackageDestinationPath = Path.Combine(rootInstallPath, timeStamp.ToString(), config.Name + "-package");
                        Buildpath.SummaryDestinationPath = Path.Combine(rootInstallPath, timeStamp.ToString(), config.Name + "_Summary");

                        dbContext.UserBuildMaster.Add(userbuildModel);
                        dbContext.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                returnValue.ResponseStatus = false;
                returnValue.Exception = ex;
                returnValue.Message = this.LogException(ex, templateName, timeStamp.ToString());
                return returnValue;
            }


            try
            {
                var copyResponseModel = new List<CopyResponseModel>();
                var deleteResponseModel = new List<DeleteResponseModel>();
                var moveResponseModel = new List<MoveResponseModel>();
                var writeXMLResponseModel = new List<WriteXMLResponseModel>();
                var writeXMLTableResponseModel = new List<WriteXMLTableResponseModel>();
                var writeCABResponseModel = new List<WriteCABResponseModel>();
                bool blnIscab = false;
                var cabFolderDownloaded = false;

                foreach (var build in buildModel.build)
                {
                    if (build.type == "copy")
                    {
                        var list = this.ListAllBlobs<CloudBlockBlob, string>(b => b.Name, Buildpath.AzureContainerName, Buildpath.Program);

                        var copyModel = JsonConvert.DeserializeObject<CopyModel>(build.build.ToString());
                        copyResponseModel.Add(CopyFileFromAzure(copyModel, timeStamp, list));
                    }

                    if (build.type == "delete")
                    {
                        var deleteModel = JsonConvert.DeserializeObject<DeleteModel>(build.build.ToString());
                        deleteResponseModel.Add(DeleteFile(deleteModel, timeStamp));
                    }

                    if (build.type == "move")
                    {
                        var moveModel = JsonConvert.DeserializeObject<MoveModel>(build.build.ToString());
                        moveResponseModel.Add(MoveFile(moveModel, timeStamp));
                    }

                    if (build.type == "writeXml")
                    {
                        var writeXMLModel = JsonConvert.DeserializeObject<WriteXMLModel>(build.build.ToString());
                        writeXMLResponseModel.Add(UpdateNode(writeXMLModel, timeStamp));
                    }

                    if (build.type == "writeXmlTable")
                    {
                        var writeXMLTableModel = JsonConvert.DeserializeObject<WriteXMLTableModel>(build.build.ToString());
                        writeXMLTableResponseModel = this.UpdatedXmlTableNode(writeXMLTableModel, timeStamp);
                    }

                    if (build.type == "writeCAB")
                    {
                        var writeCabModel = JsonConvert.DeserializeObject<WriteCABModel>(build.build.ToString());
                        if (!cabFolderDownloaded)
                        {
                            CopyCABFolderFromAzure(writeCabModel);
                        }
                        writeCABResponseModel.Add(WriteCABFile(writeCabModel, timeStamp));
                        blnIscab = true;
                    }
                }

                //if (buildModel.writeXMLTable != null)
                //{
                //    writeXMLTableResponseModel = this.UpdatedXmlTableNode(buildModel.writeXMLTable, timeStamp);
                //}

                returnValue.copy = copyResponseModel;
                returnValue.delete = deleteResponseModel;
                returnValue.move = moveResponseModel;
                returnValue.writeXML = writeXMLResponseModel;
                returnValue.writeXMLTable = writeXMLTableResponseModel;
                returnValue.writeCAB = writeCABResponseModel;


                ZipFile.CreateFromDirectory(Buildpath.InstallRootPath, Buildpath.ZipDestinationPath);

                if (!Directory.Exists(Buildpath.PackageDestinationPath))
                {
                    Directory.CreateDirectory(Buildpath.PackageDestinationPath);
                }
                // File.Copy(Buildpath.ZipDestinationPath, Path.Combine(Buildpath.PackageDestinationPath, Buildpath.Program + ".zip"));
                File.Copy(Buildpath.ZipDestinationPath, Path.Combine(Buildpath.PackageDestinationPath, "default.zip"));


                // Added code for CAB Package
                if (blnIscab)
                {
                    var CabPath = Path.Combine(rootInstallPath, timeStamp.ToString(), "CABPackage");
                    var signedFolderPath = Path.Combine(rootInstallPath, timeStamp.ToString(), templateName, "SignedIX");
                    FileHelpers.SetCABPackageFiles(CabPath, signedFolderPath);
                    var cabzipPath = Path.Combine(rootInstallPath, timeStamp.ToString(), "CABPackage.zip");
                    ZipFile.CreateFromDirectory(CabPath, cabzipPath);
                    File.Copy(cabzipPath, Path.Combine(Buildpath.PackageDestinationPath, "CABPackage.zip"));
                    //File.Copy(exePath, Path.Combine(Buildpath.PackageDestinationPath, Buildpath.ExeFileName));
                }
                // End Code for CAB Package

                // start creating the summary JSON 
                if (!Directory.Exists(Buildpath.SummaryDestinationPath))
                {
                    Directory.CreateDirectory(Buildpath.SummaryDestinationPath);
                }

                var summaryFilePath = Path.Combine(Buildpath.SummaryDestinationPath, Buildpath.SummaryJSONFileName);
                using (StreamWriter writer = new StreamWriter(summaryFilePath, true))
                {
                    var summaryList = new List<SummaryKeyValue>();
                    foreach (var item in buildModel.SummaryJSON)
                    {
                        var summary = new SummaryKeyValue();
                        summary.Key = item.Label;
                        summary.Value = Convert.ToString(item.Value);
                        summaryList.Add(summary);
                    }
                    writer.WriteLine(JsonConvert.SerializeObject(summaryList, Newtonsoft.Json.Formatting.Indented));
                }


                // End code for summary JSON FILE
                // File.Copy(summaryFilePath, Path.Combine(Buildpath.SummaryDestinationPath, Buildpath.SummaryJSONFileName));
                if (File.Exists(Buildpath.TemplateErrorPath))
                {
                    File.Copy(Buildpath.TemplateErrorPath, Path.Combine(Buildpath.SummaryDestinationPath, "Error_ " + timeStamp.ToString() + ".txt"));
                }
                ZipFile.CreateFromDirectory(Buildpath.SummaryDestinationPath, Path.Combine(Buildpath.PackageDestinationPath, templateName + "_Summary" + ".zip"));

                var rootAssertsPath = System.Web.Hosting.HostingEnvironment.MapPath("/Assets/");
                if (string.IsNullOrEmpty(rootAssertsPath))
                {
                    var dirPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
                    var rootPath = dirPath.Substring(0, dirPath.IndexOf(".Tests"));
                    rootAssertsPath = Path.Combine(rootPath, "Assets");
                }
                var exePath = Path.Combine(rootAssertsPath, Buildpath.ExeFileName);
                File.Copy(exePath, Path.Combine(Buildpath.PackageDestinationPath, Buildpath.ExeFileName));
                ZipFile.CreateFromDirectory(Buildpath.PackageDestinationPath, Buildpath.PackageDestinationPath + ".zip");
                //ZipFile.CreateFromDirectory(Buildpath.PackageDestinationPath, "default.zip");

                returnValue.ResponseStatus = true;
                returnValue.Message = "Build JSON has been inserted successfully";
                // Uncomment below line to download zip file with old approach
                //returnValue.zipFilePath = Path.Combine("Install", timeStamp.ToString(), Buildpath.Program + ".zip");

                // Uncomment below line to download zip file with new approach along with exe file
                //returnValue.zipFilePath = Buildpath.PackageDestinationPath + ".zip";

                returnValue.zipFilePath = Path.Combine("Install", timeStamp.ToString(), Buildpath.Program + "-package.zip");
            }
            catch (Exception ex)
            {
                returnValue.ResponseStatus = false;
                returnValue.Exception = ex;
                returnValue.Message = this.LogException(ex, templateName, timeStamp.ToString());
            }

            return returnValue;
        }

        private List<V> ListAllBlobs<T, V>(Expression<Func<T, V>> expression, string containerName, string prefix)
        {
            var storageConnection = "DefaultEndpointsProtocol=https;AccountName=ixconfigtemplate;AccountKey=yApTue8cPgOMTMwO3t84F8Fueirtgzq3/89r/9ckJXsWqb7ZSa3JSYlWy8r5TAoe1OD4565nQZ8TTtlF8QUMxg==;EndpointSuffix=core.windows.net";
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnection);
            CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = cloudBlobClient.GetContainerReference(containerName);
            container.CreateIfNotExists();

            var list = container.ListBlobs(prefix: prefix, useFlatBlobListing: true);

            List<V> data = list.OfType<T>().Select(expression.Compile()).ToList();
            return data;
        }

        // Create a SAS token for the source blob, to enable it to be read by the StartCopyAsync method
        private static string GetSharedAccessUri(string blobName, CloudBlobContainer container)
        {
            DateTime toDateTime = DateTime.Now.AddMinutes(60);

            SharedAccessBlobPolicy policy = new SharedAccessBlobPolicy
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessStartTime = null,
                SharedAccessExpiryTime = new DateTimeOffset(toDateTime)
            };

            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
            string sas = blob.GetSharedAccessSignature(policy);

            return blob.Uri.AbsoluteUri + sas;
        }

        private async Task<string> UploadFileToBlobAsync()
        {
            try
            {
                var storageConnection = "DefaultEndpointsProtocol=https;AccountName=ixconfigtemplate;AccountKey=yApTue8cPgOMTMwO3t84F8Fueirtgzq3/89r/9ckJXsWqb7ZSa3JSYlWy8r5TAoe1OD4565nQZ8TTtlF8QUMxg==;EndpointSuffix=core.windows.net";
                CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(storageConnection);
                CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
                var timeStamp = DateTime.Now.Ticks.ToString();
                var destContainerName = "install\\" + timeStamp;
                CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(destContainerName);
                string fileName = "637353021165650168.zip";
                var rootInstallPath = System.Web.Hosting.HostingEnvironment.MapPath("/Install/");
                var zipDestinationPath = Path.Combine(rootInstallPath, fileName);

                if (await cloudBlobContainer.CreateIfNotExistsAsync())
                {
                    await cloudBlobContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
                }

                if (fileName != null)
                {
                    CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(fileName);
                    await cloudBlockBlob.UploadFromFileAsync(zipDestinationPath);
                    return cloudBlockBlob.Uri.AbsoluteUri;
                }
                return "";
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private string LogException(Exception ex, string templateName, string timeStamp)
        {
            var returnValue = ex.Message;
            var errorFolder = System.Web.Hosting.HostingEnvironment.MapPath("/Error/");
            if (string.IsNullOrEmpty(errorFolder))
            {
                var dirPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
                var rootPath = dirPath.Substring(0, dirPath.IndexOf(".Tests"));
                errorFolder = Path.Combine(rootPath, "Error");
            }
            if (!Directory.Exists(errorFolder))
            {
                Directory.CreateDirectory(errorFolder);
            }
            if (string.IsNullOrEmpty(templateName))
            {
                templateName = "Template";
            }
            errorFolder = Path.Combine(errorFolder, templateName);
            if (!Directory.Exists(errorFolder))
            {
                Directory.CreateDirectory(errorFolder);
            }
            if (string.IsNullOrWhiteSpace(timeStamp))
            {
                timeStamp = DateTime.Now.Ticks.ToString();
            }
            var filePath = Path.Combine(errorFolder, "Error_ " + timeStamp + ".txt");
            Buildpath.TemplateErrorPath = filePath;
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine("-----------------------------------------------------------------------------");
                writer.WriteLine("Date : " + DateTime.Now.ToString());
                writer.WriteLine();

                while (ex != null)
                {
                    writer.WriteLine(ex.GetType().FullName);
                    writer.WriteLine("Message : " + ex.Message);
                    writer.WriteLine("StackTrace : " + ex.StackTrace);

                    ex = ex.InnerException;
                    if (ex != null)
                        returnValue = ex.Message;
                }
            }

            return returnValue;
        }

        //-------------------------------------------------
        // Create a container
        //-------------------------------------------------
        private static async Task<BlobContainerClient> CreateSampleContainerAsync(BlobServiceClient blobServiceClient, CloudBlobClient destContainer)
        {
            try
            {
                //var containerClient = await blobServiceClient.GetBlobContainersAsync();
                //if (!containerClient.Exists())
                // CloudBlobContainer destBlobContainer = destContainer.GetContainerReference(destContainer);
                //var destBlob = destBlobContainer.GetBlockBlobReference(sourceBlob.Name);
                //{
                //    if (await container.ExistsAsync())
                //    {
                //        var containerClient = blobServiceClient.GetBlobContainerClient("ixconfigcontainer");
                //        return container;
                //    }
                //}
            }
            catch (RequestFailedException e)
            {
                Console.WriteLine("HTTP error code {0}: {1}",
                                    e.Status, e.ErrorCode);
                Console.WriteLine(e.Message);
            }

            return null;
        }

        // private void CopyContainerFromAzureToTempFolder(List<CopyModel> copyFiles, long timeStamp)
        private void CopyContainerFromAzureToTempFolder(TblConfiguration config)
        {
            // tempcode
            Buildpath.Program = config.Name;
            var tempInstallPath = System.Web.Hosting.HostingEnvironment.MapPath("/TempAzureFolder/");
            Buildpath.TempFolderRootPath = Path.Combine(tempInstallPath, DateTime.Now.Ticks.ToString(), config.Name);
            // 


            var returnValue = new List<CopyResponseModel>();
            var list = this.ListAllBlobs<CloudBlockBlob, string>(b => b.Name, Buildpath.AzureContainerName, Buildpath.Program);

            var blobServiceClient = new BlobServiceClient(Buildpath.azureConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(Buildpath.AzureContainerName);



            for (int i = 0; i < list.Count(); i++)
            {
                var tempRootPathWithoutProgram = Directory.GetParent(Buildpath.TempFolderRootPath).FullName;
                var azurePath = list[i].Replace("/", "\\");
                var fileDestinationPath = Path.Combine(tempRootPathWithoutProgram, azurePath);
                //if (replaceMentRequired)
                //{
                //    var replaceStartIndex = fileDestinationPath.ToLower().IndexOf(replacibleContent.ToLower());
                //    var finalreplaceContent = fileDestinationPath.Substring(replaceStartIndex, replacibleContent.Length);
                //    fileDestinationPath = fileDestinationPath.Replace(finalreplaceContent, replaceBy);
                //}
                var directoryPath = Path.GetDirectoryName(fileDestinationPath);
                PathGuard.CreateDirectoryIfNotExists(directoryPath);

                var FileClient = containerClient.GetBlobClient(azurePath);
                if (FileClient.Exists())
                {
                    try
                    {
                        FileClient.DownloadTo(fileDestinationPath);
                    }
                    catch (Exception ex)
                    {
                    }
                }
                //var azurePath = list[i].Replace("/", "\\");
                //if (PathGuard.IsFile(fromFilePath))
                //{
                //    var checkFileName = azurePath.ToLower() == fromFilePath.ToLower();
                //    var FileClient = containerClient.GetBlobClient(azurePath);
                //    if (FileClient.Exists())
                //    {
                //        try
                //        {
                //            var directoryPath = Path.GetDirectoryName(toFilePath);
                //            PathGuard.CreateDirectoryIfNotExists(directoryPath);
                //            FileClient.DownloadTo(toFilePath);
                //            copyResponseModel.Status = true;
                //        }
                //        catch (Exception ex)
                //        {
                //            copyResponseModel.Status = false;
                //            copyResponseModel.Exception = ex;
                //        }
                //    }
                //}
                //else // its a directory so copy folder and its nested child
                //{
                //    var installRootPathWithoutProgram = Directory.GetParent(Buildpath.InstallRootPath).FullName;
                //    var fileDestinationPath = Path.Combine(installRootPathWithoutProgram, azurePath);
                //    if (replaceMentRequired)
                //    {
                //        var replaceStartIndex = fileDestinationPath.ToLower().IndexOf(replacibleContent.ToLower());
                //        var finalreplaceContent = fileDestinationPath.Substring(replaceStartIndex, replacibleContent.Length);
                //        fileDestinationPath = fileDestinationPath.Replace(finalreplaceContent, replaceBy);
                //    }
                //    var directoryPath = Path.GetDirectoryName(fileDestinationPath);
                //    PathGuard.CreateDirectoryIfNotExists(directoryPath);

                //    var FileClient = containerClient.GetBlobClient(azurePath);
                //    if (FileClient.Exists())
                //    {
                //        try
                //        {
                //            FileClient.DownloadTo(fileDestinationPath);
                //            copyResponseModel.Status = true;
                //        }
                //        catch (Exception ex)
                //        {
                //            copyResponseModel.Status = false;
                //            copyResponseModel.Exception = ex;
                //        }
                //    }
                //}
            }
        }

        private List<CopyResponseModel> CopyFilesFromAzure(List<CopyModel> copyFiles, long timeStamp)
        {
            var returnValue = new List<CopyResponseModel>();
            var list = this.ListAllBlobs<CloudBlockBlob, string>(b => b.Name, Buildpath.AzureContainerName, Buildpath.Program);

            foreach (var copyModel in copyFiles)
            {
                var copyResponseModel = new CopyResponseModel();
                string replacibleContent = copyModel.from.Replace(Buildpath.LibraryPathText, ""); ;
                string replaceBy = copyModel.to.Replace(Buildpath.InstallPathText, "");
                var replaceMentRequired = replacibleContent.ToLower() != replaceBy.ToLower();

                var fromFilePath = copyModel.from.ReplaceLibraryPath();
                var toFilePath = copyModel.to.ReplaceInstallPath();

                if (fromFilePath.IndexOf(Buildpath.InstallPathText) == -1)
                {
                    copyResponseModel.from = fromFilePath;
                    copyResponseModel.to = toFilePath;

                    var blobServiceClient = new BlobServiceClient(Buildpath.azureConnectionString);
                    var containerClient = blobServiceClient.GetBlobContainerClient(Buildpath.AzureContainerName);

                    // fromFilePath = fromFilePath.Replace("\\", "/");

                    for (int i = 0; i < list.Count(); i++)
                    {
                        var azurePath = list[i].Replace("/", "\\");
                        if (azurePath.ToLower().IndexOf(fromFilePath.ToLower()) > -1)
                        {
                            if (PathGuard.IsFile(fromFilePath))
                            {
                                var checkFileName = azurePath.ToLower() == fromFilePath.ToLower();
                                var FileClient = containerClient.GetBlobClient(azurePath);
                                if (FileClient.Exists())
                                {
                                    try
                                    {
                                        var directoryPath = Path.GetDirectoryName(toFilePath);
                                        PathGuard.CreateDirectoryIfNotExists(directoryPath);
                                        FileClient.DownloadTo(toFilePath);
                                        copyResponseModel.Status = true;
                                    }
                                    catch (Exception ex)
                                    {
                                        copyResponseModel.Status = false;
                                        copyResponseModel.Exception = ex;
                                    }
                                }
                            }
                            else // its a directory so copy folder and its nested child
                            {
                                var installRootPathWithoutProgram = Directory.GetParent(Buildpath.InstallRootPath).FullName;
                                var fileDestinationPath = Path.Combine(installRootPathWithoutProgram, azurePath);
                                if (replaceMentRequired)
                                {
                                    var replaceStartIndex = fileDestinationPath.ToLower().IndexOf(replacibleContent.ToLower());
                                    var finalreplaceContent = fileDestinationPath.Substring(replaceStartIndex, replacibleContent.Length);
                                    fileDestinationPath = fileDestinationPath.Replace(finalreplaceContent, replaceBy);
                                }
                                var directoryPath = Path.GetDirectoryName(fileDestinationPath);
                                PathGuard.CreateDirectoryIfNotExists(directoryPath);

                                var FileClient = containerClient.GetBlobClient(azurePath);
                                if (FileClient.Exists())
                                {
                                    try
                                    {
                                        FileClient.DownloadTo(fileDestinationPath);
                                        copyResponseModel.Status = true;
                                    }
                                    catch (Exception ex)
                                    {
                                        copyResponseModel.Status = false;
                                        copyResponseModel.Exception = ex;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    fromFilePath = copyModel.from.ReplaceInstallPath();
                    copyResponseModel.from = fromFilePath;
                    copyResponseModel.to = toFilePath;
                    if (PathGuard.IsFile(fromFilePath))
                    {
                        if (File.Exists(fromFilePath))
                        {
                            try
                            {
                                var directoryInfo = Path.GetDirectoryName(toFilePath);
                                if (!Directory.Exists(directoryInfo))
                                {
                                    Directory.CreateDirectory(directoryInfo);
                                }

                                File.Copy(fromFilePath, toFilePath, true);
                                copyResponseModel.Status = true;
                            }
                            catch (IOException e)
                            {
                                copyResponseModel.Status = false;
                                copyResponseModel.Exception = e;
                            }
                        }
                    }
                    else // its a directory so copy folder and its nested child
                    {
                        if (Directory.Exists(fromFilePath))
                        {
                            try
                            {
                                FileHelpers.CopyDirectories(fromFilePath, toFilePath);
                            }
                            catch (Exception ex)
                            {
                                copyResponseModel.Status = false;
                                copyResponseModel.Exception = ex;
                            }
                        }
                    }
                }
                returnValue.Add(copyResponseModel);
            }

            return returnValue;
        }

        private CopyResponseModel CopyFileFromAzure(CopyModel copyModel, long timeStamp, List<string> list)
        {
            var copyResponseModel = new CopyResponseModel();
            string replacibleContent = copyModel.from.Replace(Buildpath.LibraryPathText, ""); ;
            string replaceBy = copyModel.to.Replace(Buildpath.InstallPathText, "");
            var replaceMentRequired = replacibleContent.ToLower() != replaceBy.ToLower();

            var fromFilePath = copyModel.from.ReplaceLibraryPath();
            var toFilePath = copyModel.to.ReplaceInstallPath();

            if (fromFilePath.IndexOf(Buildpath.InstallPathText) == -1)
            {
                copyResponseModel.from = fromFilePath;
                copyResponseModel.to = toFilePath;

                var blobServiceClient = new BlobServiceClient(Buildpath.azureConnectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(Buildpath.AzureContainerName);

                // fromFilePath = fromFilePath.Replace("\\", "/");

                for (int i = 0; i < list.Count(); i++)
                {
                    var azurePath = list[i].Replace("/", "\\");
                    if (azurePath.ToLower().IndexOf(fromFilePath.ToLower()) > -1)
                    {
                        if (PathGuard.IsFile(fromFilePath))
                        {
                            var checkFileName = azurePath.ToLower() == fromFilePath.ToLower();
                            var FileClient = containerClient.GetBlobClient(azurePath);
                            if (FileClient.Exists())
                            {
                                try
                                {
                                    var directoryPath = Path.GetDirectoryName(toFilePath);
                                    PathGuard.CreateDirectoryIfNotExists(directoryPath);
                                    FileClient.DownloadTo(toFilePath);
                                    copyResponseModel.Status = true;
                                }
                                catch (Exception ex)
                                {
                                    copyResponseModel.Status = false;
                                    copyResponseModel.Exception = ex;
                                }
                            }
                        }
                        else // its a directory so copy folder and its nested child
                        {
                            var installRootPathWithoutProgram = Directory.GetParent(Buildpath.InstallRootPath).FullName;
                            var fileDestinationPath = Path.Combine(installRootPathWithoutProgram, azurePath);
                            if (replaceMentRequired)
                            {
                                var replaceStartIndex = fileDestinationPath.ToLower().IndexOf(replacibleContent.ToLower());
                                var finalreplaceContent = fileDestinationPath.Substring(replaceStartIndex, replacibleContent.Length);
                                fileDestinationPath = fileDestinationPath.Replace(finalreplaceContent, replaceBy);
                            }
                            var directoryPath = Path.GetDirectoryName(fileDestinationPath);
                            PathGuard.CreateDirectoryIfNotExists(directoryPath);

                            var FileClient = containerClient.GetBlobClient(azurePath);
                            if (FileClient.Exists())
                            {
                                try
                                {
                                    FileClient.DownloadTo(fileDestinationPath);
                                    copyResponseModel.Status = true;
                                }
                                catch (Exception ex)
                                {
                                    copyResponseModel.Status = false;
                                    copyResponseModel.Exception = ex;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                fromFilePath = copyModel.from.ReplaceInstallPath();
                copyResponseModel.from = fromFilePath;
                copyResponseModel.to = toFilePath;
                if (PathGuard.IsFile(fromFilePath))
                {
                    if (File.Exists(fromFilePath))
                    {
                        try
                        {
                            var directoryInfo = Path.GetDirectoryName(toFilePath);
                            if (!Directory.Exists(directoryInfo))
                            {
                                Directory.CreateDirectory(directoryInfo);
                            }

                            File.Copy(fromFilePath, toFilePath, true);
                            copyResponseModel.Status = true;
                        }
                        catch (IOException e)
                        {
                            copyResponseModel.Status = false;
                            copyResponseModel.Exception = e;
                        }
                    }
                }
                else // its a directory so copy folder and its nested child
                {
                    if (Directory.Exists(fromFilePath))
                    {
                        try
                        {
                            FileHelpers.CopyDirectories(fromFilePath, toFilePath);
                        }
                        catch (Exception ex)
                        {
                            copyResponseModel.Status = false;
                            copyResponseModel.Exception = ex;
                        }
                    }
                }
            }

            return copyResponseModel;
        }

        private List<CopyResponseModel> CopyFiles(List<CopyModel> copyFiles, long timeStamp)
        {
            var returnValue = new List<CopyResponseModel>();
            foreach (var copyModel in copyFiles)
            {
                returnValue.Add(this.CopyFile(copyModel, timeStamp));
            }

            return returnValue;
        }

        private CopyResponseModel CopyFile(CopyModel copyModel, long timeStamp)
        {
            var copyResponseModel = new CopyResponseModel();
            var fromFilePath = copyModel.from.ReplaceLibraryPath();
            var toFilePath = copyModel.to.ReplaceInstallPath();
            if (fromFilePath.IndexOf(Buildpath.InstallPathText) > -1)
            {
                fromFilePath = copyModel.from.ReplaceInstallPath();
            }

            copyResponseModel.from = fromFilePath;
            copyResponseModel.to = toFilePath;
            if (PathGuard.IsFile(fromFilePath))
            {
                if (File.Exists(fromFilePath))
                {
                    try
                    {
                        var directoryInfo = Path.GetDirectoryName(toFilePath);
                        if (!Directory.Exists(directoryInfo))
                        {
                            Directory.CreateDirectory(directoryInfo);
                        }

                        File.Copy(fromFilePath, toFilePath, true);
                        copyResponseModel.Status = true;
                    }
                    catch (IOException e)
                    {
                        copyResponseModel.Status = false;
                        copyResponseModel.Exception = e;
                    }
                }
            }
            else // its a directory so copy folder and its nested child
            {
                if (Directory.Exists(fromFilePath))
                {
                    try
                    {
                        FileHelpers.CopyDirectories(fromFilePath, toFilePath);
                    }
                    catch (Exception ex)
                    {
                        copyResponseModel.Status = false;
                        copyResponseModel.Exception = ex;
                    }
                }
            }

            return copyResponseModel;
        }

        private List<MoveResponseModel> MoveFiles(List<MoveModel> moveFiles, long timeStamp)
        {
            var returnValue = new List<MoveResponseModel>();
            foreach (var moveModel in moveFiles)
            {
                returnValue.Add(this.MoveFile(moveModel, timeStamp));
            }

            return returnValue;
        }
        private MoveResponseModel MoveFile(MoveModel moveModel, long timeStamp)
        {
            var moveResponseModel = new MoveResponseModel();
            var fromFilePath = moveModel.from.ReplaceInstallPath();
            var toFilePath = moveModel.to.ReplaceInstallPath();

            moveResponseModel.from = fromFilePath;
            moveResponseModel.to = toFilePath;
            if (PathGuard.IsFile(fromFilePath))
            {
                if (File.Exists(fromFilePath))
                {
                    try
                    {
                        var directoryInfo = Path.GetDirectoryName(toFilePath);
                        if (!Directory.Exists(directoryInfo))
                        {
                            Directory.CreateDirectory(directoryInfo);
                        }

                        File.Copy(fromFilePath, toFilePath, true);
                        File.Delete(fromFilePath);
                        moveResponseModel.Status = true;
                    }
                    catch (IOException e)
                    {
                        moveResponseModel.Status = false;
                        moveResponseModel.Exception = e;
                    }
                }
            }
            else // its a directory so copy folder and its nested child
            {
                if (Directory.Exists(fromFilePath))
                {
                    try
                    {
                        FileHelpers.CopyDirectories(fromFilePath, toFilePath);
                        Directory.Delete(fromFilePath, true);
                        moveResponseModel.Status = true;
                    }
                    catch (Exception ex)
                    {
                        moveResponseModel.Status = false;
                        moveResponseModel.Exception = ex;
                    }
                }
            }

            return moveResponseModel;
        }

        private List<DeleteResponseModel> DeleteFiles(List<DeleteModel> files, long timeStamp)
        {
            var returnValue = new List<DeleteResponseModel>();
            foreach (var item in files)
            {
                returnValue.Add(this.DeleteFile(item, timeStamp));
            }

            return returnValue;
        }
        private DeleteResponseModel DeleteFile(DeleteModel item, long timeStamp)
        {
            var deleteResponseModel = new DeleteResponseModel();
            var filePath = item.path.ReplaceInstallPath();
            deleteResponseModel.path = filePath;
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    deleteResponseModel.Status = true;
                }
                catch (IOException e)
                {
                    deleteResponseModel.Status = false;
                    deleteResponseModel.Exception = e;
                }
            }
            else if (Directory.Exists(filePath))
            {
                try
                {
                    Directory.Delete(filePath, true);
                    deleteResponseModel.Status = true;
                }
                catch (IOException e)
                {
                    deleteResponseModel.Status = false;
                    deleteResponseModel.Exception = e;
                }
            }

            return deleteResponseModel;
        }

        private List<WriteCABResponseModel> WriteCABFiles(List<WriteCABModel> files, long timeStamp)
        {
            var returnValue = new List<WriteCABResponseModel>();
            CopyCABFolderFromAzure(files[0]);

            foreach (var item in files)
            {
                returnValue.Add(this.WriteCABFile(item, timeStamp));
            }

            return returnValue;
        }

        private void CopyCABFolderFromAzure(WriteCABModel firstItem)
        {
            var fromFilePathXML = firstItem.from.ReplaceLibraryPath();

            // Copy From Azure To Local 
            var list = this.ListAllBlobs<CloudBlockBlob, string>(b => b.Name, Buildpath.AzureContainerName, Buildpath.Program);
            var fromFilePath = fromFilePathXML.Substring(0, fromFilePathXML.LastIndexOf("\\"));
            var blobServiceClient = new BlobServiceClient(Buildpath.azureConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(Buildpath.AzureContainerName);

            for (int i = 0; i < list.Count(); i++)
            {
                var azurePath = list[i].Replace("/", "\\");
                if (azurePath.ToLower().IndexOf(fromFilePath.ToLower()) > -1)
                {
                    var installRootPathWithoutProgram = Directory.GetParent(Buildpath.InstallRootPath).FullName;
                    var fileDestinationPath = Path.Combine(installRootPathWithoutProgram, azurePath);
                    var directoryPath = Path.GetDirectoryName(fileDestinationPath);
                    PathGuard.CreateDirectoryIfNotExists(directoryPath);

                    var FileClient = containerClient.GetBlobClient(azurePath);
                    if (FileClient.Exists())
                    {
                        try
                        {
                            FileClient.DownloadTo(fileDestinationPath);
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
            }
        }
        private WriteCABResponseModel WriteCABFile(WriteCABModel item, long timeStamp)
        {
            var writeCABResponseModel = new WriteCABResponseModel();
            var fromFilePathLocal = item.from.ReplaceLibraryPath();
            var toFilePath = item.to;
            var userProfileFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile); ;
            var downloadFolder = Path.Combine(userProfileFolder, "Downloads");

            var lastDeliminatorIndexForConfig = fromFilePathLocal.LastIndexOf("\\");
            var configPath = fromFilePathLocal.Substring(0, lastDeliminatorIndexForConfig);

            var firstDeliminatorIndexForConfig = fromFilePathLocal.IndexOf("\\");
            var firstName = fromFilePathLocal.Substring(0, firstDeliminatorIndexForConfig);

            var lastDeliminatorIndex = toFilePath.LastIndexOf("\\");
            var cabName = toFilePath.Substring(lastDeliminatorIndex + 1);
            var cabMain = new CabMain();
            var installRootPathWithoutProgram = Directory.GetParent(Buildpath.InstallRootPath).FullName;
            var fileDestinationPath = Path.Combine(installRootPathWithoutProgram, fromFilePathLocal);
            var configLocalPath = Path.Combine(installRootPathWithoutProgram, fromFilePathLocal);
            var fullInstallPath = installRootPathWithoutProgram + "\\" + firstName;

            downloadFolder = installRootPathWithoutProgram + "\\" + "CABPackage";
            PathGuard.CreateDirectoryIfNotExists(downloadFolder);
            cabMain.GenerateDynamicCAB(fullInstallPath, downloadFolder, configLocalPath, cabName);

            //File.Copy(@System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).ToString().Substring(6) + "\\" + "lstHardDisk.exe", @downloadFolder+ "\\" + "lstHardDisk-" + DateTime.Now.Ticks.ToString()+ ".exe");
            return writeCABResponseModel;
        }

        private List<WriteXMLResponseModel> UpdateNodes(List<WriteXMLModel> updateNodes, long timeStamp)
        {
            var returnValue = new List<WriteXMLResponseModel>();
            foreach (var item in updateNodes)
            {
                returnValue.Add(this.UpdateNode(item, timeStamp));
            }

            return returnValue;
        }

        private WriteXMLResponseModel UpdateNode(WriteXMLModel item, long timeStamp)
        {
            var writeXMLResponseModel = new WriteXMLResponseModel();
            var filePath = item.file.ReplaceInstallPath();
            if (File.Exists(filePath))
            {
                try
                {
                    var doc = new XmlDocument();
                    doc.Load(filePath);
                    var nodes = doc.SelectNodes(item.xpath);
                    if (doc.ChildNodes.Count > 1)
                    {
                        if (!string.IsNullOrEmpty(doc.ChildNodes[1].NamespaceURI))
                        {
                            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(doc.NameTable);
                            namespaceManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
                            namespaceManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
                            namespaceManager.AddNamespace("ns", doc.ChildNodes[1].NamespaceURI);

                            var excludeAttribute = item.xpath.Substring(0, item.xpath.IndexOf("@") - 1);
                            var pathAttribute = item.xpath.Substring(item.xpath.IndexOf("@") + 1);
                            var xpathNew = excludeAttribute.Replace("/", "/ns:");
                            nodes = doc.SelectNodes(xpathNew, namespaceManager);

                            foreach (XmlNode node in nodes)
                            {
                                node.Attributes[pathAttribute].Value = item.value;
                            }
                        }
                        else
                        {
                            foreach (XmlNode node in nodes)
                            {
                                node.ChildNodes[0].Value = item.value;
                            }
                        }
                    }

                    if (nodes.Count == 0 && item.toCreate)
                    {
                        var defaultString = "[@value=";
                        //if (item.xpath.IndexOf(defaultString) == -1)
                        //{
                        //    defaultString = "[@key=";
                        //}

                        //  "/TerminalConfiguration/EMVDataObjectLists/AIDCandidates/EMVTag[@value='50']"
                        var extractXPath = item.xpath.Substring(0, item.xpath.IndexOf(defaultString));
                        extractXPath = extractXPath.Substring(0, extractXPath.LastIndexOf("/"));
                        var elementName = item.xpath.Substring(item.xpath.LastIndexOf("/") + 1, item.xpath.IndexOf(defaultString) - item.xpath.LastIndexOf("/") - 1);
                        var selectedNodes = doc.SelectNodes(extractXPath);
                        var attributeName = item.xpath.Substring(item.xpath.IndexOf("@") + 1, item.xpath.IndexOf("=") - item.xpath.IndexOf("@") - 1);
                        foreach (XmlNode node in selectedNodes)
                        {
                            XmlElement elem = doc.CreateElement(elementName);
                            XmlAttribute attr = doc.CreateAttribute(attributeName);
                            attr.Value = item.value;
                            elem.Attributes.Append(attr);
                            elem.InnerText = item.value;
                            node.AppendChild(elem);
                        }
                    }
                    doc.Save(filePath);

                    writeXMLResponseModel.Status = true;
                }
                catch (IOException e)
                {
                    writeXMLResponseModel.Status = false;
                    writeXMLResponseModel.Exception = e;
                }
            }
            else
            {
                writeXMLResponseModel.Status = false;
                writeXMLResponseModel.Exception = new Exception("File doesn't exists");
            }

            return writeXMLResponseModel;
        }

        private List<WriteXMLTableResponseModel> UpdatedXmlTableNode(WriteXMLTableModel item, long timeStamp)
        {
            var returnValue = new List<WriteXMLTableResponseModel>();

            var writeXMLResponseModel = new WriteXMLTableResponseModel();
            // @"C:\Users\Public\DeleteTest\test.txt")
            var filePath = item.file.ReplaceInstallPath();
            if (File.Exists(filePath))
            {
                try
                {
                    var doc = new XmlDocument();
                    doc.Load(filePath);

                    // Retrieve the title of every science-fiction movie.
                    var nodes = doc.SelectNodes(item.xpath);

                    foreach (XmlNode node in nodes)
                    {

                        // clean the nodes child nodes first
                        for (int i = node.ChildNodes.Count - 1; i >= 0; i--)
                        {
                            node.RemoveChild(node.ChildNodes[i]);
                        }
                        for (var i = 0; i < item.attributes.Count(); i++)
                        {
                            if (i > 0)
                            {
                                var newNode = doc.CreateElement(item.element);
                                newNode.SetAttribute("priority", item.attributes[i].attr[0]);
                                newNode.SetAttribute("address", item.attributes[i].attr[1]);
                                newNode.SetAttribute("portNumber", item.attributes[i].attr[2]);
                                node.AppendChild(newNode);
                            }
                        }
                    }
                    doc.Save(filePath);

                    writeXMLResponseModel.Status = true;
                }
                catch (IOException e)
                {
                    writeXMLResponseModel.Status = false;
                    writeXMLResponseModel.Exception = e;
                }
            }
            else
            {
                writeXMLResponseModel.Status = false;
                writeXMLResponseModel.Exception = new Exception("File doesn't exists");
            }
            returnValue.Add(writeXMLResponseModel);

            return returnValue;
        }


        // just the xml testing method
        public void testXmlNode(WriteXMLModel item)
        {
            try
            {
                var filePath = item.file.Replace(Buildpath.InstallPathText, @"E:\MO\Angular\CloudConfigAPI\CloudConfiguration.WebAPI\Install\637559459286835799\TemplateUnitTesting\SignedIX\SDVM\IXPayMain");
                var doc = new XmlDocument();
                doc.Load(filePath);
                var nodes = doc.SelectNodes(item.xpath);

                var str = new StringBuilder();

                foreach (XmlNode node in nodes)
                {
                    node.ChildNodes[0].Value = item.value;
                }
                if (nodes.Count == 0 && item.toCreate)
                {
                    var defaultString = "[@value=";
                    //if (item.xpath.IndexOf(defaultString) == -1)
                    //{
                    //    defaultString = "[@key=";
                    //}

                    //  "/TerminalConfiguration/EMVDataObjectLists/AIDCandidates/EMVTag[@value='50']"
                    var extractXPath = item.xpath.Substring(0, item.xpath.IndexOf(defaultString));
                    extractXPath = extractXPath.Substring(0, extractXPath.LastIndexOf("/"));
                    var elementName = item.xpath.Substring(item.xpath.LastIndexOf("/") + 1, item.xpath.IndexOf(defaultString) - item.xpath.LastIndexOf("/") - 1);
                    var selectedNodes = doc.SelectNodes(extractXPath);
                    var attributeName = item.xpath.Substring(item.xpath.IndexOf("@") + 1, item.xpath.IndexOf("=") - item.xpath.IndexOf("@") - 1);
                    foreach (XmlNode node in selectedNodes)
                    {
                        XmlElement elem = doc.CreateElement(elementName);
                        XmlAttribute attr = doc.CreateAttribute(attributeName);
                        attr.Value = item.value;
                        elem.Attributes.Append(attr);
                        elem.InnerText = item.value;
                        node.AppendChild(elem);
                    }
                }
                doc.Save(filePath);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
