using DecisionsFramework.Design.Flow;
using System.Linq;

namespace Decisions.USPS;

[AutoRegisterMethodsOnClass(true, "Integration", "USPS", "Deprecated")]
public static class USPSSteps
{
    public static string GetCityByZip(string zip5)
    {
        CityStateLookupResponse response = USPS.GetCityByZip(zip5);
        ZipCode zipCode = response.ZipCodes.FirstOrDefault();

        if (zipCode?.Error != null && zipCode.Error.Description != "Invalid Zip Code.")
            throw new USPSException(zipCode.Error);

        return zipCode?.City;
    }

    public static CityStateLookupResponse GetInformationByZip(string zip5)
    {
        CityStateLookupResponse response = USPS.GetCityByZip(zip5);
        return response;
    }
    
    public static string[] GetCitiesByZips(string[] zip5Codes)
    {
        CityStateLookupResponse response = USPS.GetCitiesByZips(zip5Codes);
        ZipCode[] zipCodes = response.ZipCodes.ToArray();

        ZipCode zipWithError = zipCodes.FirstOrDefault(code => code.Error != null);

        if (zipWithError != null)
            throw new USPSException(zipWithError.Error);

        string[] nameCities = zipCodes.Select(code => code.City).ToArray();
        return nameCities;
    }

    public static string GetZipByCity(string address1, string address2, string city, string state)
    {
        ZipCodeLookupRequest response = USPS.GetZipByCity(address1, address2, city, state);
        Address address = response.Addresses.FirstOrDefault();

        if (address?.Error != null)
            throw new USPSException(address.Error);

        return address?.Zip5;
    }

    
    public static Address NormalizeAddress(string address1, string address2, string city, string state)
    {
        AddressValidateResponse response = USPS.NormalizeAddress(address1, address2, city, state);
        Address address = response.Addresses.FirstOrDefault();

        if (address?.Error != null)
            throw new USPSException(address.Error);

        return address;
    }

    
    public static bool IsZipValidForCity(string zip, string city)
    {
        string cityForZip = GetCityByZip(zip);

        return !string.IsNullOrEmpty(cityForZip) && city.ToLower() == cityForZip.ToLower();
    }
}