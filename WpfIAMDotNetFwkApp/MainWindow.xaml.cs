using IdentityModel.OidcClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
      var options = new OidcClientOptions()
      {
        Authority = "https://tdkeycloak.azurewebsites.net/auth/realms/Technidata",
        ClientId = "tdnexlabs",
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
        txbMessage.Text = loginResult.User.Identity.Name;
      }
    }
  }
}
