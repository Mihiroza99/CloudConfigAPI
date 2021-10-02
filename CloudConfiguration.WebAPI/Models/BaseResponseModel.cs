using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudConfiguration.WebAPI.Models
{
    public class BaseResponseModel
    {
        public bool ResponseStatus { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }
    }
}