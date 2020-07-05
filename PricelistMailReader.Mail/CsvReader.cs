using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using PricelistMailReader.DataBase;

namespace PricelistMailReader.Mail
{
    public static class CsvReader
    {
        private const char separator = ';';
        private static char _diapasone = '-';
        private static char _greater = '>';
        private static char _less = '<';
        public static List<PriceItem> GetPriceItemsFromCsv(StreamReader stream, Dictionary<string, string> columnsDefinitions, IProgress<ProgressOptions> progress)
        {
            string line;
            var isHeader = true;
            var headerIdxs = new Dictionary<string, int>();
            var priceItems = new List<PriceItem>();

            int columnCount = 0;
            while ((line = stream.ReadLine()) != null)
            {
                string[] strArray = null;

                if (isHeader)
                {
                    strArray = line.Split(separator);
                    columnCount = strArray.Length;
                    headerIdxs.Add("Vendor", Array.IndexOf(strArray, columnsDefinitions["Vendor"]));
                    headerIdxs.Add("Number", Array.IndexOf(strArray, columnsDefinitions["Number"]));
                    headerIdxs.Add("Description", Array.IndexOf(strArray, columnsDefinitions["Description"]));
                    headerIdxs.Add("Price", Array.IndexOf(strArray, columnsDefinitions["Price"]));
                    headerIdxs.Add("Count", Array.IndexOf(strArray, columnsDefinitions["Count"]));
                    isHeader = false;
                }
                else
                {
                    if (headerIdxs.Count != 0)
                    {
                        var crr = new CsvRowReader(line, columnCount);
                        strArray = crr.ToStringArray();
                        var priceItem = new PriceItem();
                        priceItem.Vendor = strArray[headerIdxs["Vendor"]];
                        priceItem.Number = strArray[headerIdxs["Number"]];
                        priceItem.Description = strArray[headerIdxs["Description"]].Length > 255 ? strArray[headerIdxs["Description"]].Substring(0, 255) : strArray[headerIdxs["Description"]];
                        priceItem.Price = decimal.Parse(strArray[headerIdxs["Price"]],
                            CultureInfo.GetCultureInfo("Ru-ru"));
                        priceItem.Count = GetCount(strArray[headerIdxs["Count"]]);
                        priceItem.SearchVendor = CleanInput(priceItem.Vendor).ToUpper();
                        priceItem.SearchNumber = CleanInput(priceItem.Number).ToUpper();
                        priceItems.Add(priceItem);
                    }
                }
            }

            return priceItems;
            
        }

        static string CleanInput(string strIn)
        {
            try
            {
                return Regex.Replace(strIn, @"[^\w]", "",
                    RegexOptions.None, TimeSpan.FromSeconds(1.5));
            }
            catch (RegexMatchTimeoutException)
            {
                return String.Empty;
            }
        }

        static int GetCount(string strIn)
        {
            if (strIn.Contains(_diapasone)) 
                return int.Parse(strIn.Substring(strIn.IndexOf(_diapasone) + 1, strIn.Length - strIn.IndexOf(_diapasone) - 1).Trim());
            if (strIn.Contains(_greater))
                return int.Parse(strIn.Substring(strIn.IndexOf(_greater) + 1, strIn.Length - strIn.IndexOf(_greater) - 1).Trim());
            if (strIn.Contains(_less))
                return int.Parse(strIn.Substring(strIn.IndexOf(_less) + 1, strIn.Length - strIn.IndexOf(_less) - 1).Trim());
            return int.Parse(strIn);
        }

    }

    public class CsvRowReader
    {
        private const char Separator = ';';
        private string _str;
        private int _curSeparatorIdx;
        private int _restOfStringLength;
        private int _columnCount;
        private int CurSeparatorIdx
        {
            get => _curSeparatorIdx;
            set => _curSeparatorIdx = value;
        }
        private int RestOfStringLenght
        {
            get =>  CurSeparatorIdx == 0 ? _str.Length : _str.Length - CurSeparatorIdx - 1;
            set => _restOfStringLength = value;
        }
        private int FirstCharAfterSeparatorIdx => _curSeparatorIdx == 0 ? 0 : _curSeparatorIdx + 1;
        private int FirstCharAfterSeparatorExceptQuoteIdx => FirstCharAfterSeparatorIdx + 1;

        public CsvRowReader(string row, int columnCount)
        {
            _str = row;
            _restOfStringLength = _str.Length;
            _columnCount = columnCount;
        }

        private string GetNext()
        {
            bool firstElement = CurSeparatorIdx == 0;
            bool lastElement = NextSeparatorIdx == -1;
            var substr = _str.Substring(CurSeparatorIdx, !lastElement ? NextSeparatorIdx - CurSeparatorIdx + 1 : _str.Length - CurSeparatorIdx - 1);
            if (substr.Contains(";\"") && !substr.Contains("\";"))
            {
                var endQuoteIdx = GetEndQuoteIndex();
                substr = _str.Substring(FirstCharAfterSeparatorIdx + 1, endQuoteIdx - 2 - CurSeparatorIdx);
                CurSeparatorIdx = endQuoteIdx + 1;
                return substr;
            }

            CurSeparatorIdx = NextSeparatorIdx;
            return firstElement ? substr.Remove(substr.Length - 1, 1) : lastElement ? substr.Remove(0, 1) : substr.Remove(0, 1).Remove(substr.Length - 2, 1);
        }

        private int NextSeparatorIdx
        {
            get => _str.IndexOf(Separator, FirstCharAfterSeparatorIdx, RestOfStringLenght);
        }

        public string[] ToStringArray()
        {
            var result = new string[_columnCount];
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = GetNext();
            }

            return result;
        }

        private int GetEndQuoteIndex()
        {
            return _str.IndexOf("\";", FirstCharAfterSeparatorExceptQuoteIdx, RestOfStringLenght - 1);
        }
    }
}
