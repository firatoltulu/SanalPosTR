﻿using RestSharp;
using System.Collections.Generic;

namespace SimplePayTR
{
    public class PostForm
    {
        public PostForm()
        {
        }

        public DataFormat RequestFormat { get; set; }

        public string ContentType { get; set; }

        public SendParameterType SendParameterType { get; set; } = SendParameterType.RequestBody;

        public string Content { get; set; }

        public string PreTag { get; set; }

        public List<SimplePayAttribute> Parameters { get; set; } = new List<SimplePayAttribute>();
    }

    public enum SendParameterType
    {
        RequestBody = 4,
        QueryString = 5
    }
}