using System;
using System.Text;

namespace Reefoo.CMPP30
{
    /// <summary>
    /// 编码帮助类。
    /// </summary>
    internal static class Convert
    {
        #region 公有方法
        /// <summary>
        /// 字节流解码。
        /// </summary>
        public static string ToString(byte[] buffer, int startIndex, int length, CmppEncoding encoding)
        {
            switch (encoding)
            {
                case CmppEncoding.GBK:
                    return Encoding.GetEncoding("gb2312").GetString(buffer, startIndex, length);
                case CmppEncoding.ASCII:
                    return Encoding.ASCII.GetString(buffer, startIndex, length);
                case CmppEncoding.UCS2:
                    return Encoding.BigEndianUnicode.GetString(buffer, startIndex, length);
                default:
                    return "";
            }
        }
        /// <summary>
        /// 字节流编码。
        /// </summary>
        public static byte[] ToBytes(string value, byte coding)
        {
            if (string.IsNullOrEmpty(value)) return null;
            switch (coding)
            {
                case CmppConstants.Encoding.GBK:
                    return Encoding.GetEncoding("gb2312").GetBytes(value);
                case CmppConstants.Encoding.ASCII:
                case CmppConstants.Encoding.Binary:
                    return Encoding.ASCII.GetBytes(value);
                case CmppConstants.Encoding.UCS2:
                case CmppConstants.Encoding.Special:
                    return Encoding.BigEndianUnicode.GetBytes(value);
                default:
                    return null;
            }
        }

        public static byte[] ToBytes(string value, byte encoding, int byteLen)
        {
            Encoding encode;
            switch (encoding)
            {
                case CmppConstants.Encoding.GBK:
                    encode = Encoding.GetEncoding("gb2312");
                    break;
                case CmppConstants.Encoding.ASCII:
                    encode = Encoding.ASCII;
                    break;
                case CmppConstants.Encoding.UCS2:
                    encode = Encoding.BigEndianUnicode;
                    break;
                default:
                    return null;
            }
            var buffer = new byte[byteLen];
            var bytes = encode.GetBytes(value);
            Array.Copy(bytes, 0, buffer, 0, Math.Min(bytes.Length, byteLen));
            return buffer;
        }
        /// <summary>
        /// 计算字符串长度（该长度为转换为字节流后的长度）。
        /// </summary>
        public static byte Length(string value, byte coding)
        {
            var buffer = ToBytes(value, coding);
            return (byte)(buffer == null ? 0 : buffer.Length);
        }
        /// <summary>
        /// 字节流编码。
        /// </summary>
        public static byte[] ToBytes(uint value)
        {
            var bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            return bytes;
        }
        /// <summary>
        /// 字节流编码。
        /// </summary>
        public static byte[] ToBytes(ulong value)
        {
            var bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            return bytes;
        }
        /// <summary>
        /// 字节流解码。
        /// </summary>
        public static uint ToUInt32(byte[] bytes, int index)
        {
            Array.Reverse(bytes, index, 4);
            return BitConverter.ToUInt32(bytes, index);
        }
        /// <summary>
        /// 字节流解码。
        /// </summary>
        public static ulong ToUInt64(byte[] bytes, int index)
        {
            Array.Reverse(bytes, index, 8);
            return BitConverter.ToUInt64(bytes, index);
        }
        #endregion
    }
}
