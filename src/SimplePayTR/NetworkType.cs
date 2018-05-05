using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePayTR
{
    public enum NetworkType
    {
        /// <summary>
        /// Genel
        /// </summary>
        EST,
        /// <summary>
        /// Garanti
        /// </summary>
        GB,
        /// <summary>
        /// Yapı Kredi
        /// </summary>
        YKB,
      
        Paypal,
        PayU,
        None,
        VPOS,
        /// <summary>
        /// Deniz Bank
        /// </summary>
        DB
    }
}
