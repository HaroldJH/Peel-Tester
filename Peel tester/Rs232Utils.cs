using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peel_tester
{
    class Rs232Utils
    {
        public static string ByteArrayToHexString(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder(1024);
            foreach(byte b in bytes)
            {
                sb.AppendFormat("{0:x2}", b);
            }
            return sb.ToString().TrimEnd();
        }
    }
}
