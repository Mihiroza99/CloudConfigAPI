using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudConfiguration.WebAPI.Helpers
{
    public class iXBlobServiceclient : BlobServiceClient
    {
        public iXBlobServiceclient() : base("DefaultEndpointsProtocol=https;AccountName=ixconfigtemplate;AccountKey=yApTue8cPgOMTMwO3t84F8Fueirtgzq3/89r/9ckJXsWqb7ZSa3JSYlWy8r5TAoe1OD4565nQZ8TTtlF8QUMxg==;EndpointSuffix=core.windows.net")
        {

        }
    }
}