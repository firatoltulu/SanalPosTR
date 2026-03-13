using SanalPosTR.Model;
using System.Collections.Generic;

namespace SanalPosTR
{
    public enum RequestDataFormat
    {
        Xml,
        Json,
        None
    }

    public class PostForm
    {
        public PostForm()
        {
        }

        public RequestDataFormat RequestFormat { get; set; }

        public string ContentType { get; set; }

        public SendParameterType SendParameterType { get; set; } = SendParameterType.RequestBody;

        public string Content { get; set; }

        public string PreTag { get; set; }

        public List<SanalPosTRAttribute> Parameters { get; set; } = new List<SanalPosTRAttribute>();
    }

    public enum SendParameterType
    {
        RequestBody = 4,
        QueryString = 5
    }
}
