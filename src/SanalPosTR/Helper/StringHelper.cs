using HandlebarsDotNet;
using SanalPosTR.Configuration;
using SanalPosTR.Extensions;
using SanalPosTR.Model;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SanalPosTR
{
    public class StringHelper
    {
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

        public static string PrepaireXML(ViewModel model, string template)
        {
            try
            {
                Handlebars.RegisterHelper("formatMoney", (writer, context, parameters) =>
                {
                    var value = Convert.ToDouble(parameters[0]);
                    writer.WriteSafeString(value.ToString("C2"));
                });

                Handlebars.RegisterHelper("formatMoneyUS", (writer, context, parameters) =>
                {
                    var value = Convert.ToDouble(parameters[0]);
                    writer.WriteSafeString(value.ToString(new CultureInfo("en-US")));
                });

                Handlebars.RegisterHelper("formatMoneyWithoutDecimal", (writer, context, parameters) =>
                {
                    var value = Convert.ToDouble(parameters[0]);
                    writer.WriteSafeString(value.ToString("0", new CultureInfo("en-US")));
                });

                Handlebars.RegisterHelper("formatInstallment", (writer, context, parameters) =>
                {
                    var value = Convert.ToInt32(parameters[0]);
                    if (value < 10)
                        writer.WriteSafeString($"0{value}");
                    else
                        writer.WriteSafeString(value.ToString());
                });

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

                var compiledTemplate = Handlebars.Compile(template);
                string renderResult = compiledTemplate(values);

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