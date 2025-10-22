using System.Net.Http;
using Decisions.OAuth;
using Decisions.USPS.Clients;
using DecisionsFramework;
using DecisionsFramework.Design.Flow;
using DecisionsFramework.Design.Properties.Attributes;
using DecisionsFramework.Utilities.Data;
using DecisionsFramework.Data.ORMapper;

namespace Decisions.USPS;

[AutoRegisterMethodsOnClass(true, "Integration", "USPS")]
public static class UpdatedUspsSteps
{
    public static string GetCityByZipcode([TokenPicker] string tokenId, string zip5)
    {
        OAuthToken token = GetTokenById(tokenId);
        HttpClient httpClient = HttpClients.GetHttpClient(HttpClientAuthType.Normal);
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.TokenData);
        AddressesClient client = new AddressesClient(httpClient);
        
        try
        {
            CityAndState zipResponse = client.GetCityState(zip5);
            return $"{zipResponse?.City}, {zipResponse?.State}";
        }
        catch (Clients.ApiException ex)
        {
            // Handle authentication errors and other API exceptions more gracefully
            if (ex.StatusCode == 401)
            {
                throw new DecisionsFramework.BusinessRuleException(
                    "USPS API authentication failed. Please check that your OAuth token is valid and has the necessary permissions for city/state lookup.");
            }
            
            // For other API exceptions, provide the raw response which might contain useful error information
            throw new DecisionsFramework.BusinessRuleException(
                $"USPS API request failed with status {ex.StatusCode}. Response: {ex.Response}");
        }
    }
    
    public static Address GetZipcode([TokenPicker] string tokenId, string firm, string address1, string address2, string city, string state, string zip5, string zip4)
    {
        OAuthToken token = GetTokenById(tokenId);
        HttpClient httpClient = HttpClients.GetHttpClient(HttpClientAuthType.Normal);
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.TokenData);
        AddressesClient client = new AddressesClient(httpClient);
        
        try
        {
            ZIPCodeResponse zipResponse = client.GetZIPCode(firm, address1, address2, city, state, zip5, zip4);
            return new Address()
            {
                Address1 = zipResponse.Address.StreetAddress,
                Address2 = zipResponse.Address.SecondaryAddress,
                City = zipResponse.Address.City,
                State = zipResponse.Address.State,
                Zip5 = zipResponse.Address.ZIPCode,
                Zip4 = zipResponse.Address.ZIPPlus4,
            };
        }
        catch (Clients.ApiException ex)
        {
            // Handle authentication errors and other API exceptions more gracefully
            if (ex.StatusCode == 401)
            {
                throw new DecisionsFramework.BusinessRuleException(
                    "USPS API authentication failed. Please check that your OAuth token is valid and has the necessary permissions for ZIP code lookup.");
            }
            
            // For other API exceptions, provide the raw response which might contain useful error information
            throw new DecisionsFramework.BusinessRuleException(
                $"USPS API request failed with status {ex.StatusCode}. Response: {ex.Response}");
        }
    }

    public static Address GetAddress([TokenPicker] string tokenId, string firm, string address1, string address2, string city, string state, string urbanization, string zip5, string zip4)
    {
        OAuthToken token = GetTokenById(tokenId);
        HttpClient httpClient = HttpClients.GetHttpClient(HttpClientAuthType.Normal);
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.TokenData);
        AddressesClient client = new AddressesClient(httpClient);
        
        try
        {
            AddressResponse addressResponse = client.GetAddress(firm, address1, address2, city, state, urbanization, zip5, zip4);
            return new Address()
            {
                Address1 = addressResponse.Address.StreetAddress,
                Address2 = addressResponse.Address.SecondaryAddress,
                City = addressResponse.Address.City,
                State = addressResponse.Address.State,
                Zip5 = addressResponse.Address.ZIPCode,
                Zip4 = addressResponse.Address.ZIPPlus4,
            };
        }
        catch (Clients.ApiException ex)
        {
            // Handle authentication errors and other API exceptions more gracefully
            if (ex.StatusCode == 401)
            {
                throw new DecisionsFramework.BusinessRuleException(
                    "USPS API authentication failed. Please check that your OAuth token is valid and has the necessary permissions for address verification.");
            }
            
            // For other API exceptions, provide the raw response which might contain useful error information
            throw new DecisionsFramework.BusinessRuleException(
                $"USPS API request failed with status {ex.StatusCode}. Response: {ex.Response}");
        }
    }

    private static OAuthToken GetTokenById(string tokenId)
    {
        ORM<OAuthToken> orm = new ORM<OAuthToken>();
        OAuthToken token = orm.Fetch(tokenId);
        return token;
    }
}