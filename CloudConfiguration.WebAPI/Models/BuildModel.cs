using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudConfiguration.WebAPI.Models
{
    public class MainBuildModel
    {
        public string type { get; set; }
        public int index { get; set; }

        public object build { get; set; }
    }
    public class BuildModel
    {
        public List<MainBuildModel> build { get; set; }

        //public List<CopyModel> copy { get; set; }
        //public List<MoveModel> move { get; set; }
        //public List<DeleteModel> delete { get; set; }
        //public List<WriteXMLModel> writeXML { get; set; }
        public WriteXMLTableModel writeXMLTable { get; set; }
        //public List<WriteCABModel> writeCAB { get; set; }

        public string productPath { get; set; }
        public string serviceUtilityPath { get; set; }
        public int programId { get; set; }

        public int isProduction { get; set; }

        //public int BuildId { get; set; }
        public int UserId { get; set; }
        //public string SessionId { get; set; }
        public string Template { get; set; }
        public string BuildFile { get; set; }
        public List<SummaryJSONModel> SummaryJSON { get; set; }
        //public DateTime CreatedDate { get; set; }
    }

    public class SummaryJSONModel
    {
        public string Key { get; set; }
        public string Label { get; set; }
        public object Value { get; set; }
    }

    public class SummaryKeyValue
    {
        public string Key { get; set; }
        public object Value { get; set; }
    }

    public class BaseModel
    {

    }

    public class CopyModel : BaseModel
    {
        public string from { get; set; }
        public string to { get; set; }
    }

    public class MoveModel : BaseModel
    {
        public string from { get; set; }
        public string to { get; set; }
    }

    public class DeleteModel : BaseModel
    {
        public string path { get; set; }
    }

    public class WriteXMLModel : BaseModel
    {
        public string file { get; set; }
        public string xpath { get; set; }
        public string value { get; set; }
        public bool toCreate { get; set; }
    }

    public class WriteCABModel : BaseModel
    {
        public string from { get; set; }
        public string to { get; set; }
        public string overwrite { get; set; }
    }

    public class WriteXMLTableModel : BaseModel
    {
        public string file { get; set; }
        public string xpath { get; set; }
        public string element { get; set; }
        public List<AttributeModel> attributes { get; set; }
        public string condition { get; set; }
        public string table { get; set; }
    }

    public class AttributeModel
    {
        public List<string> attr { get; set; }

    }

    public class AttrModel
    {
        public string priority { get; set; }
        public string address { get; set; }
        public string portNumber { get; set; }

    }

    public class DeleteResponseModel : DeleteModel
    {
        public bool Status { get; set; }
        public Exception Exception { get; set; }
    }

    public class CopyResponseModel : CopyModel
    {
        public bool Status { get; set; }
        public Exception Exception { get; set; }
    }

    public class MoveResponseModel : MoveModel
    {
        public bool Status { get; set; }
        public Exception Exception { get; set; }
    }

    public class WriteXMLResponseModel : WriteXMLModel
    {
        public bool Status { get; set; }
        public Exception Exception { get; set; }
    }
    public class WriteCABResponseModel : WriteCABModel
    {
        public bool Status { get; set; }
        public Exception Exception { get; set; }
    }
    public class WriteXMLTableResponseModel : WriteXMLTableModel
    {
        public bool Status { get; set; }
        public Exception Exception { get; set; }
    }
    public class BuildResponseModel : BaseResponseModel
    {
        public string zipFilePath { get; set; }
        public List<CopyResponseModel> copy { get; set; }
        public List<MoveResponseModel> move { get; set; }
        public List<DeleteResponseModel> delete { get; set; }
        public List<WriteXMLResponseModel> writeXML { get; set; }
        public List<WriteCABResponseModel> writeCAB { get; set; }
        public List<WriteXMLTableResponseModel> writeXMLTable { get; set; }
    }

    public class ManifestModel
    {
        public int componentType { get; set; }
        public string fileName { get; set; }
    }

    public class ManifestList
    {
        public List<ManifestModel> files { get; set; }
    }
}