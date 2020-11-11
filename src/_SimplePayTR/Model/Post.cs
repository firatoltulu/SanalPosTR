using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace SimplePayTR
{
    public class Post
    {
        public Post() {
            Method = RestSharp.Method.POST;
            Parameters = new Dictionary<string, object>();
        }

        public Request Request { get; set; }

        public DataFormat RequestFormat { get; set; }


        public string PreTag { get; set; }

        public string ConfigName { get; set; }

        public bool IsQueryParameter { get; set; }

        public RestSharp.Method Method { get; set; }

        public Dictionary<string, object> Parameters { get; set; }

    }
}
