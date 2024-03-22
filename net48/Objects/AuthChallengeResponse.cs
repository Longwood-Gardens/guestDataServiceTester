using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace guestDataServiceTester_net48.Objects
{
    public class AuthChallengeResponse
    {
        public string ChallengeID { get; set; } = string.Empty;
        public string Salt { get; set; } = string.Empty;
        public string EphemeralPublic { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
