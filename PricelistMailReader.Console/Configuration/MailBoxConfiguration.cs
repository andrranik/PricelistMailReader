using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PricelistMailReader.Console.Configuration
{
    public class MailBoxSection : ConfigurationSection
    {
        [ConfigurationProperty("MailBoxes")]
        public MailBoxesCollections MailBoxes
        {
            get
            {
                MailBoxesCollections mailBoxes = (MailBoxesCollections)base["MailBoxes"];
                return mailBoxes;
            }
        }
    }

    [ConfigurationCollection(typeof(MailBoxElement), AddItemName = "MailBox")]
    public class MailBoxesCollections : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new MailBoxElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((MailBoxElement)(element)).Name;
        }

        public MailBoxElement this[int idx] => (MailBoxElement)BaseGet(idx);
    }

    public class MailBoxElement : ConfigurationElement
    {
        [ConfigurationProperty("Name", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string Name => (string)base["Name"];

        [ConfigurationProperty("ServerUrl", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string ServerUrl => (string)base["ServerUrl"];

        [ConfigurationProperty("Port", DefaultValue = 0, IsKey = true, IsRequired = true)]
        public int Port => (int)base["Port"];
    }
}
