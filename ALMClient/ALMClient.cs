using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using RestSharp.Authenticators;
using System.Net;
using System.Net.Http;

namespace ALMRestClient
{
	/// <summary>
	/// Represents the ALM system
	/// 
	/// From IP at https://MYDOMAIN.saas.hp.com/qcbin/Help/doc_library/api_refs/REST/webframe.html
	/// </summary>
	public partial class ALMClient : IDisposable
	{
		private RestClient client;
		private ALMClientConfig clientConfig;

		/// <summary>
		/// Get or set a value indicating the exceptions thrown by the client should be hidden
		/// </summary>
		public bool HideCustomExceptions
		{
			get;
			set;
		}

		/// <summary>
		/// Get or set the domain
		/// </summary>
		public string Domain
		{
			get;
			set;
		}

		/// <summary>
		/// Get or set the project
		/// </summary>
		public string Project
		{
			get;
			set;
		}

		/// <summary>
		/// Construct with the url (just the https://something.saas.hp.com bit)
		/// </summary>
		/// <param name="url">Base url for ALM</param>
		/// <param name="username">username for login</param>
		/// <param name="password">password</param>
		/// <param name="domain">The domain to log into</param>
		/// <param name="project">The project to log into</param>
		/// <param name="version">The ALM version to use</param>
		public ALMClient(string url, string username, string password, string domain, string project, string version)
		{
			client = new RestClient((url + "/qcbin/").Replace(@"/qcbin//qcbin/", @"/qcbin/"));
			client.Authenticator = new HttpBasicAuthenticator(username, password);
			client.CookieContainer = new System.Net.CookieContainer();
			Domain = domain;
			Project = project;
			clientConfig = new ALMClientConfig(version);
		}



		/// <summary>
		/// Construct with the url (just the https://something.saas.hp.com bit)
		/// </summary>
		/// <param name="url">Base url for ALM</param>
		/// <param name="username">username for login</param>
		/// <param name="password">password</param>
		/// <param name="domain">The domain to log into</param>
		/// <param name="project">The project to log into</param>
		public ALMClient(string url, string username, string password, string domain, string project)
			: this(url, username, password, domain, project, String.Empty)
		{
		}


		/// <summary>
		/// Get the list of defects
		/// </summary>
		public List<ALMItem> GetDefects()
		{
			List<ALMItem> items = new List<ALMItem>();

			ReadDefects(items);

			return items;
		}

		/// <summary>
		/// Get the list of defects
		/// </summary>
		/// <param name="items">Defects result</param>
		private void ReadDefects(List<ALMItem> items)
		{
			int startIndex = 0;
			int pageSize = 100;
			int total = 0;

			do
			{

				RestRequest getDefects = new RestRequest(clientConfig.EntitiesAddress);


				AddDomainAndProject(getDefects);
				AddDefect(getDefects);
				getDefects.AddHeader("Accept", "application/xml");

				getDefects.AddParameter("page-size", pageSize);

				// start index appears to be 1-based, despite the documentation on https://MYDOMAIN.saas.hp.com/qcbin/Help/doc_library/api_refs/REST/webframe.html
				getDefects.AddParameter("start-index", startIndex + 1);

				IRestResponse response = Execute(getDefects, "get defects");

				XDocument doc = XDocument.Parse(response.Content);
				total = FindTotal(doc);

				foreach (var entity in doc.Root.Elements())
				{
					items.Add(ALMItem.FromXML(entity.Elements("Fields").Elements()));
				}

				// time for the next page
				startIndex += pageSize;

			} while (startIndex < total);
		}

		/// <summary>
		/// Find the total count
		/// </summary>
		/// <param name="doc">The xml document from HP ALM containing the elements</param>
		private static int FindTotal(XDocument doc)
		{
			var attribute = doc.Root.Attribute("TotalResults");
			if (attribute != null)
			{
				try
				{
					return int.Parse(attribute.Value);
				}
				catch (Exception e)
				{
					// swallow the exception - not much can be done
				}
			}
			return 0;
		}

		/// <summary>
		/// Executes a query against HP ALM
		/// </summary>
		/// <param name="request">The request to send</param>
		/// <param name="message">The message used in the case of errors</param>
		/// <remarks>The session is updated before each query and doesn't need to be managed elsewhere (except for Login/Logout)</remarks>
		private IRestResponse Execute(IRestRequest request, string message)
		{
			if (VerifyLoggedInAuthenticatedAndExtendSession() == false)
			{
				if (HideCustomExceptions == false)
				{
					throw new ALMClientException(HttpStatusCode.Unauthorized, "Cannot execute without authenticated user", string.Empty);
				}
				else
				{
					return null;
				}
			}

			CleanRequest(ref request);

			IRestResponse response = client.Execute(request);

			if (response.StatusCode != HttpStatusCode.OK)
			{
				ThrowExceptionIfNecessary(response, "Execute", message);
			}

			return response;
		}

		/// <summary>
		/// Update an ALM item
		/// </summary>
		/// <param name="id">Id of the item</param>
		/// <param name="changes">An item containing all the fields to change</param>
		public bool UpdateDefect(string id, ALMItem changes)
		{
			var req = new RestRequest(clientConfig.LockEntityAddress);
			AddDomainAndProject(req);
			AddDefectAndId(id, req);

			IRestResponse response = null;

			try
			{
				response = Execute(req, "update defect -> lock item");

				if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
				{
					if (Update(id, changes) == false && HideCustomExceptions == false)
					{
						// Todo: Determine what exception should be thrown
					}
				}
			}
			finally
			{
				// then delete the lock
				req.Method = Method.DELETE;
				response = Execute(req, "update defect -> unlock item");
			}

			// Delete lock was successful
			return response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.NoContent;
		}

		/// <summary>
		/// Add the defect segment
		/// </summary>
		/// <param name="req">The request to be processed</param>
		private static void AddDefect(IRestRequest req)
		{
			req.AddParameter("Entity Type", "defect", ParameterType.UrlSegment);
		}

		/// <summary>
		/// Add the defect and defect Id
		/// </summary>
		/// <param name="id">The defect Id to be requested</param>
		/// <param name="req">The request to be processed</param>
		private static void AddDefectAndId(string id, RestRequest req)
		{
			AddDefect(req);
			req.AddParameter("Entity ID", id, ParameterType.UrlSegment);
		}

		/// <summary>
		/// Add the domain and project to the request
		/// </summary>
		/// <param name="req">The request to be processed</param>
		private void AddDomainAndProject(RestRequest req)
		{
			req.AddParameter("domain", Domain, ParameterType.UrlSegment);
			req.AddParameter("project", Project, ParameterType.UrlSegment);
		}

		/// <summary>
		/// Update the item
		/// </summary>
		/// <param name="id">The item id to be updated</param>
		/// <param name="changes">The modified item to be updated</param>
		private bool Update(string id, ALMItem changes)
		{
			var req = new RestRequest(clientConfig.EntityAddress);
			AddDomainAndProject(req);
			AddDefectAndId(id, req);

			req.Method = Method.PUT;

			req.AddHeader("Content-Type", "application/xml");
			req.AddHeader("Accept", "application/xml");
			req.AddParameter("application/xml", ConvertToFieldXml(changes.Fields), ParameterType.RequestBody);

			var response = Execute(req, "update");

			return response.StatusCode == HttpStatusCode.OK;
		}

		/// <summary>
		/// Converts the dictionary into an xml string
		/// </summary>
		/// <param name="dictionary">The dictionary to be converted</param>
		private string ConvertToFieldXml(Dictionary<string, string> dictionary)
		{
			StringBuilder builder = new StringBuilder();

			builder.Append("<Entity Type=\"defects\">");
			builder.Append("<Fields>");

			foreach (var key in dictionary.Keys)
			{
				string value = dictionary[key];

				builder.Append("<Field Name=\"" + key + "\">");
				builder.Append("<Value>");
				builder.Append(value);
				builder.Append("</Value>");
				builder.Append("</Field>");
			}

			builder.Append("</Fields>");
			builder.Append("</Entity>");

			return builder.ToString();
		}

		/// <summary>
		/// Cleans the request before processing in an attempt to minimize errors
		/// </summary>
		/// <param name="request">The request to be processed</param>
		private void CleanRequest(ref IRestRequest request)
		{
			if (client != null && client.BaseUrl != null && string.IsNullOrEmpty(client.BaseUrl.AbsolutePath) == false)
			{
				if (string.IsNullOrEmpty(request.Resource) == false && client.BaseUrl.AbsolutePath.Contains(@"/qcbin/") == true)
				{
					request.Resource = request.Resource.Replace(@"/qcbin/", string.Empty);
				}
			}
		}

		/// <summary>
		/// Throws an exception with a standard message when HideCustomExceptions is false
		/// </summary>
		/// <param name="response">The response that caused the error</param>
		/// <param name="location">The location of the error</param>
		private void ThrowExceptionIfNecessary(IRestResponse response, string location)
		{
			if (HideCustomExceptions == false)
			{
				throw new ALMClientException(response.StatusCode, string.Format("Error in {0} {1}", location, response.ErrorMessage), response.Content);
			}
		}


		/// <summary>
		/// Throws an exception with a standard message when HideCustomExceptions is false
		/// </summary>
		/// <param name="response">The response that caused the error</param>
		/// <param name="location">The location of the error</param>
		/// <param name="customMessage">A custom message descripting the error</param>
		private void ThrowExceptionIfNecessary(IRestResponse response, string location, string customMessage)
		{
			if (HideCustomExceptions == false)
			{
				throw new ALMClientException(response.StatusCode, string.Format("Error in {0}:{1} {2}", location, customMessage, response.ErrorMessage), response.Content);
			}
		}
	}
}
