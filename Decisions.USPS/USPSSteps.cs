using System.Collections.Generic;
using DecisionsFramework.Design.Flow;
using System.Linq;
using System.Net.Http;
using Decisions.OAuth;
using Decisions.USPS.Clients;
using DecisionsFramework.Data.ORMapper;
using DecisionsFramework.ServiceLayer;
using DecisionsFramework.Utilities.Data;

namespace Decisions.USPS;

[AutoRegisterMethodsOnClass(true, "Integration", "USPS")]
public static class USPSSteps
{
    public static string GetCityByZip(string zip5)
    {
        OAuthToken? token = GetTokenFromSettings();

        // Handle deprecated API if token is not set
        if (token == null)
        {
            CityStateLookupResponse response = USPS.GetCityByZip(zip5);
            ZipCode zipCode = response.ZipCodes.FirstOrDefault();

            if (zipCode?.Error != null && zipCode.Error.Description != "Invalid Zip Code.")
                throw new USPSException(zipCode.Error);

            return zipCode?.City;
        }
        
        // Handle updated api
        HttpClient httpClient = HttpClients.GetHttpClient(HttpClientAuthType.Normal);
        AddressesClient client = new AddressesClient(httpClient);
        var cityAndState = client.GetCityState(zip5);

        return $"{cityAndState.City}, {cityAndState.State}";
    }

    public static CityStateLookupResponse GetInformationByZip(string zip5)
    {
        OAuthToken? token = GetTokenFromSettings();
        
        // Handle deprecated API if token is not set
        if (token == null)
        {
            CityStateLookupResponse response = USPS.GetCityByZip(zip5);
            return response;
        }
        
        // Handle updated api
        HttpClient httpClient = HttpClients.GetHttpClient(HttpClientAuthType.Normal);
        AddressesClient client = new AddressesClient(httpClient);
        CityAndState lookupResponse = client.GetCityState(zip5);
        return new CityStateLookupResponse()
        {
            ZipCodes =
            [
                new ZipCode()
                {
                    Zip5 = lookupResponse.ZIPCode,
                    City = lookupResponse.City,
                    State = lookupResponse.State
                }
            ]
        };
    }
    
    public static string[] GetCitiesByZips(string[] zip5Codes)
    {
        OAuthToken? token = GetTokenFromSettings();
        
        // Handle deprecated API if token is not set
        if (token == null)
        {
            CityStateLookupResponse response = USPS.GetCitiesByZips(zip5Codes);
            ZipCode[] zipCodes = response.ZipCodes.ToArray();

            ZipCode zipWithError = zipCodes.FirstOrDefault(code => code.Error != null);

            if (zipWithError != null)
                throw new USPSException(zipWithError.Error);

            string[] nameCities = zipCodes.Select(code => code.City).ToArray();
            return nameCities;
        }
        
        // Handle updated API
        HttpClient httpClient = HttpClients.GetHttpClient(HttpClientAuthType.Normal);
        AddressesClient client = new AddressesClient(httpClient);

        List<string> cities = [];
        foreach (var zip5 in zip5Codes)
        {
            var lookupResponse = client.GetCityState(zip5);
            cities.Add($"{lookupResponse.City}, {lookupResponse.State}");
        }
        return cities.ToArray();
    }

    public static string GetZipByCity(string address1, string address2, string city, string state)
    {
        OAuthToken? token = GetTokenFromSettings();
        
        // Handle deprecated API if token is not set
        if (token == null)
        {
            ZipCodeLookupRequest response = USPS.GetZipByCity(address1, address2, city, state);
            Address address = response.Addresses.FirstOrDefault();

            if (address?.Error != null)
                throw new USPSException(address.Error);

            return address?.Zip5;
        }
        
        // Handle updated API
        HttpClient httpClient = HttpClients.GetHttpClient(HttpClientAuthType.Normal);
        AddressesClient client = new AddressesClient(httpClient);
        var zipResponse = client.GetZIPCode(string.Empty, address1, address2, city, state, string.Empty, string.Empty);
        return zipResponse.Address.ZIPCode;
    }

    /// <summary>
    /// New step to allow full access to GetZipCode on the new API
    /// </summary>
    public static Address GetZipcode(string firm, string address1, string address2, string city, string state, string zip5, string zip4)
    {
        OAuthToken? token = GetTokenFromSettings();

        // Handle updated API
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
    
    public static Address NormalizeAddress(string address1, string address2, string city, string state)
    {
        OAuthToken? token = GetTokenFromSettings();
        
        // Handle deprecated API if token is not set
        if (token == null)
        {
            AddressValidateResponse response = USPS.NormalizeAddress(address1, address2, city, state);
            Address address = response.Addresses.FirstOrDefault();

            if (address?.Error != null)
                throw new USPSException(address.Error);

            return address;
        }
        
        // Handle updated API
        HttpClient httpClient = HttpClients.GetHttpClient(HttpClientAuthType.Normal);
        AddressesClient client = new AddressesClient(httpClient);
        AddressResponse addressResponse = client.GetAddress(string.Empty, address1, address2, city, state, string.Empty, string.Empty, string.Empty);
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

    /// <summary>
    /// New step to allow full access to GetZipCode on the new API
    /// </summary>
    public static Address GetAddress(string firm, string address1, string address2, string city, string state, string urbanization, string zip5, string zip4)
    {
        OAuthToken? token = GetTokenFromSettings();

        // Handle updated API
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
    
    public static bool IsZipValidForCity(string zip, string city)
    {
        OAuthToken? token = GetTokenFromSettings();
        
        // Handle deprecated API if token is not set
        if (token == null)
        {
            var cityForZip = GetCityByZip(zip);

            return !string.IsNullOrEmpty(cityForZip) && city.ToLower() == cityForZip.ToLower();
        }
        
        // Handle updated API
        HttpClient httpClient = HttpClients.GetHttpClient(HttpClientAuthType.Normal);
        AddressesClient client = new AddressesClient(httpClient);
        string[] cities = GetCitiesByZips([zip]);
        return cities.Any(c => c.ToLower().Contains(city.ToLower()));
    }

    /// <summary>
    /// Helper method for loading configured OAuth token from the USPS settings.
    /// </summary>
    private static OAuthToken? GetTokenFromSettings()
    {
        USPSSettings settings = ModuleSettingsAccessor<USPSSettings>.GetSettings();
        if (string.IsNullOrEmpty(settings.OAuthToken))
            return null;
        
        ORM<OAuthToken> orm = new ORM<OAuthToken>();
        return orm.Fetch(settings.OAuthToken);
    }
}