using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;

namespace CloudConfiguration.WebAPI.Helpers
{
    class CabContentFile
    {
        //This class is for storing file list with its source path and destnation path

        private string _installDir;
        private string _shortCut;
        private string _destdir;
        private List<FileItem> _fileList = new List<FileItem>();

        public CabContentFile(string baseDir, string currDir, string destdir)
        {
            this._installDir = currDir;
            this._installDir = this._installDir.Replace(baseDir, "");
            this._shortCut = "File.Common" + this._installDir;
            this._shortCut = this._shortCut.Replace("\\", ".");
            this._destdir = destdir;
        }


        public void AddFileItem(FileInfo fi, string flag, string altName)
        {
            this._fileList.Add(new FileItem(fi.Name, flag, altName));
        }

        public string InstallDir
        {
            get { return this._installDir; }
        }

        public string Destdir
        {
            get { return this._destdir; }
        }

        public List<FileItem> FileList
        {
            get { return this._fileList; }
        }

    }

    public struct FileItem
    {
        public string name;
        public string flag;
        public string altname;

        public FileItem(string filename, string fileflag, string altName)
        {
            name = filename;
            flag = fileflag;
            altname = altName;
        }

    }
}