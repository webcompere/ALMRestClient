using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace ALMRestClient.Configuration
{

	[ConfigurationCollection(typeof(ALMConfigurationSetting), AddItemName = "setting")]
	public class ALMConfiguration : ConfigurationElementCollection
	{
		[ConfigurationProperty("majorVersion")]
		public int MajorVersion
		{
			get
			{
				return (int)base["majorVersion"];
			}
			set
			{
				base["majorVersion"] = value;
			}
		}

		[ConfigurationProperty("minorVersion")]
		public int? MinorVersion
		{
			get
			{
				return (int?)base["minorVersion"];
			}
			set
			{
				base["minorVersion"] = value;
			}
		}

		public string Version
		{
			get
			{
				int majorVersion = MajorVersion;
				int minorVersion;

				if (MinorVersion.HasValue)
				{
					minorVersion = MinorVersion.Value;
				}
				else
				{
					minorVersion = 0;
				}

				return string.Format("{0}.{1}", majorVersion, minorVersion);
			}
		}


		internal const string PropertyName = "setting";

		public override ConfigurationElementCollectionType CollectionType
		{
			get
			{
				return ConfigurationElementCollectionType.BasicMap;
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
			return new ALMConfigurationSetting();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((ALMConfigurationSetting)element).Name;
		}

		public override bool IsReadOnly()
		{
			return false;
		}

	}
}
