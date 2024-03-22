using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace guestDataServiceTester_net48.Objects
{
    public class AuthenticateRequest
    {
        public string ChallengeId { get; set; } = string.Empty;
        public string Proof { get; set; } = string.Empty;
    }
}
