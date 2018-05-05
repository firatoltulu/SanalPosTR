using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace SimplePayTR
{
    public class Gateway : IGate
    {
        public Gateway(NetworkType itype)
        {
            Gate = Create(itype);
        }

        private IGate Gate { get; set; }

        public Result Pay(Request request)
        {
            return Gate.Pay(request);
        }

        public Result Refound(Request request)
        {
            return Gate.Refound(request);
        }

        private IGate Create(NetworkType itype)
        {
            IGate result = null;

            var asm = Assembly.GetExecutingAssembly().GetTypes();

            foreach (var type in asm)
            {
                //if (!type.IsClass || type.IsNotPublic) continue;

                Type[] interfaces = type.GetInterfaces();

                if (((IList)interfaces).Contains(typeof(IGate)))
                {
                    if (type.Name.Contains(itype.ToString()))
                    {
                        object obj = Activator.CreateInstance(type);
                        result = (IGate)obj;
                        break;
                    }
                }
            }

            return result;
        }

        public List<NetworkConfigurationModel> GetNetworkConfiguration()
        {
            return this.Gate.GetNetworkConfiguration();
        }

        public Result Pay3D(Request request, System.Collections.Specialized.NameValueCollection collection)
        {
            return this.Gate.Pay3D(request, collection);
        }

        public bool Check3D(System.Collections.Specialized.NameValueCollection formCollection, Dictionary<string, object> accounts)
        {
            return this.Gate.Check3D(formCollection, accounts);
        }
    }
}