using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Decisions.USPS
{
    public class ZipCode
    {
        public string Zip5 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public Error Error { get; set; }
    }
}
