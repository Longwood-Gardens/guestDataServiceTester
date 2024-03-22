using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace guestDataServiceTester_net48
{
    public static class StringExtensions
    {
        public static string ToHexString(this byte[] bytes)
        {
            StringBuilder hex = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public static int HexNybbleToInt(this char c)
        {
            if (c >= '0' && c <= '9')
            {
                return c - '0';
            }
            c = (char)(c & ~0x20);
            if (c >= 'A' && c <= 'F')
            {
                return c - ('A' - 10);
            }
            throw new ArgumentException("Invalid nybble: " + c);
        }
        public static byte HexNybbleToByte(this char c)
        {
            if (c >= '0' && c <= '9')
            {
                return (byte)(c - '0');
            }
            c = (char)(c & ~0x20);
            if (c >= 'A' && c <= 'F')
            {
                return (byte)(c - ('A' - 10));
            }
            throw new ArgumentException("Invalid nybble: " + c);
        }

        public static byte[] HexToBytes(this string hexString)
        {
            if ((hexString.Length & 1) != 0)
            {
                throw new ArgumentException("Input must have even number of characters");
            }
            int length = hexString.Length / 2;
            byte[] ret = new byte[length];
            for (int i = 0, j = 0; i < length; i++)
            {
                int high = hexString[j++].HexNybbleToInt();
                int low = hexString[j++].HexNybbleToInt();
                ret[i] = (byte)((high << 4) | low);
            }

            return ret;
        }
    }
}
