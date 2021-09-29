using System.Collections.Generic;
using System.Xml.Serialization;

namespace Decisions.USPS
{
    [XmlRoot("AddressValidateResponse")]
    public class AddressValidateResponse
    {
        [XmlElement("Address")]
        public List<Address> Addresses { get; set; }
    }


    [XmlRoot("CityStateLookupResponse")]
    public class CityStateLookupResponse
    {
        [XmlElement("ZipCode")]
        public List<ZipCode> ZipCodes { get; set; }
    }

    [XmlRoot("ZipCodeLookupResponse")]
    public class ZipCodeLookupRequest
    {
        [XmlElement("Address")]
        public List<Address> Addresses { get; set; }
    }
}
