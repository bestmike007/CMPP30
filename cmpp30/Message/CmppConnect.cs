using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Reefoo.CMPP30.Message
{
    /// <summary>
    /// SP 请求连接到 ISMG（CMPP_CONNECT）操作（SP->ISMG）。
    /// </summary>
    /// <remarks>
    /// CMPP_CONNECT 操作的目的是 SP 向 ISMG 注册作为一个合法 SP 身份，若注册成功后即建立了“应用层”的连接，此后 SP 可以通过此 ISMG 接收和发送短信。ISMG 以 CMPP_CONNECT_RESP 消息响应SP的请求。
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    internal struct CmppConnect : ICmppMessage
    {

        #region 消息体
        /// <summary>
        /// 源地址，此处为SP_Id，即 SP 的企业代码（长度为 6 字节）。
        /// </summary>
        /// <remarks>
        /// SP_Id（SP 的企业代码）：网络中 SP 地址和身份的标识、地址翻译、计费、结算等均以企业代码为依据。企业代码以数字表示，共 6 位，从“9XY000”至“9XY999”，其中“XY”为各移动公司代码。
        /// </remarks>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 6)]
        public string SourceAddress;
        /// <summary>
        /// 用于鉴别源地址。其值通过单向 MD5 hash 计算得出，表示如下：AuthenticatorSource =MD5(Source_Addr + 9 字节的 0 + shared secret + timestamp)，Shared secret 由中国移动与源地址实体事先商定，timestamp 格式为：MMDDHHMMSS，即月日时分秒，10位。
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] AuthenticatorSource;
        /// <summary>
        /// 双方协商的版本号（高位 4 bit 表示主版本号,低位 4 bit 表示次版本号），对于 3.0 的版本，高 4bit 为 3，低 4 位为 0。
        /// </summary>
        public byte Version;
        /// <summary>
        /// 时间戳的明文，由客户端产生，格式为 MMDDHHMMSS，即：月日时分秒，10 位数字的整型，右对齐。
        /// </summary>
        public uint TimeStamp;
        #endregion

        public uint GetCommandId()
        {
            return CmppConstants.CommandCode.Connect;
        } 

        public byte[] ToBytes()
        {
            var buffer = new List<byte>(CmppConstants.PackageBodySize.CmppConnect);
            buffer.AddRange(Convert.ToBytes(SourceAddress, CmppConstants.Encoding.ASCII, 6));
            buffer.AddRange(AuthenticatorSource);
            buffer.Add(Version);
            buffer.AddRange(Convert.ToBytes(TimeStamp));
            return buffer.ToArray();
        }

        public void FromBytes(byte[] body)
        {
            throw new NotImplementedException();
        }
    }
}
