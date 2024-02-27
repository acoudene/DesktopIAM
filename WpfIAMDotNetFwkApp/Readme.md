1. Create Realm => "Technidata" => URL = http://localhost:9080/realms/Technidata
2. Inside Realm Technidata, create clientId => "tdnexlabs"
3. Inside clientId "tdnexlabs", Add "Valid Redirect Uris" = "https://tdnexlabs/*" and Add "Web origins" = "https://tdnexlabs" (without ending slash)
4. Be careful about this line in this code to avoid https checking: 
var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(metadataAddress, new OpenIdConnectConfigurationRetriever(), new HttpDocumentRetriever() { RequireHttps = false });
5. Create a user in Keyclaok to experiment.