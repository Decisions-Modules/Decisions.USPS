using System.Net.Http;
using Decisions.OAuth;
using Decisions.USPS.Clients;
using DecisionsFramework.Design.Flow;
using DecisionsFramework.Design.Properties.Attributes;
using DecisionsFramework.Utilities.Data;

namespace Decisions.USPS;

[AutoRegisterMethodsOnClass(true, "Integration", "USPS")]
public static class UpdatedUspsSteps
{
    public static string GetCityByZipcode([TokenPicker] OAuthToken token, string zip5)
    {
        HttpClient httpClient = HttpClients.GetHttpClient(HttpClientAuthType.Normal);
        AddressesClient client = new AddressesClient(httpClient);
        CityAndState zipResponse = client.GetCityState(zip5);
        return $"{zipResponse?.City}, {zipResponse?.State}";
    }
    
    public static Address GetZipcode([TokenPicker] OAuthToken token, string firm, string address1, string address2, string city, string state, string zip5, string zip4)
    {
        HttpClient httpClient = HttpClients.GetHttpClient(HttpClientAuthType.Normal);
        AddressesClient client = new AddressesClient(httpClient);
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

    public static Address GetAddress([TokenPicker] OAuthToken token, string firm, string address1, string address2, string city, string state, string urbanization, string zip5, string zip4)
    {
        HttpClient httpClient = HttpClients.GetHttpClient(HttpClientAuthType.Normal);
        AddressesClient client = new AddressesClient(httpClient);
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
}