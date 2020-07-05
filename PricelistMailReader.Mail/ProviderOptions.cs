using System.Collections.Generic;

namespace PricelistMailReader.Mail
{
    public class ProviderOptions
    {
        public string Name { get; set; }
        public Dictionary<string, string> ColumnDefinitions { get; set; }
    }
}
