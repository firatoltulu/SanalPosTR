using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanalPosTR
{
    public static class TemplateMethodFilter
    {
        public static string formatMoney(string input)
        {
            return decimal.Parse(input).ToString("C2");
        }

        public static string formatMoneyUS(string input)
        {
            return decimal.Parse(input).ToString(new CultureInfo("en-US"));
        }

        public static string formatMoneyWithoutDecimal(string input)
        {
            return decimal.Parse(input).ToString("0", new CultureInfo("en-US"));
        }

        public static string formatInstallment(string input)
        {
            var value = Convert.ToInt32(input);
            if (value < 10)
                return $"0{value}";
            else
                return value.ToString();
        }
    }
}
