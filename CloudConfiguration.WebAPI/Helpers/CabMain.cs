using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CloudConfiguration.WebAPI.Helpers
{
    public class CabMain
    {
        //// This class is for generating .inf file and thereby using it with the cabwiz process to generate cab files
        ////Srcpath - install path ( <from>)
        ////configPath - xml where we need to select(C:\Library_old\iXPayEPP 3.0.104.200\CabConfigs)
        ////destPath - download path(defaulkt directory download)
        ////cabname - from <to> tag after "/"
        public int GenerateDynamicCAB(string srcpath, string destPath, string configPath, string cabname)
        {
            try
            {
                Serializer ser = new Serializer();
                string path = string.Empty;
                string xmlInputData = string.Empty;
                string xmlOutputData = string.Empty;
                string strFileName = "";
                int inttotalcnt = 0;
                string[] _strsrcArray;
                //string[] _strdestArray;
                string ddfPath = "";
                string strtemp = "";
                string strmainsrcpath = "";
                string strmaindestdest = "";
                int processCode = 0;

                path = configPath;

                xmlInputData = File.ReadAllText(path);

                var deserializer = new XmlSerializer(typeof(CabConfig));
                var test = new CabConfig();
                using (var reader = XmlReader.Create(new StringReader(xmlInputData)))
                {
                    test = (CabConfig)deserializer.Deserialize(reader);
                }

                if (String.IsNullOrEmpty(srcpath) && String.IsNullOrEmpty(destPath))
                {
                    strmainsrcpath = test.srcFolderpath.ToString();
                    strmaindestdest = test.destFolderPath.ToString();
                }
                else
                {
                    strmainsrcpath = srcpath;
                    strmaindestdest = destPath;
                }

                var rootmainPath = System.Web.Hosting.HostingEnvironment.MapPath("//ThirdParty//");
                var roottempPath = rootmainPath + "WorkingDir";
                //Copy Files to Temp Path
                //DeleteDirectory("C:\\WorkingDir");
                //DeleteDirectory(roottempPath);
                //CopyDir(strmainsrcpath, "C:\\WorkingDir");
                CopyDir(strmainsrcpath, roottempPath);
                //strmainsrcpath = Environment.CurrentDirectory + "\\" + "WorkingDir";
                //strmainsrcpath = "C:" + "\\" + "WorkingDir";
                strmainsrcpath = roottempPath;

                for (int j = 0; j < test.build.Count; j++)
                {
                    Process process = new Process();
                    ProcessStartInfo startInfo = new ProcessStartInfo();

                    strFileName = cabname;
                    inttotalcnt = test.build[j].destinationFolder.Count;
                    _strsrcArray = new string[inttotalcnt];

                    for (int i = 0; i < inttotalcnt; i++)
                    {
                        _strsrcArray[i] = strmainsrcpath + (test.build[j]).destinationFolder[i].Source.ToString() + '~' + (test.build[j]).destinationFolder[i].FName.ToString();
                    }

                    if (strFileName.ToUpper().IndexOf(".CAB") >= 0)
                    {
                        strFileName = strFileName.Remove(strFileName.Length - 4, 4);
                    }
                    //Set path where file has been saved
                    ddfPath = Path.Combine(strmaindestdest, strFileName + ".cab");

                    var map = GetCabContentList(_strsrcArray);

                    string cabPath = Path.Combine(strmaindestdest, strFileName + ".inf");

                    string infFileName = cabPath;// strFileName + ".inf";

                    using (FileStream fs = new FileStream(cabPath, FileMode.Create, FileAccess.Write))
                    {
                        strtemp = CreateInfFile(fs, map, test, j, strmainsrcpath);
                    }

                    File.WriteAllText(ddfPath, strtemp, Encoding.Default);

                    //startInfo.WorkingDirectory = Environment.CurrentDirectory;
                    startInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).ToString().Substring(6); //Environment.CurrentDirectory;//"C:\\Users\\Mihir\\source\\repos\\CABTest\\bin";// "D:\\Projects\\Dheeraj\\gpmnt-ixconfigurator\\gpmnt-ixconfigurator\\Desktop\\bin\\x86\\Debug";//Environment.CurrentDirectory;
                    startInfo.CreateNoWindow = true;
                    //startInfo.FileName = "Cabwiz.exe";
                    startInfo.FileName = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).ToString().Substring(6) + "\\" + "Cabwiz.exe";
                    startInfo.Arguments = infFileName;// parameters;
                    startInfo.RedirectStandardOutput = true;
                    startInfo.RedirectStandardError = true;
                    startInfo.UseShellExecute = false;
                    process.StartInfo = startInfo;
                    //process.ErrorDataReceived += Process_ErrorDataReceived;
                    //process.OutputDataReceived += Process_OutputDataReceived;
                    process.StartInfo.UseShellExecute = false;
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                        processCode = process.ExitCode;
                }
                //Directory.Delete("C:\\WorkingDir", true);
                //Directory.Delete(roottempPath, true);

                System.IO.DirectoryInfo di = new DirectoryInfo(rootmainPath);
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }
                return processCode;
            }

            catch (Exception ex)
            {
                throw ex;

            }
        }

        private Dictionary<string, CabContentFile> GetCabContentList(string[] fileContentArray)
        {
            string str = "";

            List<string> fileNameList = new List<string>();
            //List<string> fileRenameList = new List<string>();
            Dictionary<string, List<FileLocation>> fileRenameList = new Dictionary<string, List<FileLocation>>();

            Dictionary<string, CabContentFile> cabContentMap = new Dictionary<string, CabContentFile>();
            for (int k = 0; k < fileContentArray.Length; k++)
            {
                if (fileContentArray[k] != null)
                {
                    str = fileContentArray[k].Substring(0, fileContentArray[k].IndexOf('~'));
                    List<FileInfo> fileList = GetDirectoryFiles(str);

                    if (fileList.Count > 0) // added condition to check for child folder
                    {
                        foreach (FileInfo file in fileList)
                        {
                            string key = file.DirectoryName;
                            if (cabContentMap.ContainsKey(key) == false)
                            {
                                CabContentFile newDir = new CabContentFile(fileContentArray[0].Substring(0, fileContentArray[0].IndexOf('~')), key, (fileContentArray[k].Substring(fileContentArray[k].IndexOf('~'))).Replace("~", ""));
                                cabContentMap.Add(key, newDir);
                            }

                            try
                            {
                                if (fileNameList.Contains(file.Name))
                                {
                                    int currCount = 1;
                                    if (!fileRenameList.ContainsKey(file.Name))
                                    {
                                        var fileLoc = new FileLocation(file.FullName, file.Name);
                                        fileRenameList.Add(file.Name, new List<FileLocation> { fileLoc });
                                    }
                                    else
                                    {
                                        currCount = fileRenameList[file.Name].Count + 1;
                                        var fileLoc = new FileLocation(file.FullName, file.Name);
                                        fileRenameList[file.Name].Add(fileLoc);
                                    }

                                    CabContentFile currItem = cabContentMap[key];
                                    currItem.AddFileItem(file, "0x40000000", file.Name + "." + currCount);
                                    fileNameList.Add(file.Name);
                                }
                                else
                                {
                                    CabContentFile currItem = cabContentMap[key];
                                    currItem.AddFileItem(file, "0x40000000", string.Empty);
                                    //Here 0x40000000 flag is used because it can be used for simultaneous read and write operations
                                    fileNameList.Add(file.Name);
                                }
                            }
                            catch (Exception err)
                            {
                                throw err;
                            }
                        }

                    }


                }

            }
            try
            {
                foreach (var kvp in fileRenameList)
                {
                    int index = 1;
                    foreach (var fileLoc in kvp.Value)
                    {
                        File.Move(fileLoc.FullName, fileLoc.FullName + "." + index);
                        index++;
                    }

                    //File.Move(file, file + ".1");
                }
                //fileRenameList.Clear();
            }
            catch (Exception err)
            {
                throw err;
            }

            return cabContentMap;
        }

        private string CreateInfFile(FileStream fs, Dictionary<string, CabContentFile> cabContentFileList, CabConfig test, int buildcount, string srcPath)
        {

            string excludefiles = "";
            int inttotalcnt = 0;

            StringBuilder sb = new StringBuilder();
            StringBuilder sbDefaultInstall = new StringBuilder();
            StringBuilder sbSourceDisksNames = new StringBuilder();
            StringBuilder sbSourceDisksFiles = new StringBuilder();
            StringBuilder sbDestinationDirs = new StringBuilder();
            StringBuilder sbFiles = new StringBuilder();

            // version
            sb.AppendLine("[Version]");
            sb.AppendLine("Signature=" + "$Windows NT$");
            sb.AppendLine("Provider=" + "Wayne");
            sb.AppendLine("CESignature=" + "$Windows CE$");
            sb.AppendLine();

            // cestrings
            sb.AppendLine("[CEStrings]");
            string appName = test.build[0].Name;
            sb.AppendLine("AppName=" + appName);
            sb.AppendLine("InstallDir=" + @"\Flash\");
            sb.AppendLine();

            // cedevice
            sb.AppendLine("[CEDevice]");
            sb.AppendLine("VersionMin=" + "3.0");
            sb.AppendLine("VersionMax=" + "6.99");
            sb.AppendLine();

            sbDefaultInstall.AppendLine("[DefaultInstall]");
            sbSourceDisksNames.AppendLine("[SourceDisksNames]");
            sbSourceDisksFiles.AppendLine("[SourceDisksFiles]");
            sbDestinationDirs.AppendLine("[DestinationDirs]");

            sbDefaultInstall.Append(@"CopyFiles=Files.Common.Content, ");


            int count = 1;

            List<string> fileAppList = new List<string>();

            foreach (KeyValuePair<string, CabContentFile> kvp in cabContentFileList)
            {
                CabContentFile folder = kvp.Value;
                if (count > 1)
                {
                    sbDefaultInstall.Append(@"Files.Common.Content" + kvp.Value.InstallDir + ", ");
                }
                sbDestinationDirs.AppendLine(@"Files.Common.Content" + kvp.Value.InstallDir + "=0,%InstallDir%\\" + kvp.Value.Destdir);
                sbFiles.AppendLine("[" + "Files.Common.Content" + kvp.Value.InstallDir + "]");

                // Check if xml contains excluded files
                if (test.build[buildcount].Excludefiles != null && test.build[buildcount].Excludefiles.Count > 0)
                {
                    //excluded files count
                    inttotalcnt = test.build[buildcount].Excludefiles.Count;
                    for (int i = 0; i < inttotalcnt; i++)
                    {
                        excludefiles += srcPath + (test.build[buildcount]).Excludefiles[i] + ",";
                    }
                    excludefiles = excludefiles.Remove(excludefiles.LastIndexOf(","));
                }


                foreach (FileItem fileItem in kvp.Value.FileList)
                {
                    string temp = kvp.Key + "\\" + fileItem.name;

                    // Here check if excluded files are there
                    if (excludefiles.Length > 0)
                    {
                        //if (test.build[buildcount].Excludefiles.Any(s => s.Equals(temp)))
                        if (excludefiles.IndexOf(temp) >= 0)
                        {
                            continue;
                        }
                    }
                    sbSourceDisksNames.AppendLine(count + "=,\"Common" + count + "\",,\"" + srcPath + "\"");

                    if (!String.IsNullOrEmpty(fileItem.altname))
                    {
                        sbSourceDisksFiles.AppendLine(fileItem.altname + "=" + count + "," + kvp.Key);
                        sbFiles.AppendLine(fileItem.name + "," + fileItem.altname + ",," + fileItem.flag);
                    }
                    else
                    {
                        sbSourceDisksFiles.AppendLine(fileItem.name + "=" + count + "," + kvp.Key);
                        sbFiles.AppendLine(fileItem.name + ",,," + fileItem.flag);
                    }
                }
                sbFiles.AppendLine();
                count++;
            }

            sbDefaultInstall.AppendLine();
            sb.AppendLine(sbDefaultInstall.ToString());
            sb.AppendLine(sbSourceDisksNames.ToString());
            sb.AppendLine(sbSourceDisksFiles.ToString());
            sb.AppendLine(sbDestinationDirs.ToString());
            sb.AppendLine(sbFiles.ToString());

            fs.Write(Encoding.ASCII.GetBytes(sb.ToString()), 0, sb.Length);
            return sb.ToString();
        }

        #region File Functions
        private List<FileInfo> GetDirectoryFiles(string path)
        {
            List<FileInfo> fList = new List<FileInfo>();
            CabConfig t = new CabConfig();
            fList.AddRange(GetFiles(path));

            //DirectoryInfo dInfo = new DirectoryInfo(path);
            //foreach (DirectoryInfo dir in dInfo.GetDirectories())
            //{
            //    fList.AddRange(GetDirectoryFiles(dir.FullName));
            //}

            return fList;
        }


        private List<FileInfo> GetFiles(string path)
        {
            List<FileInfo> fList = new List<FileInfo>();
            DirectoryInfo di = new DirectoryInfo(path);
            if (di.Exists) //added condition to check folder exists or not for given path
            {
                foreach (FileInfo fInfo in di.GetFiles())
                {
                    fList.Add(fInfo);
                }
            }
            return fList;
        }

        #endregion

        #region Utility Functions
        private void CopyDir(string source, string target)
        {
            if (!Directory.Exists(target)) Directory.CreateDirectory(target);
            string[] sysEntries = Directory.GetFileSystemEntries(source);

            foreach (string sysEntry in sysEntries)
            {
                string fileName = Path.GetFileName(sysEntry);
                string targetPath = Path.Combine(target, fileName);
                if (Directory.Exists(sysEntry))
                    CopyDir(sysEntry, targetPath);
                else
                {
                    File.Copy(sysEntry, targetPath, true);
                }
            }

        }

        private void DeleteDirectory(string target_dir)
        {
            if (!Directory.Exists(target_dir))
                return;

            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(target_dir, false);
        }

        internal class Serializer
        {
            public T Deserialize<T>(string input) where T : class
            {
                System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(typeof(T));

                using (StringReader sr = new StringReader(input))
                {
                    return (T)ser.Deserialize(sr);
                }
            }
        }

        #endregion
    }
}