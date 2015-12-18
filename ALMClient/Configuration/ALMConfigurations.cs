using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ALMRestClient.Configuration
{
	[ConfigurationCollection(typeof(ALMConfiguration))]
	public class ALMConfigurations : ConfigurationElementCollection
	{
		internal const string PropertyName = "ALMConfiguration";
		
		public override ConfigurationElementCollectionType CollectionType
		{
			get
			{
				return ConfigurationElementCollectionType.BasicMapAlternate;
			}
		}
		protected override string ElementName
		{
			get
			{
				return PropertyName;
			}
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new ALMConfiguration();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			ALMConfiguration almElement = (ALMConfiguration)element;
			return string.Format("{0}{1}", almElement.MajorVersion, almElement.MinorVersion);
		}

		public override bool IsReadOnly()
		{
			return false;
		}
	}
}
