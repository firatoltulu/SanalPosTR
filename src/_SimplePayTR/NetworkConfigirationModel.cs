using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePayTR
{
    public class NetworkConfigurationModel
    {
        public string Key { get; set; }

        public string Value { get; set; }

        public string Description { get; set; }

        public NetworkType iType { get; set; }
    }


}
