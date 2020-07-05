using System.Collections.Generic;
using System.Configuration;
using PricelistMailReader.DataBase;

namespace PricelistMailReader.Console.Configuration
{
    public class ProvidersSection : ConfigurationSection
    {
        [ConfigurationProperty("Providers")]
        public ProvidersCollections Providers
        {
            get
            {
                ProvidersCollections column = (ProvidersCollections) base["Providers"];
                return column;
            }
        }
    }

    [ConfigurationCollection(typeof(ProviderElement), AddItemName = "Provider")]
    public class ProvidersCollections : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ProviderElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ProviderElement) (element)).Name;
        }

        public ProviderElement this[int idx] => (ProviderElement) BaseGet(idx);
    }

    public class ProviderElement : ConfigurationElement
    {
        [ConfigurationProperty("Name", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string Name => (string) base["Name"];

        [ConfigurationProperty("VendorCaption", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string VendorCaption => (string) base["VendorCaption"];

        [ConfigurationProperty("DescriptionCaption", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string DescriptionCaption => (string) base["DescriptionCaption"];

        [ConfigurationProperty("PriceCaption", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string PriceCaption => (string) base["PriceCaption"];

        [ConfigurationProperty("CountCaption", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string CountCaption => (string) base["CountCaption"];

        [ConfigurationProperty("NumberCaption", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string NumberCaption => (string) base["NumberCaption"];

        public Dictionary<string, string> ToDictionary()
        {
            return new Dictionary<string, string>
            {
                { PriceItemFieldsNames.Vendor, VendorCaption },
                { PriceItemFieldsNames.Description, DescriptionCaption },
                { PriceItemFieldsNames.Number, NumberCaption },
                { PriceItemFieldsNames.Price, PriceCaption },
                { PriceItemFieldsNames.Count, CountCaption },
            };
        }
    }
}
