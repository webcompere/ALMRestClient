using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ALMRestClient
{
    /// <summary>
    /// Represents the ALM system
    /// 
    /// From IP at https://MYDOMAIN.saas.hp.com/qcbin/Help/doc_library/api_refs/REST/webframe.html
    /// </summary>
    public class ALMClient
    {
        private RestClient client;

        public string Domain { get; set; }
        public String Project { get; set; }

        /// <summary>
        /// Construct with the url (just the https://something.saas.hp.com bit)
        /// </summary>
        /// <param name="url">Base url for ALM</param>
        /// <param name="username">username for login</param>
        /// <param name="password">password</param>
        public ALMClient(string url, string username, string password, string domain, string project) 
        {
            client = new RestClient(url+"/qcbin/");
            client.Authenticator = new HttpBasicAuthenticator(username, password);
            client.CookieContainer = new System.Net.CookieContainer();
            Domain = domain;
            Project = project;
        }

        /// <summary>
        /// Call this before calling other methods - will throw an ALMClientException on failure
        /// </summary>
        public void Login()
        {
            IRestResponse response = client.Execute(new RestRequest("authentication-point/authenticate"));

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new ALMClientException(response.StatusCode, "Error in login " + response.ErrorMessage, response.Content);
            }
        }

        public List<ALMItem> GetDefects()
        {
            List<ALMItem> items = new List<ALMItem>();
            
            ReadDefects(items);

            return items;
        }

        private void ReadDefects(List<ALMItem> items)
        {
            int startIndex = 0;
            int pageSize = 100;
            int total = 0;

            do
            {

                var getDefects = new RestRequest("rest/domains/{domain}/projects/{project}/defects");
                AddDomainAndProject(getDefects);
                getDefects.AddParameter("page-size", pageSize);

                // start index appears to be 1-based, despite the documentation on https://MYDOMAIN.saas.hp.com/qcbin/Help/doc_library/api_refs/REST/webframe.html
                getDefects.AddParameter("start-index", startIndex+1);

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

        private IRestResponse Execute(RestRequest request, string message)
        {
            IRestResponse response = client.Execute(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new ALMClientException(response.StatusCode, "Error in " + message + ": " + response.ErrorMessage, response.Content);
            }
            return response;
        }

        /// <summary>
        /// Update an ALM item
        /// </summary>
        /// <param name="id">Id of the item</param>
        /// <param name="changes">An item containing all the fields to change</param>
        public void UpdateDefect(string id, ALMItem changes)
        {
            var req = new RestRequest("/rest/domains/{domain}/projects/{project}/{Entity Type}/{Entity ID}/lock");
            AddDomainAndProject(req);
            AddDefectAndId(id, req);

            IRestResponse response = Execute(req, "update defect -> lock item");

            try
            {
                Update(id, changes);
            }
            finally
            {
                // then delete the lock
                req.Method = Method.DELETE;
                response = Execute(req, "update defect -> unlock item");
            }
        }

        private static void AddDefectAndId(string id, RestRequest req)
        {
            req.AddParameter("Entity Type", "defects", ParameterType.UrlSegment);
            req.AddParameter("Entity ID", id, ParameterType.UrlSegment);
        }

        private void AddDomainAndProject(RestRequest req)
        {
            req.AddParameter("domain", Domain, ParameterType.UrlSegment);
            req.AddParameter("project", Project, ParameterType.UrlSegment);
        }

        private void Update(string id, ALMItem changes)
        {
            var req = new RestRequest("/rest/domains/{domain}/projects/{project}/{Entity Type}/{Entity ID}");
            AddDomainAndProject(req);
            AddDefectAndId(id, req);

            req.Method = Method.PUT;

            req.AddHeader("Content-Type", "application/xml");
            req.AddHeader("Accept", "application/xml");
            req.AddParameter("application/xml", ConvertToFieldXml(changes.Fields), ParameterType.RequestBody);

            var response = Execute(req, "update");
        }

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
    }
}
