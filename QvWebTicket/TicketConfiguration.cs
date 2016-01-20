using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;

namespace QvWebTicket
{
    public class TicketConfiguration
    {
        private bool _WindowsAuthentication = false;
        public bool WindowsAuthentication
        {
            get { return _WindowsAuthentication; }
            set { _WindowsAuthentication = value; }
        }

        public NetworkCredential Credentials { get; set; }

        private Uri _GetWebTicketUri = new Uri("http://localhost/QvAJAXZfc/GetWebTicket.aspx");
        public Uri GetWebTicketUri
        {
            get { return _GetWebTicketUri; }
            set { _GetWebTicketUri = value; }
        }

        private Uri _AccessPointUri = null;
        public Uri AccessPointUri
        {
            get
            {
                if (_AccessPointUri == null)
                    return new UriBuilder(_GetWebTicketUri.Scheme, _GetWebTicketUri.Host).Uri;
                else
                    return _AccessPointUri;
            }
            set
            {
                _AccessPointUri = new UriBuilder(value.Scheme, value.Host).Uri;
            }
        }


        private Uri _TryUri = null;
        public Uri TryUri
        {
            get
            {
                if (_TryUri == null)
                {
                    Uri _x = _AccessPointUri != null ? _AccessPointUri : _GetWebTicketUri;
                    return new UriBuilder(_x.Scheme, _x.Host, _x.Port, "QlikView").Uri;
                }
                else
                {
                    return _TryUri;
                }
            }
            set { _TryUri = value; }
        }

        private string _BackUrl = "";
        public string BackUrl
        {
            get { return _BackUrl; }
            set { _BackUrl = value; }
        }

        private string _Document = "";
        public string Document {
            get { return _Document; }
            set
            {
                if (!value.ToLower().EndsWith(".qvw"))
                    _Document = value + ".qvw";
                else
                    _Document = value;
            }
        }
        public Dictionary<string, string> Select { get; set; }
        public string QvsHost { get; set; }
    }
}
