using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
namespace ALMRestClient.Configuration
{
	public class ALMConfigurationSetting : ConfigurationElement
	{
		[ConfigurationProperty("name", IsRequired = true)]
		public string Name
		{
			get
			{
				return (string)base["name"];
			}
			set
			{
				base["name"] = value;
			}
		}
		[ConfigurationProperty("value", IsRequired = true)]
		public string Value
		{
			get
			{
				return (string)base["value"];
			}
			set
			{
				base["value"] = value;
			}
		}

		[ConfigurationProperty("type", IsRequired = false, DefaultValue = ConfigType.Setting)]
		public ConfigType MyProperty
		{
			get
			{
				return (ConfigType)base["type"];
			}
			set
			{
				base["type"] = value;
			}
		}

		public enum ConfigType
		{
			Setting,
			Address

		}
	}
}
