using DecisionsFramework.Design.Flow;
using System.IO;
using System.Xml.Serialization;
using System.Linq;
using System.Net.Http;

namespace Decisions.USPS
{

    [AutoRegisterMethodsOnClass(true, "Integration", "USPS")]
    public static class USPSSteps
    {
        public static string GetCityByZip(string zip5)
        {
            var response = USPS.GetCityByZip(zip5);
            var zipCode = response.ZipCodes.First();

            if (zipCode.Error != null && zipCode.Error.Description != "Invalid Zip Code.")
                throw new USPSException(zipCode.Error);

            return zipCode.City;
        }

        public static CityStateLookupResponse GetInformationByZip(string zip5)
        {
            CityStateLookupResponse response = USPS.GetCityByZip(zip5);

            return response;
        }
        
        public static string[] GetCitiesByZips(string[] zip5Codes)
        {
            var response = USPS.GetCitiesByZips(zip5Codes);
            var zipCodes = response.ZipCodes.ToArray();

            var zipWithError = zipCodes.FirstOrDefault(code => code.Error != null);

            if (zipWithError != null)
                throw new USPSException(zipWithError.Error);

            var nameCities = zipCodes.Select(code => code.City).ToArray();
            return nameCities;
        }

        public static string GetZipByCity(string address1, string address2, string city, string state)
        {
            var response = USPS.GetZipByCity(address1, address2, city, state);
            var address = response.Addresses.First();

            if (address.Error != null)
                throw new USPSException(address.Error);

            return address.Zip5;
        }

        public static Address NormalizeAddress(string address1, string address2, string city, string state)
        {
            var response = USPS.NormalizeAddress(address1, address2, city, state);
            var address = response.Addresses.First();

            if (address.Error != null)
                throw new USPSException(address.Error);

            return address;
        }

        public static bool IsZipValidForCity(string zip, string city)
        {
            var cityForZip = GetCityByZip(zip);

            return !string.IsNullOrEmpty(cityForZip) && city.ToLower() == cityForZip.ToLower();
        }
    }
}
