using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace guestDataServiceTester_net48.Objects
{
    public class FoodBeverageDiscountResponse
    {
        public string Discount { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal GuestNumber { get; set; }
        public decimal MembershipNumber { get; set; }
        public string MembershipDescription { get; set; } = string.Empty;
    }
}
