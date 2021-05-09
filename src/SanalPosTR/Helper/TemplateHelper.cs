using DotLiquid;
using SanalPosTR.Configuration;
using SanalPosTR.Extensions;
using SanalPosTR.Model;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SanalPosTR
{
    public static class TemplateHelper
    {
        public static void initializeTemplate()
        {
            Template.RegisterFilter(typeof(TemplateMethodFilter));
            Template.NamingConvention = new DotLiquid.NamingConventions.CSharpNamingConvention();

            Template.RegisterSafeType(typeof(NestPayConfiguration), new[] { "*" });
            Template.RegisterSafeType(typeof(YKBConfiguration), new[] { "*" });
            Template.RegisterSafeType(typeof(CreditCardInfo), new[] { "*" });
            Template.RegisterSafeType(typeof(OrderInfo), new[] { "*" });
            Template.RegisterSafeType(typeof(Refund), new[] { "*" });
            Template.RegisterSafeType(typeof(PaymentModel), new[] { "*" });
            Template.RegisterSafeType(typeof(IEnvironmentConfiguration), new[] { "*" });
            
            
        }

        public static string ReadEmbedResource(string name)
        {
            string result = string.Empty;
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames().FirstOrDefault(x => x.ToLowerInvariant().Contains(name.ToLowerInvariant()));

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                result = reader.ReadToEnd();
            }

            return result;
        }


        public static string CompileOrderLink(this string value, PaymentModel orderInfo)
        {
            if (value.Contains("{{") && value.Contains("}}"))
            {
                var compiledTemplate = Template.Parse(value);
                return compiledTemplate.Render(Hash.FromAnonymousObject(orderInfo));
            }
            else
                return value;
        }

        public static string PrepaireXML(ViewModel model, string template1)
        {
            try
            {
                dynamic values = new ExpandoObject();

                values.Configuration = model.Configuration;
                values.Order = model.Order;
                values.CreditCard = model.CreditCard;
                values.Refund = model.Refund;
                values.Environment = model.Environment;

                if (model.Use3DSecure && model.Configuration is I3DConfiguration)
                {
                    var transform = (I3DConfiguration)model.Configuration;
                    values.SiteSuccessUrl = transform.SiteSuccessUrl.CompileOrderLink(model);
                    values.SiteFailUrl = transform.SiteFailUrl.CompileOrderLink(model);
                }

                if (model.Attributes.Count > 0)
                {
                    model.Attributes.ForEach(item =>
                    {
                        ((IDictionary<string, object>)values)[item.Key] = item.Value;
                    });
                }

                Template template = Template.Parse(template1); // Parses and compiles the template

                // string renderResult = template.Render(Hash.FromAnonymousObject(model));
                string renderResult = template.Render(Hash.FromDictionary(((IDictionary<string, object>)values)));

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