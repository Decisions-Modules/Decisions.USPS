namespace Decisions.USPS;  // Backwards compatible namespace

public class ZipCode
{
    public string Zip5 { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public Error Error { get; set; }
}