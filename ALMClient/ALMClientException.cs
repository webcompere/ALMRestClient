using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ALMRestClient
{
    public class ALMClientException : Exception
    {
        public ALMClientException(HttpStatusCode statusCode, string error, string content)
            : base(error)
        {
            Error = error;
            Content = content;
        }

        public ALMClientException(HttpStatusCode statusCode, string error, string content, Exception e)
            : base(error, e)
        {
            Error = error;
            Content = content;
        }

        public string Error { get; set; }
        public string Content { get; set; }
    }
}
