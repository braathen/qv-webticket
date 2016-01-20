### About

QvWebTicket is an ASP.NET module for simpifying integration with QlikView to other products, platforms or portals using so called "Web Tickets".

### Installation

Easiest way to install is by using the NuGet Package Management Console inside of Visual Studio.

```sh
PM> Install-Package QvWebTicket
```

### Getting Started

The code example here is more or less what is needed to retrieve a webticket and a redirection url where to forward the user to. Some additional parameters and features are available in other sections below.

```c#
protected void Page_Load(object sender, EventArgs e)
{
    // Define some configuration options for the QlikView Server
    TicketConfiguration config = new TicketConfiguration()
    {
        GetWebTicketUri = new Uri("http://qlikview.domain.com/QvAJAXZfc/GetWebTicket.aspx"),
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
```

### Options

**TicketRequest()** - User information for requesting a webticket:

* TicketRequest.**UserId** (string)
  The user identity of the user that should have a webticket. It can be in any form, prefixed with a domain, a plain username or an email address for example.
* TicketRequest.**Groups** (string[])
  An array of groups the user is a member of, this can be used both for authorization in AccessPoint and in Section Access.

**TicketResponse()** - These properties are read only and contains information about the ticket request:

* TicketResponse.**Ticket**

  The ticket itself, which there normally is not any need of, it's recommended to use the ``RedirectUri`` instead.

* TicketResponse.**RedirectUri**

  This property is set on successfull ticket request and contains the full Uri that the user should be redirected to, including the webticket.

* TicketResponse.**ErrorMessage**

  In case something went wrong the error message will be in here.

**TicketConfiguration()** - Basic configuration options:

* TicketConfiguration.**GetWebTicketUri** (uri)

  The full path to the GetWebTicket.aspx page on the QlikView web server that will be delivering the webtickets. Default is ``http://localhost/QvAJAXZfc/GetWebTicket.aspx``.

* TicketConfiguration.**AccessPointUri** (uri)

  The servername or address to where the QlikView AccessPoint is located. This is only necessary to specify if it is different than the servername used in _TicketConfiguration.GetWebTicket_ above. Typically ``http://qlikview.domain.com``

* TicketConfiguration.**TryUri** (uri)

  Should normally not be neccessary to specify, it will default to AccessPoint unless a specific document is set, see _TicketConfiguration.Document_ below.

* TicketConfiguration.**BackUri** (uri)

  In case of authentication failing for some reason, this is where to redirect the user.

These options are for using Windows Authentication as trust. Default is IP whitelists:

* TicketConfiguration.**WindowsAuthentication** (true|false)
  
  Set this to ``true`` to use Windows Authentication as trust mechanism for the code or process to retrieve a webticket.

* TicketConfiguration.**Credentials** NetworkCredential(string userName, string Password)

  Use this to specify the Windows Authentication credentials to be used together with _TicketConfiguration.WindowsAuthentication_. If no credentials are specified ``UseDefaultCredentials`` will be used to allow the calling process to be used instead. This can for example be the application pool in IIS.

The options below are for redirecting the user to an application instead of the AccessPoint portal:

* TicketConfiguration.**Document** (string)

  By specifying this the TryUrl will be ignored and instead the user will be redirected directly into the specified application.
  > Note: This options requires the _TicketConfiguration.QvsHost_ to also be set.

* TicketConfiguration.**QvsHost** (string)

  This option is only used together with _TicketConfiguration.Document_ and tells QlikView which QVS host to use. This is the name specified for the QlikView Server in QMC, typically in the form of "QVS@server".

* TicketConfiguration.**Select** Dictionary<string, string>

  To select initial values inside of the application something like can be used. There is currently a limitation of making selections if one object, but multiple values may be selected.

  ```c#
  TicketConfiguration config = new TicketConfiguration()
  {
      Select = new Dictionary<string, string> {{ "LB39", "Banana,Lime" }}
  };
  ```

### QlikView Configuration

First of all, while not mandatory it can be easier to use IIS for the QlikView Web Server when working with ticketing. It may be required to host your login page anyway.
* QlikView needs to be an Enterprise Edition Licence.
* QlikView needs to be running in DMS mode for security (see manual).
* The QlikView web site in IIS needs to be set up to use Anonymous permissions – it will be expecting windows permissions by default – specifically it is the QVAJAXZFC directory that needs it's permission changing.
* QlikView needs to trust the code asking for the ticket. There is a web page within the QlikView web server called GetWebTicket.aspx which handles requests for tickets, this will only return a ticket to a trusted user/process and this is identified using one of two options:

* **Option 1 – use windows permissions**
  * The code or process asking for a ticket needs to run as or provide a windows user identity. The user ID must be a member of the QlikView Administrators windows group on the QlikView server.
  * As the GetWebTicket page runs under the QVAJAXZFC directory on the web server and one of the above steps made the directory work with Anonymous users –for this page only– enable windows authentication in IIS
  * Now the Login page must authenticate itself when requesting a ticket as a windows user and that user must be a QlikView Administrator. In the attached example the login is hardcoded, however this could be configured in other ways

* **Option 2 – use an IP address white list**
  * For some code technologies NTLM is not available or it may just not be appropriate to use it. For these scenarios a “white list” of approved IP addresses can be used rather than a named user. Using this approach the GetWebTicket page will only return a ticket to code running from a specific IP address
  * To configure this option:
    * Open the web server config file from C:\ProgramData\QlikTech\WebServer\config.xml
    * Locate the line &lt;GetWebTicket url="/QvAjaxZfc/GetWebTicket.aspx" /&gt;
    * Replace it with the following specifying the IP address(s) of the web server(s) running the code
    ```xml
    <GetWebTicket url="/QvAjaxZfc/GetWebTicket.aspx">
        <TrustedIP>127.0.0.1</TrustedIP>
    </GetWebTicket>
    ```

It's also strongly recommended to prohibit anonymous users in QlikView Server and last but not least set a custom login page for Authentication in the QlikView Web Server configuration and then you set this page which retrieves the webticket as login page of course. This will redirect the user to get a ticket when they're trying to access QlikView.

### SafeForwardList

In order to prevent man-in-the-middle attacks when using WebTickets it's recommended to use a SafeForwardList like the below example.

```xml
<Authentication>
  ...
  <SafeForwardList>
    <TrustedHost>10.76.224.35</TrustedHost>
    <TrustedHost>qlikview.domain.com</TrustedHost>
  </SafeForwardList>
  ...
</Authentication>
```

### Troubleshooting

If the request for a webticket fails it more or less always has to do with trust issues, that you are not allowed to request a webticket. Common problems when using white lists is that the IP address for TrustedIP is not what you think it is. For example with IPv6 enabled on the server localhost is not 127.0.0.1 anymore but ::1. Try to ping localhost to see what it resolves into.

### License

This software is made available "AS IS" without warranty of any kind under The Mit License (MIT). QlikTech support agreement does not cover support for this software.

### Meta

* Code: `git clone git://github.com/braathen/qv-webticket.git`
* Home: <https://github.com/braathen/qv-webticket>
* Bugs: <https://github.com/braathen/qv-webticket/issues>
