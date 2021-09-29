using System;

namespace Decisions.USPS
{
    public class Error
    {
        public string Number { get; set; }
        public string Source { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return Description;
        }
    }

    public class USPSException : Exception
    {
        public USPSException(Error error): base(error.ToString())
        {
        }
    }
}
