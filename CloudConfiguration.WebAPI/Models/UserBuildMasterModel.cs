using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudConfiguration.WebAPI.Models
{
    public class UserBuildMasterModel
    {
            public int BuildId { get; set; }
            public int UserId { get; set; }
            public string SessionId { get; set; }
            public string Template { get; set; }
            public string BuildFile { get; set; }
            public DateTime CreatedDate { get; set; }
    }
}