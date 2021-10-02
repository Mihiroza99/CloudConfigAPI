using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudConfiguration.WebAPI.Models
{
    public class TemplateModel
    {
        public string templateName { get; set; }
        public int templateId { get; set; }
        public int isProduction { get; set; }
    }
}