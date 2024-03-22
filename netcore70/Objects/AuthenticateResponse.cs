using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace guestDataServiceTester.Objects
{
    public class AuthenticateResponse
    {
        public string Message { get; set; } = string.Empty;
        public string Proof { get; set; } = string.Empty;
        public string EBearer { get; set; } = string.Empty;
    }
}
