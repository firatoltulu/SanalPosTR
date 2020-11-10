using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace SimplePayTR
{
    public static class StringHelpers
    {
        public static string RemoveEscape(string text, bool clearWhiteSpeace = false)
        {
            string tmp = text;

            tmp = tmp.Replace("ç", "c");
            tmp = tmp.Replace("ğ", "g");
            tmp = tmp.Replace("ı", "i");
            tmp = tmp.Replace("ö", "o");
            tmp = tmp.Replace("ş", "s");
            tmp = tmp.Replace("ü", "u");
            tmp = tmp.Replace("Ç", "C");
            tmp = tmp.Replace("Ğ", "G");
            tmp = tmp.Replace("İ", "I");
            tmp = tmp.Replace("Ö", "O");
            tmp = tmp.Replace("Ş", "S");
            tmp = tmp.Replace("Ü", "U");
            tmp = tmp.Replace("{", "");
            tmp = tmp.Replace("}", "");
            tmp = tmp.Replace("“", "");
            tmp = tmp.Replace("”", "");
            tmp = tmp.Replace("'", "");
            tmp = tmp.Replace('"', ' ');
            if (clearWhiteSpeace)
            {
                tmp = tmp.Clean(true, true, false, string.Empty, true);
            }
            tmp = tmp.Replace("‘", string.Empty);
            tmp = tmp.Replace("’", string.Empty);



            return tmp;
        }

        /// <summary>
        /// String Ayır Örneğin InProc yazdığınızda size In Proc Olarak Dönücektir
        /// </summary>
        /// <param name="camelCase"></param>
        /// <returns></returns>
        public static string FromCamelCase(this string camelCase)
        {
            if (camelCase == null)
                throw new ArgumentException("Lütfen Değer Giriniz");

            var sb = new StringBuilder(camelCase.Length + 10);
            bool first = true;
            char lastChar = '\0';

            foreach (char ch in camelCase)
            {
                if (!first &&
                     (char.IsUpper(ch) ||
                       char.IsDigit(ch) && !char.IsDigit(lastChar)))
                    sb.Append(' ');

                sb.Append(ch);
                first = false;
                lastChar = ch;
            }

            return sb.ToString(); ;
        }

        /// <summary>
        /// String Ayır Örneğin In Proc Yazdınız Size InProc Olarak Dönücektir
        /// </summary>
        /// <param name="phrase"></param>
        /// <returns></returns>
        public static string ToCamelCase(this string phrase)
        {
            if (phrase == null)
                return string.Empty;

            StringBuilder sb = new StringBuilder(phrase.Length);

            //ilk karakter her zmn büyük olmak zorunda.
            bool nextUpper = true;

            foreach (char ch in phrase)
            {
                if (char.IsWhiteSpace(ch) || char.IsPunctuation(ch) || char.IsSeparator(ch))
                {
                    nextUpper = true;
                    continue;
                }

                if (nextUpper)
                    sb.Append(char.ToUpper(ch));
                else
                    sb.Append(char.ToLower(ch));

                nextUpper = false;
            }

            return sb.ToString();
        }

        /// <summary>
        /// String Şeklindeki Bir Array Dizini Verilen chr  değerinde bicimlendirir örneğin 1;2;3;5 burda kullanılan chr = ; dir
        /// </summary>
        /// <param name="str"></param>
        /// <param name="chr"></param>
        /// <returns></returns>
        public static string ToStringFormat(this string[] str, string chr)
        {
            string rtn = string.Empty;
            foreach (string item in str)
            {
                rtn += item + chr;
            }
            rtn = rtn.Substring(0, rtn.Length - 1);
            return rtn;
        }



        public static bool FindStartWith(this string key, string value)
        {
            return key.StartsWith(value, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool FindEndWith(this string key, string value)
        {
            return key.EndsWith(value, StringComparison.InvariantCultureIgnoreCase);
        }





        public static string Highlight(this string s, string Search_str, string StartTag, string EndTag)
        {
            Regex RegExp = new Regex(Search_str.Replace(" ", "|").Trim(), RegexOptions.IgnoreCase);

            // Highlight keywords by calling the delegate each time a keyword is found.
            return RegExp.Replace(s, new MatchEvaluator(pk => StartTag + pk.Value + EndTag));
        }

        public static string FixHTMLForDisplay(this string Html)
        {
            Html = Html.Replace("<", "&lt;");
            Html = Html.Replace(">", "&gt;");
            Html = Html.Replace("\"", "&quot;");
            return Html;
        }

        public static string EncodeJsString(this string s)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\"");
            foreach (char c in s)
            {
                switch (c)
                {
                    case '\"':
                        sb.Append("\\\"");
                        break;
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    default:
                        int i = (int)c;
                        if (i < 32 || i > 127)
                        {
                            sb.AppendFormat("\\u{0:X04}", i);
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
            sb.Append("\"");

            return sb.ToString();
        }


        #region StringConstantsToHelpWithComparisons
        private const string m_Letters = "abcdefghijklmnopqrstuvwxyz";
        private const string m_Digits = "0123456789";
        private const string m_ForwardSlash = "/";
        private const string m_BackSlash = "\\";
        private const string m_Period = ".";
        private const string m_DollarSign = "$";
        private const string m_PercentSign = "%";
        private const string m_Comma = ",";
        private const string m_Yes = "yes";
        private const string m_No = "no";
        private const string m_True = "true";
        private const string m_False = "false";
        private const string m_1 = "1";
        private const string m_0 = "0";
        private const string m_y = "y";
        private const string m_n = "n";
        private const string m_special = "-()+*~";

        #endregion

        #region DataTypeStringConstants
        public const string m_GUID = "guid";
        public const string m_Boolean1 = "boolean";
        public const string m_Boolean2 = "bool";
        public const string m_Byte = "byte";
        public const string m_Char = "char";
        public const string m_DateTime = "datetime";
        public const string m_DBNull = "dbnull";
        public const string m_Decimal = "decimal";
        public const string m_Double = "double";
        public const string m_Empty = "empty";
        public const string m_Int16_1 = "int16";
        public const string m_Int16_2 = "short";
        public const string m_Int32_1 = "int32";
        public const string m_Int32_2 = "int";
        public const string m_Int32_3 = "integer";
        public const string m_Int64_1 = "int64";
        public const string m_Int64_2 = "long";
        public const string m_Object = "object";
        public const string m_SByte = "sbyte";
        public const string m_Single = "single";
        public const string m_String = "string";
        public const string m_UInt16 = "uint16";
        public const string m_UInt32 = "uint32";
        public const string m_UInt64 = "uint64";
        #endregion

        #region MethodsThatCheckDataType
        /// <summary>
        /// Evaluates whether passed-in string can be converted to a bool
        /// </summary>
        /// <param name="stream">string to check</param>
        /// <returns>
        /// bool indicating whether stream is a bool (0, 1, true/True,
        /// false/False)
        /// </returns>
        public static bool IsStandardBool(string stream)
        {
            try
            {
                if (stream == null || stream == string.Empty)
                    return false;
                stream = stream.Trim().ToLower();
                switch (stream)
                {
                    case m_0:
                        return true;
                    case m_1:
                        return true;
                    case m_True:
                        return true;
                    case m_False:
                        return true;
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                //ErrorTool.ProcessError(ex);
                return false;
            }
        }

        /// <summary>
        /// Evaluates whether string can can be COERCED to a bool
        /// </summary>
        /// <param name="stream">string to check</param>
        /// <returns>
        /// bool indicating whether argument is a standard or custom bool
        /// (0, 1, true/True, false/False) OR (y/Y, yes/Yes, n/N, no/NO)
        /// </returns>
        public static bool IsFriendlyBool(string stream)
        {
            try
            {
                if (stream == null || stream == string.Empty)
                    return false;
                stream = stream.Trim().ToLower();
                switch (stream)
                {
                    case m_0:
                        return true;
                    case m_1:
                        return true;
                    case m_True:
                        return true;
                    case m_False:
                        return true;
                    case m_n:
                        return true;
                    case m_y:
                        return true;
                    case m_No:
                        return true;
                    case m_Yes:
                        return true;
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                //ErrorTool.ProcessError(ex);
                return false;
            }
        }

        /// <summary>
        /// Returns a bool conversion of the passed in string
        /// </summary>
        /// <param name="stream">string to convert/coerce</param>
        /// <returns>
        /// bool representation of passed-in string
        /// </returns>
        public static bool CoerceToBool(string stream)
        {
            try
            {
                stream = stream.Trim().ToLower();
                switch (stream)
                {
                    case m_0:
                        return true;
                    case m_1:
                        return true;
                    case m_True:
                        return true;
                    case m_False:
                        return false;
                    case m_n:
                        return false;
                    case m_y:
                        return true;
                    case m_No:
                        return false;
                    case m_Yes:
                        return true;
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                //ErrorTool.ProcessError(ex);
                return false;
            }
        }

        /// <summary>
        /// Evaluates whether passed-in string contains any characters/
        /// digits/symbols. Trims spaces before checking.
        /// </summary>
        /// <param name="stream">string to check</param>
        /// <returns>
        /// bool indicating whether argument is void of characters/
        /// digits/symbols
        ///</returns>
        public static bool IsEmpty(string stream)
        {
            try
            {
                if (stream == null || stream == string.Empty
                || stream.Trim() == string.Empty)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                //ErrorTool.ProcessError(ex);
                return false;
            }
        }

        /// <summary>
        /// Checks each character of the string for any character other
        /// than a digit, or a dollar sign or a percentage sign. If any
        /// are found, returns false indicating that the stream is NOT
        /// a number
        /// </summary>
        /// <param name="stream">
        /// The stream of characters (string) to check
        /// </param>
        /// <returns>
        /// True/False value indicating whether the string can be
        /// coerced to a number
        /// </returns>
        public static bool IsNumber(string stream)
        {
            try
            {
                if (stream == null || stream == string.Empty)
                    return false;

                string character = string.Empty;
                //set a string up of all characters that may indicate
                //that the stream is a number, or a formatted number:
                string validCharacters = m_Digits + m_Period +
                m_DollarSign + m_Comma;
                for (int i = 0; i < stream.Length; i++)
                {
                    character = stream.Substring(i, 1);
                    if (!validCharacters.Contains(character))
                        return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                //ErrorTool.ProcessError(ex);
                return false;
            }
        }

        /// <summary>
        /// Checks the string to see whether it is a number & if it is,
        /// then it checks whether there is formatting applied to that #
        /// </summary>
        /// <param name="stream">
        /// The stream of characters (string) to check
        /// </param>
        /// <returns>
        /// True/False value indicating whether the string is a number
        /// that is formatted (contains digits and number formatting)
        /// </returns>
        public static bool IsFormattedNumber(string stream)
        {
            try
            {
                if (stream == null || stream == string.Empty)
                    return false;

                string character = string.Empty;
                //set a string up of all characters that may indicate that
                //the stream is a number, or a formatted number:
                string validCharacters = m_Digits + m_Period +
                m_DollarSign + m_PercentSign + m_Comma;

                for (int i = 0; i < stream.Length; i++)
                {
                    character = stream.Substring(i, 1);
                    if (!validCharacters.Contains(character))
                        //the stream contains non-numeric characters:
                        return false;
                }
                //at this point, each single character is a number OR an
                //allowable symbol, but we must see whether those
                //characters contain a formatting character:
                string formattingCharacters = m_DollarSign +
                m_PercentSign + m_Comma;
                for (int i = 0; i < stream.Length; i++)
                {
                    if (formattingCharacters.Contains(character))
                        return true;
                }
                //still here? then the stream is a number, but NOT a
                //formatted number
                return false;
            }
            catch (Exception ex)
            {
                //ErrorTool.ProcessError(ex);
                return false;
            }
        }

        /// <summary>
        /// Checks whether string can be coerced into a DateTime value
        /// </summary>
        /// <param name="stream">The string to check/// </param>
        /// <returns>
        /// bool indicating whether stream can be converted to a date
        /// </returns>
        public static bool IsDate(string stream)
        {
            try
            {
                if (stream == null || stream == string.Empty)
                    return false;
                DateTime checkDate = new DateTime();
                bool dateType = true;
                try
                {
                    checkDate = DateTime.Parse(stream);
                }
                catch
                {
                    dateType = false;
                }
                return dateType;
            }
            catch (Exception ex)
            {
                //ErrorTool.ProcessError(ex);
                return false;
            }
        }

        /// <summary>
        /// Checks the string to see whether it is a number and if it is,
        /// then it checks whether there is a decimal in that number.
        /// </summary>
        /// <param name="stream">
        /// The stream of characters (string) to check
        /// </param>
        /// <returns>
        /// True/False value indicating whether the string is a
        /// double---must be a number, and include a decimal
        /// in order to pass the test
        /// </returns>
        public static bool IsDouble(string stream)
        {
            try
            {
                if (stream == null || stream == string.Empty)
                    return false;

                if (!IsNumber(stream))
                    return false;

                //at this point each character is a number OR an allowable
                //symbol; we must see whether the string holds the decimal:
                if (stream.Contains(m_Period))
                    return true;

                //still here? the stream is a #, but does NOT have a decimal
                return false;
            }
            catch (Exception ex)
            {
                //ErrorTool.ProcessError(ex);
                return false;
            }
        }

        /// <summary>
        /// Checks string to see if it matches a TypeCode string and returns
        /// that TypeCode, or returns TypeCode.Empty if there is no match
        /// </summary>
        /// <param name="dataTypeString">
        /// String representation of a TypeCode (string, int, bool...)
        /// </param>
        /// <returns>TypeCode that maps to the dataTypeString</returns>
        public static TypeCode GetDataType(string dataTypeString)
        {
            try
            {
                switch (dataTypeString.ToLower())
                {
                    // todo: isn't there a better way for guid?
                    case m_GUID:
                        return TypeCode.Object;
                    case m_Boolean1:
                        return TypeCode.Boolean;
                    case m_Boolean2:
                        return TypeCode.Boolean;
                    case m_Byte:
                        return TypeCode.Byte;
                    case m_Char:
                        return TypeCode.Char;
                    case m_DateTime:
                        return TypeCode.DateTime;
                    case m_DBNull:
                        return TypeCode.DBNull;
                    case m_Decimal:
                        return TypeCode.Decimal;
                    case m_Double:
                        return TypeCode.Double;
                    case m_Empty:
                        return TypeCode.Empty;
                    case m_Int16_1:
                        return TypeCode.Int16;
                    case m_Int16_2:
                        return TypeCode.Int16;
                    case m_Int32_1:
                        return TypeCode.Int32;
                    case m_Int32_2:
                        return TypeCode.Int32;
                    case m_Int32_3:
                        return TypeCode.Int32;
                    case m_Int64_1:
                        return TypeCode.Int64;
                    case m_Int64_2:
                        return TypeCode.Int64;
                    case m_Object:
                        return TypeCode.Object;
                    case m_SByte:
                        return TypeCode.SByte;
                    case m_Single:
                        return TypeCode.Single;
                    case m_String:
                        return TypeCode.String;
                    case m_UInt16:
                        return TypeCode.UInt16;
                    case m_UInt32:
                        return TypeCode.UInt32;
                    case m_UInt64:
                        return TypeCode.UInt64;
                    default:
                        return TypeCode.Empty;
                }
            }
            catch (Exception ex)
            {
                //ErrorTool.ProcessError(ex);
                return TypeCode.Empty;
            }
        }
        #endregion

        #region StringConversions
        /// <summary>
        /// Returns a date, as coerced from the string argument. Will raise
        /// an error, if the string cannot be coerced
        /// ----so, use IsDate in this same class FIRST
        /// </summary>
        /// <param name="stream">string to get date value from</param>
        /// <returns>a DateTime object</returns>
        public static DateTime GetDate(string stream)
        {
            DateTime dateValue = new DateTime();
            try
            {
                dateValue = DateTime.Parse(stream);
                return dateValue;
            }
            catch (Exception ex)
            {
                //ErrorTool.ProcessError(ex);
                return dateValue;
            }
        }

        /// <summary>
        /// Returns an int, as coerced from the string argument.
        /// Will raise an error, if the string cannot be coerced
        /// ----so, use IsNumber in this same class FIRST
        /// </summary>
        /// <param name="stream">string to get int value from</param>
        /// <returns>an int object</returns>
        public static int GetInteger(string stream)
        {
            try
            {
                int number = 0;
                if (!IsNumber(stream))
                    return number;
                //still here? check to see if it is formatted:
                if (IsFormattedNumber(stream))
                {
                    //it's formatted; replace the format characters
                    //with nothing (retain the decimal so as not to change
                    //the intended value
                    stream = stream.Replace(m_Comma, string.Empty);
                    stream = stream.Replace(m_DollarSign, string.Empty);
                    stream = stream.Replace(m_PercentSign, string.Empty);
                }
                //we've removed superfluous formatting characters, if they
                //did exist, now let's round it/convert it, and return it:
                number = Convert.ToInt32(stream);
                return number;
            }
            catch (Exception ex)
            {
                //ErrorTool.ProcessError(ex);
                return 0;
            }
        }

        /// <summary>
        /// Returns a double, as coerced from the string argument.
        /// Will raise an error, if the string cannot be coerced
        /// ----so, use IsNumber in this same class FIRST
        /// </summary>
        /// <param name="stream">string to get double value from</param>
        /// <returns>a double object</returns>
        public static double GetDouble(string stream)
        {
            try
            {
                double number = 0;
                if (!IsNumber(stream))
                    return number;
                //still here? check to see if it is formatted:
                if (IsFormattedNumber(stream))
                {
                    //it's formatted; replace the format characters
                    //with nothing (retain the decimal so as not to change
                    //the intended value)
                    stream = stream.Replace(m_Comma, string.Empty);
                    stream = stream.Replace(m_DollarSign, string.Empty);
                    stream = stream.Replace(m_PercentSign, string.Empty);
                }

                //we've removed superfluous formatting characters, if they
                //did exist, now let's round it/convert it, and return it:
                number = Convert.ToDouble(stream);
                return number;
            }
            catch (Exception ex)
            {
                //ErrorTool.ProcessError(ex);
                return 0;
            }
        }
        #endregion

        #region StringEdits
        /// <summary>
        /// Iterates thru an entire string, and sets the first letter of
        /// each word to uppercase, and all ensuing letters to lowercase
        /// </summary>
        /// <param name="stream">The string to alter the case of</param>
        /// <returns>
        /// Same string w/uppercase initial letters & others as lowercase
        /// </returns>
        public static string MixCase(string stream)
        {
            try
            {
                string newString = string.Empty;
                string character = string.Empty;
                string preceder = string.Empty;
                for (int i = 0; i < stream.Length; i++)
                {
                    character = stream.Substring(i, 1);
                    if (i > 0)
                    {
                        //look at the character immediately before current
                        preceder = stream.Substring(i - 1, 1);
                        //remove white space character from predecessor
                        if (preceder.Trim() == string.Empty)
                            //the preceding character WAS white space, so
                            //we'll change the current character to UPPER
                            character = character.ToUpper();
                        else
                            //the preceding character was NOT white space,
                            //we'll force the current character to LOWER
                            character = character.ToLower();
                    }
                    else
                        //index is 0, thus we are at the first character
                        character = character.ToUpper();
                    //add the altered character to the new string:
                    newString += character;
                }
                return newString;
            }
            catch (Exception ex)
            {
                //ErrorTool.ProcessError(ex);
                return null;
            }
        }

        /// <summary>
        /// Iterates thru a string, and removes anything set to clean.
        /// Except---Does NOT remove anything in exceptionsToAllow
        /// </summary>
        /// <param name="stream">
        /// The string to clean</param>
        /// <returns>
        /// The same string, missing all elements that were set to clean
        /// (except when a character was listed in exceptionsToAllow)
        /// </returns>
        public static string Clean(this string stream, bool cleanWhiteSpace,
            bool cleanDigits, bool cleanLetters, string exceptionsToAllow, bool cleanSpecial)
        {
            try
            {
                string newString = string.Empty;
                string character = string.Empty;
                string blessed = string.Empty;
                if (!cleanDigits)
                    blessed += m_Digits;

                if (!cleanLetters)
                    blessed += m_Letters;

                if (!cleanSpecial)
                    blessed += m_special;

                blessed += exceptionsToAllow;
                //we set the comparison string to lower
                //and will compare each character's lower case version
                //against the comparison string, without
                //altering the original case of the character
                blessed = blessed.ToLower();
                for (int i = 0; i < stream.Length; i++)
                {
                    character = stream.Substring(i, 1);
                    if (blessed.Contains(character.ToLower()))
                        //add the altered character to the new string:
                        newString += character;
                    else if (character.Trim() == string.Empty &&
                    !cleanWhiteSpace)
                        newString += character;
                }
                return newString;
            }
            catch (Exception ex)
            {
                //ErrorTool.ProcessError(ex);
                return null;
            }
        }
        #endregion

        #region StringLocators
        /// <summary>
        /// Parses a file system or url path, and locates the file name
        /// </summary>
        /// <param name="fullPath">
        /// String indicating a file system or url path to a file
        /// </param>
        /// <param name="includeFileExtension">
        /// Whether to return file extension in addition to file name
        /// </param>
        /// <returns>
        /// File name, if found, and extension if requested, and located
        /// </returns>
        public static string GetFileNameFromPath(string fullPath,
            bool includeFileExtension)
        {
            try
            {
                bool url = fullPath.Contains(m_ForwardSlash);
                string search = string.Empty;
                if (url)
                    search = m_ForwardSlash;
                else
                    search = m_BackSlash;
                string portion = string.Empty;

                int decimals = GetKeyCharCount(fullPath, m_Period);
                if (decimals >= 1)
                    //get all text to the RIGHT of the LAST slash:
                    portion = GetExactPartOfString(fullPath, search, false,
                        false, false);
                else
                    return string.Empty;

                if (includeFileExtension)
                    return portion;
                search = m_Period;
                portion = GetExactPartOfString(portion, search, false,
                    true, false);
                return portion;
            }
            catch (Exception ex)
            {
                //ErrorTool.ProcessError(ex);
                return null;
            }
        }

        /// <summary>
        /// Parses a url or file stream string, to get and return the
        /// path portion (sans the file name and extension)
        /// </summary>
        /// <param name="fullPath">
        /// A string indicating a file system path or a url. Can
        /// contain a file name/extension.
        /// </param>
        /// <returns>
        /// The original path minus the file name and extension,
        /// if it had existed, with no extension will return
        /// the original string, plus an optional slash
        /// </returns>
        public static string GetFolderPath(string fullPath)
        {
            try
            {
                bool url = fullPath.Contains(m_ForwardSlash);
                string slash = string.Empty;
                if (url)
                    slash = m_ForwardSlash;
                else
                    slash = m_BackSlash;

                string fileName = GetFileNameFromPath(fullPath, true);
                //use tool to return all text to the LEFT of the file name
                string portion = GetStringBetween(fullPath, string.Empty,
                    fileName);

                //add the pertinent slash to the end of the string;
                if (portion.Length > 0 && portion.Substring(
                    portion.Length - 1, 1) != slash)
                    portion += slash;
                return portion;
            }
            catch (Exception ex)
            {
                //ErrorTool.ProcessError(ex);
                return null;
            }
        }

        /// <summary>
        /// Useful to pinpoint exact string between whatever
        /// characters/string you wish to grab text from
        /// </summary>
        /// <param name="stream">
        /// string from which to cull subtext from
        /// </param>
        /// <param name="from">
        /// string that precedes the text you are looking for
        /// </param>
        /// <param name="to">
        /// string that follows the text you are looking for
        /// </param>
        /// <returns>
        /// The string between point x and point y
        /// </returns>
        public static string GetStringBetween(string stream, string from,
            string to)
        {
            try
            {
                string subField = string.Empty;
                subField = RightOf(stream, from);
                subField = LeftOf(subField, to);
                return subField;
            }
            catch (Exception ex)
            {
                //ErrorTool.ProcessError(ex);
                return null;
            }
        }

        /// <summary>
        /// Will return the text to the LEFT of indicated substring
        /// </summary>
        /// <param name="stream">
        /// string from which to cull a portion of text
        /// </param>
        /// <param name="stringToStopAt">
        /// string that indicates what char or string to stop at
        /// </param>
        /// <returns>
        /// The string to the left of point x (stringToStopAt)
        /// </returns>
        public static string LeftOf(string stream, string stringToStopAt)
        {
            try
            {
                if (stringToStopAt == null || stringToStopAt == string.Empty)
                    return stream;

                int stringLength = stream.Length;
                int findLength = stringToStopAt.Length;

                stringToStopAt = stringToStopAt.ToLower();
                string temp = stream.ToLower();
                int i = temp.IndexOf(stringToStopAt);

                if ((i <= -1) && (stringToStopAt != temp.Substring(0, findLength))
                || (i == -1))
                    return stream;

                string result = stream.Substring(0, i);
                return result;
            }
            catch (Exception ex)
            {
                //ErrorTool.ProcessError(ex);
                return null;
            }
        }

        /// <summary>
        /// Will return the text to the RIGHT of whatever substring you indicate
        /// </summary>
        /// <param name="stream">
        /// string from which to cull a portion of text
        /// </param>
        /// <param name="stringToStartAfter">
        /// string that indicates what char or string to start after
        /// </param>
        /// <returns>
        /// The string to the right of point x (stringToStartAfter)
        /// </returns>
        public static string RightOf(string stream, string stringToStartAfter)
        {
            try
            {
                if (stringToStartAfter == null || stringToStartAfter == string.Empty)
                    return stream;
                stringToStartAfter = stringToStartAfter.ToLower();
                string temp = stream.ToLower();
                int findLength = stringToStartAfter.Length;
                int i = temp.IndexOf(stringToStartAfter);
                if ((i <= -1) && (stringToStartAfter != temp.Substring(0, findLength))
                || (i == -1))
                    return stream;

                string result =
                stream.Substring(i + findLength, stream.Length - (i + findLength));
                return result;
            }
            catch (Exception ex)
            {
                //ErrorTool.ProcessError(ex);
                return null;
            }
        }

        /// <summary>
        /// Searches a string for every single instance of the passed-in
        /// field delimiters, and returns all the values between those
        /// delimiters, as a List object
        /// </summary>
        /// <param name="streamToSearch">string to search</param>
        /// <param name="leftFieldDelimiter">string to start at</param>
        /// <param name="rightFieldDelimiter">string to stop at</param>
        /// <returns>A List object of strings</returns>
        public static List<string> GetEachFieldValue(string streamToSearch,
            string leftFieldDelimiter, string rightFieldDelimiter)
        {
            string search = streamToSearch;
            string field = string.Empty;
            List<string> fields = new List<string>();
            while (!string.IsNullOrEmpty(search)
            && search.Contains(leftFieldDelimiter)
            && search.Contains(rightFieldDelimiter))
            {
                //get the val and add to list
                field = GetStringBetween(search, leftFieldDelimiter,
                    rightFieldDelimiter);
                if (!string.IsNullOrEmpty(field))
                    fields.Add(field);
                //shorten the search string and continue
                search = RightOf(search, field + rightFieldDelimiter);
            }
            return fields;
        }

        /// <summary>
        /// Instructions on using arguments:
        /// Set firstInstance = true, to stop at first instance of locateChar
        /// If firstInstance = false, then the LAST instance of locateChar will be used
        /// Set fromLeft = true, to return string from the left of locateChar
        /// If fromLeft = false, then the string from the right of locateChar
        /// will be returned.
        /// Set caseSensitive to true/false for case-sensitivity
        /// EXAMPLES:
        /// GetPartOfString('aunt jemima', 'm', 'true', 'true')
        /// will return 'aunt je'
        /// GetPartOfString('aunt jemima', 'm', 'true', 'false')
        /// </summary>
        /// <param name="stream">
        /// The string from which to cull a portion of text
        /// </param>
        /// <param name="locateChar">
        /// The character or string that is the marker
        /// for which to grab text (from left or right depending
        /// on other argument)
        /// </param>
        /// <param name="firstInstance">
        /// Whether or not to get the substring from the first
        /// encountered instance of the locateChar argument
        /// </param>
        /// <param name="fromLeft">
        /// Whether to search from the left. If set to false,
        /// then the string will be searched from the right.
        /// </param>
        /// <param name="caseSensitive">
        /// Whether to consider case (upper/lower)
        /// </param>
        /// <returns>
        /// A portion of the input string, based on ensuing arguments
        /// </returns>
        public static string GetExactPartOfString(string stream, string locateChar,
                  bool firstInstance, bool fromLeft, bool caseSensitive)
        {
            try
            {
                stream = stream.ToString();
                string tempStream = string.Empty;
                string tempLocateChar = string.Empty;
                if (!caseSensitive)
                { //case doesn't matter, convert to lower:
                    tempStream = stream.ToLower();
                    tempLocateChar = locateChar.ToLower();
                }
                //default charCnt to 1; for first inst of locateChar:
                int charCount = 1;
                if (firstInstance == false)
                    //get number of times char exists in string:
                    if (caseSensitive)
                        charCount = GetKeyCharCount(stream, locateChar);
                    else
                        charCount = GetKeyCharCount(tempStream, tempLocateChar);
                //get position of first/last inst of char in str:
                int position = 0;
                if (caseSensitive)
                    position = GetCharPosition(stream, locateChar, charCount);
                else
                    position = GetCharPosition(tempStream, tempLocateChar, charCount);
                string result = string.Empty;
                //chk that character exists in str:
                if (position == -1)
                    result = string.Empty;
                else
                {
                    //char exists, proceed:
                    int streamLength = stream.Length;
                    if (fromLeft == true)
                        //return string from left:
                        result = stream.Substring(0, position);
                    else
                    {
                        //return string from right:
                        position += 1;
                        result = stream.Substring(position, streamLength - position);
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                //ErrorTool.ProcessError(ex);
                return null;
            }
        }

        /// <summary>
        /// Returns the number of times, that the key character is found
        /// in the stream string
        /// </summary>
        /// <param name="stream">
        /// string in which to locate key character
        /// </param>
        /// <param name="keyChar">
        /// key character: the string or char to count inside the stream
        /// </param>
        /// <returns>
        /// The number of times the string or char was located
        /// </returns>
        public static int GetKeyCharCount(string stream, string keyChar)
        {
            try
            {
                string current;
                int keyCount = 0;
                for (int i = 0; i < stream.Length; i++)
                {
                    current = stream.Substring(i, 1);
                    if (current == keyChar)
                        keyCount += 1;
                }
                if (keyCount <= 0)
                    return -1;
                else
                    return keyCount;
            }
            catch (Exception ex)
            {
                //ErrorTool.ProcessError(ex);
                return -1;
            }
        }

        /// <summary>
        /// Is CASE-SENSITIVE
        /// Returns x position of sChar in sstream, where x = iCharInst.
        /// If: getCharPos('pineapple', 'p', 3) Then: 6 is returned
        /// </summary>
        /// <param name="stream">
        /// string in which to pinpoint the character (or string) position
        /// </param>
        /// <param name="charToPinpoint">character or string to locate</param>
        /// <param name="whichCharInstance">
        /// Number indicating WHICH instance of the character/string to locate
        /// </param>
        /// <returns>
        /// The index of the character or string found inside the input string.
        /// Will return -1 if the string/character is not found, or if the
        /// instance number is not found
        /// </returns>
        public static int GetCharPosition(string stream, string charToPinpoint, int whichCharInstance)
        {
            try
            {
                string current;
                int keyCharCount = 0;
                for (int i = 0; i < stream.Length; i++)
                {
                    current = stream.Substring(i, 1);
                    //was BLOCKED SCRIPT sCurr = sstream.charAt(i);
                    if (current == charToPinpoint)
                    {
                        keyCharCount += 1;
                        if (keyCharCount == whichCharInstance)
                            return i;
                    }
                }
                return -1;
            }
            catch (Exception ex)
            {
                //ErrorTool.ProcessError(ex);
                return -1;
            }
        }
        #endregion

        private static Dictionary<String, Regex> cache = new Dictionary<string, Regex>();

        private static Regex cacheRegex(string r)
        {

            if (!cache.ContainsKey(r))
                cache[r] = new Regex(r, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            return cache[r];
        }

        public static bool IsMatch(this string s, string regex)
        {
            Regex r = cacheRegex(regex);
            return r.IsMatch(s);
        }

        public static MatchCollection Matches(this string s, string regex)
        {
            Regex r = cacheRegex(regex);
            return r.Matches(s);
        }

        public static Match Match(this string s, string regex)
        {
            Regex r = cacheRegex(regex);
            return r.Match(s);
        }

        public static bool MatchBool(this string s, string regex)
        {
            Regex r = cacheRegex(regex);
            return r.Match(s).Success;
        }

        public static string[] Split(this string s, string regex)
        {
            Regex r = cacheRegex(regex);
            return r.Split(s);
        }

        public static string Replacer(this string s, string regex, string newstring)
        {
            Regex r = cacheRegex(regex);
            return r.Replace(s, newstring);
        }

        /// <summary>
        /// istenilen tağı kaldıracak
        /// </summary>
        /// <param name="s">metin</param>
        /// <param name="tag">kaldıralacak etiket adı</param>
        /// <returns></returns>
        public static string Remove(this string s, string tag)
        {
            string reg = @"<[\/]{0,1}(" + tag + ")[^><]*>";
            Regex r = cacheRegex(reg);

            return r.Replace(s, string.Empty);
        }



        /// <summary>
        /// Use the current thread's culture info for conversion
        /// </summary>
        public static string ToTitleCase(this string str)
        {
            var cultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;
            return cultureInfo.TextInfo.ToTitleCase(str.ToLower());
        }

        /// <summary>
        /// Overload which uses the culture info with the specified name
        /// </summary>
        public static string ToTitleCase(this string str, string cultureInfoName)
        {
            var cultureInfo = new CultureInfo(cultureInfoName);
            return cultureInfo.TextInfo.ToTitleCase(str.ToLower());
        }

        /// <summary>
        /// Overload which uses the specified culture info
        /// </summary>
        public static string ToTitleCase(this string str, CultureInfo cultureInfo)
        {
            return cultureInfo.TextInfo.ToTitleCase(str.ToLower());
        }

        public static string ToMaskedString(this String value)
        {
            var pattern = "^(/d{2})(/d{3})(/d*)$";
            var regExp = new Regex(pattern);
            return regExp.Replace(value, "$1-$2-$3");
        }

        public static string MakeSEOString(this string input)
        {
            input = input.ToLower(new CultureInfo("tr-TR"));
            input = StripTags(input);
            input = RemoveAccents(input);
            input = Regex.Replace(input, "&.+?;", "");
            input = Regex.Replace(input, "[^.a-z0-9 _-]", "");
            input = Regex.Replace(input, @"\.|\s+", "-");
            input = Regex.Replace(input, "-+", "-");
            input = input.Trim('-');
            return input;
        }

        private static string RemoveAccents(string input)
        {
            string normalized = input.Replace('ı', 'i').Normalize(NormalizationForm.FormKD);
            char[] array = new char[input.Length];
            int arrayIndex = 0;
            foreach (char c in normalized)
            {
                if (char.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    array[arrayIndex] = c;
                    arrayIndex++;
                }
            }
            return new string(array, 0, arrayIndex);
        }

        private static string StripTags(string input)
        {
            char[] array = new char[input.Length];
            int arrayIndex = 0;
            bool inside = false;

            for (int i = 0; i < input.Length; i++)
            {
                char let = input[i];
                if (let == '<')
                {
                    inside = true;
                    continue;
                }
                if (let == '>')
                {
                    inside = false;
                    continue;
                }
                if (!inside)
                {
                    array[arrayIndex] = let;
                    arrayIndex++;
                }
            }
            return new string(array, 0, arrayIndex);
        }
    }
}