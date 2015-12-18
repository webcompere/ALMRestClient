using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace ALMRestClient.Configuration
{
	public class ALMConfigurationSection : ConfigurationSection
	{
		[ConfigurationCollection(typeof(ALMConfigurations))]
		[ConfigurationProperty("ALMConfigurations", IsRequired = true)]
		public ALMConfigurations Configurations
		{
			get
			{
				return base["ALMConfigurations"] as ALMConfigurations;
			}
			set
			{
				base["ALMConfigurations"] = value;
			}
		}
	}
}
