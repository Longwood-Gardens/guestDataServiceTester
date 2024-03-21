using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace guestDataServiceAuthTester
{
    public class AuthChallengeResponse
    {
        public string ChallengeID { get; set; } = string.Empty;
        public string Salt { get; set; } = string.Empty;
        public string EphemeralPublic { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
