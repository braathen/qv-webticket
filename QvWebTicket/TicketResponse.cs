using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;
using System.Net;

namespace QvWebTicket
{
    public partial class TicketResponse
    {
        public string Ticket { get; set; }
        public string RedirectUri { get; set; }
        public string ErrorMessage { get; set; }

        public TicketResponse GetTicket(TicketRequest request, TicketConfiguration config)
        {
            if (String.IsNullOrEmpty(request.UserId))
            {
                ErrorMessage = "No userid defined";
                return this;
            }

            string groups = request.Groups != null && request.Groups.Length > 0 ? GetGroups(request.Groups) : "";

            string webTicketXml = string.Format("<Global method=\"GetWebTicket\"><UserId>{0}</UserId>{1}</Global>", request.UserId, groups);

            var ticket = Execute(config, webTicketXml);

            if (ticket != null && ticket.Contains("Invalid call"))
                ErrorMessage = ticket;

            if (!String.IsNullOrEmpty(ErrorMessage))
                return this;

            Ticket = ticket;

            if (String.IsNullOrEmpty(config.Document))
                RedirectUri = string.Format("{0}QvAJAXZfc/Authenticate.aspx?type=html&webticket={1}&try={2}&back={3}", config.AccessPointUri, Ticket, Uri.EscapeUriString(config.TryUri.ToString()), Uri.EscapeUriString(config.BackUrl.ToString()));
            else
            {
                if (String.IsNullOrEmpty(config.QvsHost))
                {
                    ErrorMessage = "QvsHost parameter is required";
                    return this;
                }

                var selections = "";
                if (config.Select != null && config.Select.Count > 0)
                    selections = GetSelections(config.Select);

                RedirectUri = string.Format("{0}QvAJAXZfc/Authenticate.aspx?type=html&webticket={1}&try={2}&back={3}", config.AccessPointUri, Ticket, Uri.EscapeDataString(config.AccessPointUri + "QvAJAXZfc/AccessPoint.aspx?open=&id=" + config.QvsHost + "%7C" + config.Document) + selections + "&client=Ajax", config.BackUrl);
            }

            return this;
        }

        private string Execute(TicketConfiguration config, string data = "")
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(config.GetWebTicketUri);
                if (config.WindowsAuthentication)
                {
                    if (config.Credentials != null)
                        request.Credentials = config.Credentials;
                    else
                        request.UseDefaultCredentials = true;

                    request.PreAuthenticate = true;
                }
                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.Method = "POST";
                request.Timeout = 50000;
                request.ContentType = "application/x-www-form-urlencoded";
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                var buffer = Encoding.UTF8.GetBytes(data);
                request.ContentLength = buffer.Length;
                var dataStream = request.GetRequestStream();
                dataStream.Write(buffer, 0, buffer.Length);
                dataStream.Close();

                var response = (HttpWebResponse)request.GetResponse();
                var responseStream = response.GetResponseStream();
                var reader = new StreamReader(responseStream, Encoding.UTF8);
                var result = reader.ReadToEnd();

                reader.Close();
                dataStream.Close();
                response.Close();

                XDocument doc = XDocument.Parse(result);
                return doc.Root.Element("_retval_").Value;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return null;
            }
        }

        private string GetGroups(string[] groups)
        {
            var groupsXml = new StringBuilder();

            if (groups.Length > 0)
            {
                groupsXml.Append("<GroupList>");
                groupsXml.Append(String.Join("", groups.Select(x => "<string> " + x + "</string>")));
                groupsXml.Append("</GroupList><GroupsIsNames>true</GroupsIsNames>");
            }

            return groupsXml.ToString();
        }

        private string GetSelections(Dictionary<string, string> selections)
        {
            var selectionCollection = new StringBuilder();

            foreach (KeyValuePair<string, string> pair in selections)
            {
                selectionCollection.Append("&select=");
                selectionCollection.Append(pair.Key + "%2C" + pair.Value.Replace(",","%2C"));
            }
            return selectionCollection.ToString();
        }
    }
}
