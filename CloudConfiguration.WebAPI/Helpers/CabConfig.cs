using System.Collections.Generic;
using System.Xml.Serialization;

namespace CloudConfiguration.WebAPI.Helpers
{
    //defines the structure of cabconfig xml
    public class CabConfig
    {
        [XmlElement("srcFolderPath")]
        public string srcFolderpath { get; set; }
        [XmlElement("destFolderPath")]
        public string destFolderPath { get; set; }
        [XmlElement("Build")]
        public List<Build> build { get; set; }
    }
    public class Build
    {
        [XmlElement("Name")]
        public string Name { get; set; }
        [XmlElement("DestinationFolder")]
        public List<DestinationFolder> destinationFolder { get; set; }
        [XmlElement("Excludefiles")]
        public List<string> Excludefiles { get; set; }
    }
    public class DestinationFolder
    {
        [XmlAttribute("FName")]
        public string FName { get; set; }

        [XmlElement("Source")]
        public string Source { get; set; }
    }
}