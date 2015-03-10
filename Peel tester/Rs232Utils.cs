using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;

namespace SerialProgram2
{
    public class Rs232Utils
    {
        private static char[] WhiteSpace = new char[] { ' ', '\n', '\r', '\t' };
        /// <summary>
        /// portname-baudrate-databit-parity-stopbit-flowcontrol 순
        /// COM1-9600-8-Even-1-None 과 같은 문자열이 맞는지 체크한다
        /// </summary>
        /// <param name="portdescstring"></param>
        /// <returns></returns>
        public static bool IsValidPortDescString(string portdescstring)
        {
            string[] tmp = portdescstring.Split('-');
            if (tmp.Length != 6) return false;

            if (System.Text.RegularExpressions.Regex.IsMatch(tmp[0], @"COM\d{1,2}") == false)
            {
                return false;
            }
            if (System.Text.RegularExpressions.Regex.IsMatch(tmp[1], @"\d{4,5}") == false)
            {
                return false;
            }
            if (System.Text.RegularExpressions.Regex.IsMatch(tmp[2], @"[4-8]") == false)
            {
                return false;
            }
            if (System.Text.RegularExpressions.Regex.IsMatch(tmp[3].ToUpper(), @"EVEN|ODD|NONE|MARK|SPACE") == false)
            {
                return false;
            }
            if (System.Text.RegularExpressions.Regex.IsMatch(tmp[4], @"[1|1.5|2]") == false)
            {
                return false;
            }
            if (System.Text.RegularExpressions.Regex.IsMatch(tmp[5].ToUpper(), @"XONXOFF|NONE|HARDWARE") == false)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// Port의 셋팅된 내용으로 PortDescString을 만들어서 리턴한다
        /// portname-baudrate-databit-parity-stopbit-flowcontrol 순
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static string PortDescString(System.IO.Ports.SerialPort port)
        {
            string stopbit = string.Empty;
            if (port.StopBits == System.IO.Ports.StopBits.None)
            {
                stopbit = "None";
            }
            else if (port.StopBits == System.IO.Ports.StopBits.One)
            {
                stopbit = "1";
            }
            else if (port.StopBits == System.IO.Ports.StopBits.OnePointFive)
            {
                stopbit = "1.5";
            }
            else if (port.StopBits == System.IO.Ports.StopBits.Two)
            {
                stopbit = "2";
            }
            string handshake = string.Empty;
            if (port.Handshake == System.IO.Ports.Handshake.None)
            {
                handshake = "None";
            }
            else if (port.Handshake == System.IO.Ports.Handshake.XOnXOff)
            {
                handshake = "XonXoff";
            }
            else
            {
                handshake = "Hardware";
            }
            return string.Format("{0}-{1}-{2}-{3}-{4}-{5}",
                port.PortName.ToString(),
                port.BaudRate,
                port.DataBits,
                port.Parity.ToString(),
                stopbit,
                handshake);
        }
        public static void SetPortDescString(SerialPort comport, string portdesc)
        {
            if (!IsValidPortDescString(portdesc))
            {
                throw new Exception(string.Format("{0} is not valid PortDescString", portdesc));
            }
            //셋팅
            //portname-baudrate-databit-parity-stopbit-flowcontrol 순
            string[] tmp = portdesc.Split('-');
            comport.PortName = tmp[0];
            comport.BaudRate = Convert.ToInt32(tmp[1]);
            comport.DataBits = Convert.ToInt32(tmp[2]);
            comport.Parity = (Parity)Enum.Parse(typeof(Parity), tmp[3]);
            comport.StopBits = (StopBits)Enum.Parse(typeof(StopBits), tmp[4]);


        }
        /// <summary>
        /// 00 02 03 30 31 과 같은 스트링인지 체크한다
        /// </summary>
        /// <param name="hexstring"></param>
        /// <returns></returns>
        public static bool IsValidHexString(string hexstring)
        {
            string[] tmp = hexstring.Split(WhiteSpace, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in tmp)
            {
                try
                {
                    if (s.Length != 2) return false;
                    Convert.ToByte(s, 16);
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }
        public static byte[] HexStringToByteArray(string hexstring)
        {
            string[] strarry = hexstring.Split(WhiteSpace, StringSplitOptions.RemoveEmptyEntries);
            List<byte> list = new List<byte>();
            try
            {
                foreach (string s in strarry)
                {
                    list.Add(byte.Parse(s, System.Globalization.NumberStyles.HexNumber));
                }
                return list.ToArray();
            }
            catch (Exception ex)
            {
                list = null;
                throw new Exception(string.Format("ByteUtils-HexStringToByteArray Error: {0}", ex.Message));
            }
        }
        public static string ByteToHex(byte b)
        {
            return Convert.ToString(b, 16).PadLeft(2, '0').ToUpper();
        }
        public static string ByteArrayToHexString(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder(1024);
            foreach (byte b in bytes)
            {
                sb.AppendFormat("{0} ", ByteToHex(b));
            }
            return sb.ToString().TrimEnd();
        }
        public static string ByteArrayToString(byte[] bytes)
        {
            return System.Text.Encoding.ASCII.GetString(bytes);
        }
    }
}
