using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ALMRestClient
{
	public class ALMClientConfig : IDisposable
	{
		// Setting keys
		private const string IsSessionRequiredKey = "IsSessionRequired";
		private const string IsLogoutRequiredKey = "IsLogoutRequired";
		private const string SessionCookieNameKey = "SessionCookieName";
		private const string AuthenticatedTokenNameKey = "TokenCookieName";

		// Address keys
		private const string LoginAddressKey = "Login";
		private const string LogoutAddressKey = "Logout";
		private const string EntityLockKey = "LockEntity";
		private const string EntityCollectionAddressKey = "EntityCollection";
		private const string EntityAddressKey = "Entity";
		private const string IsAuthenticatedAddressKey = "IsAuthenticated";
		private const string SessionAddresskey = "SessionAddress";
				
		private Dictionary<string, Dictionary<string, string>> versionSettings;

		/// <summary>
		/// The version to be used
		/// </summary>
		public string Version
		{
			get;
			private set;
		}

		/// <summary>
		/// Contains the settings to be consumed by the ALMClient for access HP ALM
		/// </summary>
		public ALMClientConfig()
		{
			LoadSettingsFromConfig();

			if (string.IsNullOrEmpty(Version))
			{
				Version = GetHighestVersionFromConfig();
			}
		}
		/// <summary>
		/// Contains the settings to be consumed by the ALMClient for access HP ALM
		/// </summary>
		/// <param name="version">The version to be used</param>
		public ALMClientConfig(string version)
			: this()
		{
			if (string.IsNullOrEmpty(version) == false)
			{
				Version = version;
			}
		}

		/// <summary>
		/// Loads the settings from the config file
		/// </summary>
		private void LoadSettingsFromConfig()
		{
			versionSettings = new Dictionary<string, Dictionary<string, string>>(StringComparer.CurrentCultureIgnoreCase);

			Configuration.ALMConfigurationSection configSection = (Configuration.ALMConfigurationSection)System.Configuration.ConfigurationManager.GetSection("ALMConfigurationSection"); //as Configuration.ALMConfigurationSection;

			foreach (Configuration.ALMConfiguration config in configSection.Configurations)
			{
				versionSettings.Add(config.Version, new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase));
				foreach (Configuration.ALMConfigurationSetting setting in config)
				{
					versionSettings[config.Version].Add(setting.Name, setting.Value);
				}
			}
		}

		/// <summary>
		/// Gets the highest version from the config file
		/// </summary>
		/// <returns></returns>
		private string GetHighestVersionFromConfig()
		{
			return this.versionSettings.OrderByDescending((x) => x.Key).First().Key;
		}

		/// <summary>
		/// Contains a list of the current versions settings
		/// </summary>
		public Dictionary<string, string> CurrentSettings
		{
			get
			{
				return versionSettings[Version];
			}
		}

		#region URLs

		/// <summary>
		/// Address for logging into HP ALM
		/// </summary>
		public string LoginAddress
		{
			get
			{
				return CurrentSettings[LoginAddressKey];
			}
		}

		/// <summary>
		/// Contains the address for verifying authentication status
		/// </summary>
		public string IsAuthenticatedAddress
		{
			get
			{
				return CurrentSettings[IsAuthenticatedAddressKey];
			}
		}

		/// <summary>
		/// Get the address for managing server session states
		/// </summary>
		public string SessionAddress
		{
			get
			{
				return CurrentSettings[SessionAddresskey];
			}
		}

		/// <summary>
		/// Get the address for logging a user out
		/// </summary>
		public string LogoutAddress
		{
			get
			{
				return CurrentSettings[LogoutAddressKey];
			}
		}

		/// <summary>
		/// Get the address of locking entities
		/// </summary>
		public string LockEntityAddress
		{
			get
			{
				return CurrentSettings[EntityLockKey];
			}
		}

		/// <summary>
		/// Get the value for access an entity collection
		/// </summary>
		public string EntitiesAddress
		{
			get
			{
				return CurrentSettings[EntityCollectionAddressKey];
			}
		}

		/// <summary>
		/// Get the value for accessing a specific entity
		/// </summary>
		public string EntityAddress
		{
			get
			{
				return CurrentSettings[EntityAddressKey];
			}
		}
		
		#endregion
		#region OtherSettings

		/// <summary>
		/// Indicates whether this version requires a session for retrieving data
		/// </summary>
		public bool IsSessionRequired
		{
			get
			{
				return Convert.ToBoolean(CurrentSettings[IsSessionRequiredKey]);
			}
		}

		/// <summary>
		/// Get a value indicating whether or not this version requires users to be logged out
		/// </summary>
		public bool IsLogoutRequired
		{
			get
			{
				return Convert.ToBoolean(CurrentSettings[IsLogoutRequiredKey]);
			}
		}

		/// <summary>
		/// Get the session cookie name used for this version
		/// </summary>
		public string SessionCookieName
		{
			get
			{
				return CurrentSettings[SessionCookieNameKey];
			}
		}

		/// <summary>
		/// Get the authentication token name used for this version
		/// </summary>
		public string TokenCookieName
		{
			get
			{
				return CurrentSettings[AuthenticatedTokenNameKey];
			}
		}
		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			if (versionSettings != null)
			{
				foreach (KeyValuePair<string,Dictionary<string, string>> child in versionSettings)
				{
					child.Value.Clear();
				}

				versionSettings.Clear();
			}
		}

		#endregion
	}
}
