using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using RestSharp.Authenticators;

namespace ALMRestClient
{
	public partial class ALMClient
	{

		private string loginToken = string.Empty;
		private string sessionToken = string.Empty;

		/// <summary>
		/// Call this before calling other methods - will throw an ALMClientException on failure
		/// </summary>
		public bool Login()
		{
			IRestRequest request = new RestRequest(clientConfig.LoginAddress);

			CleanRequest(ref request);

			IRestResponse response = client.Execute(request);

			if (response.StatusCode != HttpStatusCode.OK)
			{
				ThrowExceptionIfNecessary(response, "Login");
			}
			
			SetTokenFromCookies(client);

			return response.StatusCode == HttpStatusCode.OK;
		}
		
		/// <summary>
		/// Checks whether the user is currently authenticated
		/// </summary>
		public bool IsAuthenticated()
		{
			RestSharp.IRestRequest isAuthenticated = new RestSharp.RestRequest(clientConfig.IsAuthenticatedAddress);

			CleanRequest(ref isAuthenticated);

			RestSharp.IRestResponse response = client.Execute(isAuthenticated);

			if (response.StatusCode != HttpStatusCode.OK)
			{
				ThrowExceptionIfNecessary(response, "IsAuthenticated");
			}

			return response.StatusCode == HttpStatusCode.OK;
		}

		/// <summary>
		/// Logs the user out of the session
		/// </summary>
		public bool Logout()
		{
			if (clientConfig.IsLogoutRequired == false)
			{
				return true;
			}

			if (clientConfig.IsSessionRequired == true)
			{
				// Clear the session
				ManageServerSessions(() => RestSharp.Method.DELETE);
			}

			IRestRequest request = new RestRequest(clientConfig.LogoutAddress);

			CleanRequest(ref request);
			IRestResponse response = client.Execute(request);

			if (response.StatusCode != HttpStatusCode.OK)
			{
				ThrowExceptionIfNecessary(response, "Logout");
			}

			return response.StatusCode == HttpStatusCode.OK;
		}

		/// <summary>
		/// Manages the session credentials
		/// </summary>
		/// <param name="getMethod">Need to know what type of action should occur</param>
		private bool ManageServerSessions(Func<RestSharp.Method> getMethod)
		{
			if (clientConfig.IsSessionRequired == false)
			{
				return true;
			}
			
			IRestRequest updateSession = new RestSharp.RestRequest(clientConfig.SessionAddress);

			if (getMethod != null)
			{
				updateSession.Method = getMethod();
			}
			else
			{
				throw new InvalidOperationException("Unable to manage server sessions without knowing which method to use");
			}
			
			AddAuthTokensAsNecessary(updateSession);
			CleanRequest(ref updateSession);
			
			RestSharp.IRestResponse response = client.Execute(updateSession);

			SetTokenFromCookies(client);

			return response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK;
		}

		/// <summary>
		/// Adds the authentication tokens to the request if the parameters don't include them
		/// </summary>
		/// <param name="request">The request object to be checked</param>
		private RestSharp.IRestRequest AddAuthTokensAsNecessary(RestSharp.IRestRequest request)
		{
			if (string.IsNullOrEmpty(loginToken) == false && request.Parameters.Count(x => string.Equals(x.Name, clientConfig.TokenCookieName) == true) == 0)
			{
				request.AddParameter(clientConfig.TokenCookieName, loginToken);
			}

			if (string.IsNullOrEmpty(sessionToken) == false && request.Parameters.Count(x => string.Equals(x.Name, clientConfig.SessionCookieName) == true) == 0)
			{
				request.AddParameter(clientConfig.SessionCookieName, sessionToken);
			}

			return request;
		}

		/// <summary>
		/// Logs in and updates the server sessions which is required before executing queries.
		/// </summary>
		private bool VerifyLoggedInAuthenticatedAndExtendSession()
		{
			bool result = IsAuthenticated() == true || Login() == true;

			if (result = true && clientConfig.IsSessionRequired == true)
			{
				result = ManageServerSessions(GetExistingSessionMethod);
			}

			return result;
		}

		/// <summary>
		/// Sets the internal tokens for reuse
		/// </summary>
		/// <param name="clientToRead">The client containing the cookies to be reused</param>
		private void SetTokenFromCookies(IRestClient clientToRead)
		{
			FindCookie(clientToRead, clientConfig.TokenCookieName, (cookie) => loginToken = cookie.Value);
			FindCookie(clientToRead, clientConfig.SessionCookieName, (cookie) => sessionToken = cookie.Value);
		}

		/// <summary>
		/// Enumerates the client's cookie container looking for the cookie requested and calls the action when the cookie is found
		/// </summary>
		/// <param name="client">The client to be processed against</param>
		/// <param name="cookieName">The name of the cookie to locate</param>
		/// <param name="foundCookie">The action to occur if the cookie is found</param>
		/// <returns>Whether or not the cookie is found</returns>
		private static bool FindCookie(RestSharp.IRestClient client, string cookieName, Action<Cookie> foundCookie)
		{
			bool result = false;

			if (client != null && client.CookieContainer != null || client.CookieContainer.GetCookies(client.BaseUrl).Count >= 0)
			{
				foreach (Cookie cookie in client.CookieContainer.GetCookies(client.BaseUrl))
				{
					if (string.Equals(cookie.Name, cookieName) == true)
					{
						if (foundCookie != null)
						{
							foundCookie(cookie);
						}

						result = true;
						break;
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Determines the current session state and returns the correct action to continue working the same session
		/// </summary>
		private RestSharp.Method GetExistingSessionMethod()
		{
			bool reuseSession = false;

			if (FindCookie(client, clientConfig.SessionCookieName, (cookie) =>
			{
				reuseSession = cookie.TimeStamp.AddMinutes(60) > DateTime.Now;
			}) == false)
			{
				reuseSession = false;
			}

			if (reuseSession == true)
			{
				return RestSharp.Method.GET;
			}
			else
			{
				return RestSharp.Method.POST;
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			if (client != null)
			{
				// Some versions require log off to be called before termininating the sessionToken
				Logout();
			}
		}

		#endregion
	}
}
