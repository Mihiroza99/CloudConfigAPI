using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudConfiguration.WebAPI.Models
{
    public class ConfigurationResponseModel : BaseResponseModel
    {
        public List<ConfigurationModel> configurations { get; set; }
    }
}