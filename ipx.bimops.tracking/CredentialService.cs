using System;
using System.Net.Http.Headers;
using System.Resources;

namespace ipx.bimops.tracking;

public class CredentialService
{
    public static (string? clientId, string? clientSecret) GetResourceCredentials()
    {
        var resourceManager = new ResourceManager("ipx.bimops.tracking", typeof(CredentialService).Assembly);
        string? clientId = resourceManager.GetString("ClientId");
        string? clientSecret = resourceManager.GetString("ClientSecret");
        Console.WriteLine($"Client ID: {clientId}");
        Console.WriteLine($"Client Secret: {clientSecret}");

        if(clientId == "your-client-id" || clientSecret == "your-client-secret") 
            throw new Exception("Credentials are not set within the credentials file");

        return (clientId, clientSecret);
    }

    public static async Task<string> GetBearerToken(string clientId = "", string? clientSecret = "")
    {
        string accessToken = "";

        // OAuth token endpoint
        string url = "https://auth.idp.hashicorp.com/oauth2/token";
        

        // Prepare the request body
        var requestData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret),
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("audience", "https://api.hashicorp.cloud")
        });

        using (HttpClient client = new HttpClient())
        {
            try
            {
                // Send the POST request
                HttpResponseMessage response = await client.PostAsync(url, requestData);

                // Ensure the request was successful
                response.EnsureSuccessStatusCode();

                // Read the response content
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Response: " + responseBody);

                // Parse the access token from the response
                var json = System.Text.Json.JsonDocument.Parse(responseBody);
                accessToken = json.RootElement.GetProperty("access_token").GetString();

                Console.WriteLine("Access Token: " + accessToken);

                // Use this token in subsequent requests
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Request error: {e.Message}");
            }
        }
        
        return accessToken;
    }

    public static async Task GetCreds()
    {
        // Define the endpoint URL
        string url = "https://api.cloud.hashicorp.com/secrets/2023-11-28/organizations/eb6842ae-eaa1-4e0b-9292-5f5fca46cc79/projects/77157864-3a3b-4b00-8e01-31548b0b164b/apps/test/secrets:open";

        // get credentials from the internal resources
        var result = CredentialService.GetResourceCredentials();

        // Replace these with your actual client credentials
        string clientId = result.clientId;
        string clientSecret = result.clientSecret;

        // Replace this with your actual HCP API token retrieved from previous step
        string bearerToken = await CredentialService.GetBearerToken(clientId, clientSecret);

        // Create an HttpClient
        using (HttpClient client = new HttpClient())
        {
            // Set the Authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            try
            {
                // Make the GET request
                HttpResponseMessage response = await client.GetAsync(url);

                // Ensure the request was successful
                response.EnsureSuccessStatusCode();

                // Read and parse the response content
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Response:");
                Console.WriteLine(responseBody);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Request error: {e.Message}");
            }
        }
    }
}
