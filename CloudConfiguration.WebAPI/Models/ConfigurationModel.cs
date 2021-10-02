using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudConfiguration.WebAPI.Models
{
    public class ConfigurationModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string TemplatePath { get; set; }
        public int IsProduction { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
    }
}