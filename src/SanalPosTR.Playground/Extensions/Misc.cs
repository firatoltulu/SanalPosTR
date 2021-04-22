using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanalPosTR.Playground.Extensions
{
    public static class Misc
    {
        public static string ToJson<T>(this T value)
        {
            return Serialize.ToJson(value);
        }

        public static T FromJson<T>(this string value)
        {
            return Serialize.FromJson<T>(value);
        }

        public static T FromJson<T>(this Stream s)
        {
            using (StreamReader reader = new StreamReader(s, Encoding.UTF8))
            {
                var jsonString = reader.ReadToEnd();
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jsonString);
            }
        }
    }

    public static class Serialize
    {
        public static T FromJson<T>(string input)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            var serialize = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(input);
            return serialize;
        }

        public static string ToJson(object input)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            return Newtonsoft.Json.JsonConvert.SerializeObject(input);
        }
    }
}
