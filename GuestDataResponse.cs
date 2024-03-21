using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace guestDataServiceAuthTester
{
    public class GuestDataResponse<T>
    {
        public bool Success { get; set; } = true;
        public string Status { get; set; } = string.Empty;
        public T? Data { get; set; } = default;
    }
}
