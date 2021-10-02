using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudConfiguration.WebAPI.Models
{
    public class TemplateResponseModel : BaseResponseModel
    {
        public string templateContent { get; set; }
    }
}