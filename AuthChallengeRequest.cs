using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace guestDataServiceAuthTester
{
    public class AuthChallengeRequest
    {
        public string Id { get; set; }
        public string EphemeralPublic { get; set; }
    }
}
