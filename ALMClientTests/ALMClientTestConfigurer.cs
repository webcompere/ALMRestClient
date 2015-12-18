using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ALMClientTests
{
	public class ALMClientTestConfigure
	{
		public bool SetUpSettings(string userName, string password, string ALMAddress)
		{

			dynamic newPassword;

			if (global::ALMClientTests.Properties.Settings.Default.ALMPassword is System.Security.SecureString)
			{

				newPassword = new System.Security.SecureString();

				foreach (char c in password)
				{
					((System.Security.SecureString)newPassword).AppendChar(c);
				}
			}
			else
			{
				newPassword = password;
			}

			global::ALMClientTests.Properties.Settings.Default.ALMPassword = newPassword;

			global::ALMClientTests.Properties.Settings.Default.ALMAddress = ALMAddress;
			
			global::ALMClientTests.Properties.Settings.Default.ALMUserName = userName;

			global::ALMClientTests.Properties.Settings.Default.Save();

			return true;

		}
	}

}
