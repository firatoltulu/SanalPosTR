using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SimplePayTR
{
    public class Model
    {
        public static string ReadEmbedXML(string name)
        {
            string result = string.Empty;
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = string.Format("SimplePayTR.XML.{0}", name);

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                result = reader.ReadToEnd();
            }

            return result;
        }

        public static string CreatePosXML(Request request, string content)
        {
            try
            {
                dynamic model = new System.Dynamic.ExpandoObject();

                IDictionary<string, object> property = new System.Dynamic.ExpandoObject();
                foreach (var item in request.Accounts)
                    property.Add(item.Key, item.Value);

                model.Pay = request.Pos;
                model.Account = (System.Dynamic.ExpandoObject)property;

                if (request.Is3D)
                {
                    dynamic extra3d = new System.Dynamic.ExpandoObject();
                    extra3d.Url = request.Url;
                    extra3d.SuccessUrl = request.SuccessUrl;
                    extra3d.ErrorUrl = request.ErrorUrl;
                    model.Extra = extra3d;
                }

                string renderResult = RazorEngine.Razor.Parse(content, model);

                return renderResult;
            }
            catch (Exception EX)
            {
                throw EX;
            }
        }

        public static string GetInlineContent(string post, string key)
        {
            string resultCode = string.Empty;

            string inlineContent = @"(<({0})[^>]*\/?>)(.*?)(<\/(?:\2)>)";
            var result = post.Match(string.Format(inlineContent, key));

            if (result.Success)
            {
                resultCode = result.Groups[3].Value;
            }

            if (string.IsNullOrEmpty(resultCode))
            {
                inlineContent = @"({0}=)(\x22)(.*?)(\x22)";
                result = post.Match(string.Format(inlineContent, key));
                if (result.Success)
                    resultCode = result.Groups[3].Value;
            }
            return resultCode;
        }

        public static string GetWrapContent(string post, string key)
        {
            string wrapContent = @"(<({0})[^>]*\/?>)[\w\S\s]*?(<\/(?:\2)>)";
            var result = post.Match(string.Format(wrapContent, key));
            if (result.Success)
                return result.Value.Remove(key).Trim();
            else
                return string.Empty;
        }

        public static string GetSHA1(string text)
        {
            return HashHelper.GetSHA1(text);
        }
    }
}