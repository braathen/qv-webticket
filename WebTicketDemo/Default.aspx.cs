using System;
using QvWebTicket;
using System.Net;
using System.Collections.Generic;
using System.IO;

namespace WebTicketDemo
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Define some configuration options for the QlikView Server
            TicketConfiguration config = new TicketConfiguration()
            {
                GetWebTicketUri = new Uri("http://localhost/QvAJAXZfc/GetWebTicket.aspx"),
                WindowsAuthentication = true,
            };

            // Create a ticket request with userid and groups (semicolon separated)
            TicketRequest request = new TicketRequest()
            {
                UserId = "rikard",
                Groups = new string[] { "PreSales", "Stockholm" }
            };
            
            // The response will contain the generated ticket, a redirect uri and possible error message
            TicketResponse response = new TicketResponse().GetTicket(request, config);

            if (response.ErrorMessage == null)
                Response.Redirect(response.RedirectUri, true);
            else
                Response.Write("Error: " + response.ErrorMessage);
        }
    }
}