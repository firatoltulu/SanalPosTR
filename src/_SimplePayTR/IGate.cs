using System.Collections.Generic;
using System.Collections.Specialized;

namespace SimplePayTR
{
    public interface IGate
    {
        Result Pay(Request request);

        Result Refound(Request request);

        Result Pay3D(Request request, NameValueCollection collection);

        bool Check3D(NameValueCollection formCollection, Dictionary<string, object> accounts);

        List<NetworkConfigurationModel> GetNetworkConfiguration();
    }
}