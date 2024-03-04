using IdentityModel.OidcClient;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfWebView2;
using static IdentityModel.OidcConstants;

namespace WpfIAMDotNetFwkApp
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
      const string authority = @"http://localhost:9080/realms/Technidata";
      const string clientId = "tdnexlabs";
      const string audience = "account";
      const string nameClaimType = "name";
      const string roleClaimTemplate = "resource_access.tdnexlabs.roles";

      var options = new OidcClientOptions()
      {
        Authority = authority,
        ClientId = clientId,
        Scope = "openid profile email",
        RedirectUri = "https://tdnexlabs",
        Browser = new WpfEmbeddedBrowser(),
        Policy = new Policy
        {
          RequireIdentityTokenSignature = false
        }
      };

      var _oidcClient = new OidcClient(options);

      LoginResult loginResult;
      try
      {
        loginResult = await _oidcClient.LoginAsync();
        
        // Just to illustrate the differences
        string idTokenString = loginResult?.IdentityToken;        
        var idToken = new JwtSecurityToken(idTokenString);
        var idTokenClaims = idToken.Claims;

        // HERE THE ACCESS TOKEN!!!
        string accessTokenString = loginResult?.AccessToken;
        var accessToken = new JwtSecurityToken(accessTokenString);
        // Or new JwtSecurityTokenHandler().ReadJwtToken(accessTokenString);

        string metadataAddress = $"{authority}{(!authority.EndsWith("/", StringComparison.Ordinal) ? "/" : string.Empty)}.well-known/openid-configuration";

        // Warning-Https is disable here
        var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(metadataAddress, new OpenIdConnectConfigurationRetriever(), new HttpDocumentRetriever() { RequireHttps = false });
       
        var authorityConfiguration = await configurationManager.GetConfigurationAsync(CancellationToken.None);

        var validationParameters = new TokenValidationParameters();

        validationParameters.RoleClaimType = roleClaimTemplate;
        validationParameters.NameClaimType = nameClaimType;
        validationParameters.ValidAudience = audience;
        validationParameters.ValidateIssuer = true;
        validationParameters.ValidateAudience = false;
        validationParameters.IssuerSigningKeys = authorityConfiguration.SigningKeys;
        validationParameters.ValidIssuers = new[] { authorityConfiguration.Issuer };

        List<Exception> validationFailures = null;
        SecurityToken validatedToken = null;
        var securityTokenValidators = new List<ISecurityTokenValidator> { new JwtSecurityTokenHandler() };

        foreach (var validator in securityTokenValidators)
        {
          if (validator.CanReadToken(accessTokenString))
          {
            ClaimsPrincipal principal;
            try
            {
              principal = validator.ValidateToken(accessTokenString, validationParameters, out validatedToken);
            }
            catch (Exception ex)
            {              
              if (validationFailures == null)
              {
                validationFailures = new List<Exception>(1);
              }
              validationFailures.Add(ex);
              continue;
            }                       
          }
        }

        if (validationFailures != null)
        {
          txbMessage.Text = $"Unexpected Error: [{string.Join(",", validationFailures.Select(v => v.Message))}]";
          return;
        }
      }
      catch (Exception exception)
      {
        txbMessage.Text = $"Unexpected Error: {exception.Message}";
        return;
      }

      if (loginResult.IsError)
      {
        txbMessage.Text = loginResult.Error == "UserCancel" ? "The sign-in window was closed before authorization was completed." : loginResult.Error;
      }
      else
      {
        txbMessage.Text = $"Name: {loginResult.User.Identity.Name} - Claims: {string.Join(" \n ", loginResult.User.Claims.Select(c=> $"Type={c.Type}-Value={c.Value}"))}";
      }
    }
  }
}
