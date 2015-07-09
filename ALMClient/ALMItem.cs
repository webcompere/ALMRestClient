using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ALMRestClient
{
    public class ALMItem
    {
        /// <summary>
        /// Construct the item from the XML payload - factory method
        /// </summary>
        /// <returns>new ALM Item</returns>
        public static ALMItem FromXML(IEnumerable<XElement> fields)
        {
            ALMItem item = new ALMItem();

            foreach (var field in fields)
            {
                var name = field.Attribute("Name").Value;
                var valueElement = field.Element("Value");

                if (valueElement == null)
                {
                    // skip ones with no values
                    continue;  
                }
                
                var value = valueElement.Value;

                item.Fields.Add(name, value);
            }

            return item;
        }

        public ALMItem()
        {
            Fields = new Dictionary<string, string>();
        }

        /// <summary>
        /// The fields of the item
        /// </summary>
        public Dictionary<string, string> Fields { get; private set; }

        /// <summary>
        /// Status
        /// </summary>
        public string Status 
        { 
            get 
            { 
                return Fields["status"]; 
            }
            set
            {
                Fields["status"] = value;
            }
        }

        public string Id
        {
            get
            {
                return Fields["id"];
            }
            set
            {
                Fields["id"] = value;
            }
        }

        public string Name
        {
            get
            {
                return Fields["name"];
            }
            set
            {
                Fields["name"] = value;
            }
        }

        public string Description
        {
            get
            {
                return Fields["description"];
            }
            set
            {
                Fields["description"] = value;
            }
        }

    }
}
