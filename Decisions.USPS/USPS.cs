using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Xml.Serialization;
using DecisionsFramework;
using DecisionsFramework.ServiceLayer;

namespace Decisions.USPS
{
    public static class USPS
    {
        private static readonly HttpClient Client = new HttpClient();

        public static string Fetch(string api, string xmlQuery)
        {
            string urlEncodedQuery = HttpUtility.UrlEncode(xmlQuery);
            var responseString = Client.GetStringAsync($@"https://production.shippingapis.com/ShippingAPI.dll?API={api}&XML={urlEncodedQuery}").Result;

            return responseString;
        }

        /// <summary>
        /// The City/State Lookup Web Tool returns the city and state corresponding to the given ZIP Code. 
        /// </summary>
        /// <remark> This Web Tool processes up to five lookups per request.</returns>
        public static CityStateLookupResponse GetCitiesByZips(string[] zip5Codes)
        {
            var zipCodesQuery = zip5Codes.Select((code, id) => $@"<ZipCode ID=""{id}"">
                                                                    <Zip5>{code}</Zip5>
                                                                </ZipCode>").ToList();

            string userId = ModuleSettingsAccessor<USPSSettings>.GetSettings().UserId;
            
            var xmlRequest = $@"<CityStateLookupRequest USERID=""{userId}"">
                                            {string.Concat(zipCodesQuery)}
                                          </CityStateLookupRequest>";

            var responseString = Fetch("CityStateLookup", xmlRequest);

            try
            {
                var response = Deserialize<CityStateLookupResponse>(responseString);
                return response;  
            }
            catch (Exception ex)
            {
                throw new BusinessRuleException(
                    "There was an issue performing the request to the USPS web service. " +
                    "Please ensure that you have set the User ID in Administration -> Settings - USPS Settings. " +
                    $" {responseString}", ex);
            }
        }


        public static CityStateLookupResponse GetCityByZip(string zip5)
        {
            var response = GetCitiesByZips(new[] { zip5 });

            return response;
        }


        public static ZipCodeLookupRequest GetZipByCity(string address1, string address2, string city, string state)
        {
            string userId = ModuleSettingsAccessor<USPSSettings>.GetSettings().UserId;
            var responseString = Fetch("ZipCodeLookup", $@"<ZipCodeLookupRequest USERID=""{userId}"">
                                                <Address ID = ""0"" >
                                                    <Address1>{address1}</Address1>
                                                    <Address2>{address2}</Address2>
                                                    <City>{city}</City>
                                                    <State>{state}</State>
                                                </Address>
                                            </ZipCodeLookupRequest>");

            try
            {
                var address = Deserialize<ZipCodeLookupRequest>(responseString);
                return address;
            }
            catch (Exception ex)
            {
                throw new BusinessRuleException(
                    "There was an issue performing the request to the USPS web service. " +
                    "Please ensure that you have set the User ID in Administration -> Settings - USPS Settings. " +
                    $" {responseString}", ex);
            }
        }

        public static AddressValidateResponse NormalizeAddress(string address1, string address2, string city, string state)
        {
            string userId = ModuleSettingsAccessor<USPSSettings>.GetSettings().UserId;
            var responseString = Fetch("Verify", $@"<AddressValidateRequest USERID=""{userId}"">
                                                <Address ID=""0"">
                                                    <Address1>{address1}</Address1>
                                                    <Address2>{address2}</Address2>
                                                    <City>{city}</City>
                                                    <State>{state}</State>
                                                    <Zip5></Zip5>
                                                    <Zip4></Zip4>
                                                </Address>
                                            </AddressValidateRequest>");
            try
            {
                var address = Deserialize<AddressValidateResponse>(responseString);
                return address;
            }
            catch (Exception ex)
            {
                throw new BusinessRuleException(
                    "There was an issue performing the request to the USPS web service. " +
                    "Please ensure that you have set the User ID in Administration -> Settings - USPS Settings. " +
                    $" {responseString}", ex);
            }
        }

        private static T Deserialize<T>(string strObj) where T : class
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (TextReader reader = new StringReader(strObj))
            {
                try
                {
                    var response = serializer.Deserialize(reader) as T;
                    return response;
                }
                catch (Exception ex)
                {
                    throw new BusinessRuleException(
                        "There was an issue performing the request to the USPS web service. " +
                        "Please ensure that you have set the User ID in Administration -> Settings - USPS Settings", ex);
                }
            }
        }
    }
}
