using System;
using System.Runtime.InteropServices;

namespace Reefoo.CMPP30.Message
{
    /// <summary>
    /// CMPP_CONNECT_RESP 消息定义（ISMG->SP）。
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    internal struct CmppConnectResp : ICmppMessage
    {
        #region 消息体
        /// <summary>
        /// 状态（0：正确；1：消息结构错；2：非法源地址；3：认证错；4：版本太高；5~ ：其他错误）。
        /// </summary>
        public uint Status;
        /// <summary>
        /// ISMG 认证码，用于鉴别 ISMG。其值通过单向 MD5 hash 计算得出，表示如下：AuthenticatorISMG = MD5(Status + AuthenticatorSource + shared secret)，Shared secret 由中国移动与源地址实体事先商定，AuthenticatorSource 为源地址实体发送给 ISMG 的对应消息 CMPP_Connect 中的值。认证出错时，此项为空。
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] AuthenticatorISMG;
        /// <summary>
        /// 服务器支持的最高版本号，对于 3.0 的版本，高 4 bit 为 3，低 4 位为 0。
        /// </summary>
        public byte Version;
        #endregion

        #region 公有方法
        public override string ToString()
        {
            switch (Status)
            {
                case 0:
                    return "成功";
                case 1:
                    return "消息结构错";
                case 2:
                    return "非法源地址";
                case 3:
                    return "认证错";
                case 4:
                    return "版本太高";
                default:
                    return string.Format("其他错误（错误码：{0}）", Status);
            }
        }
        #endregion

        public uint GetCommandId()
        {
            return CmppConstants.CommandCode.ConnectResp;
        }

        public byte[] ToBytes()
        {
            throw new NotImplementedException();
        }

        public void FromBytes(byte[] body)
        {
            if (body == null || body.Length != CmppConstants.PackageBodySize.CmppConnectResp)
                throw new ArgumentException(string.Format("Invalid bytes to unmarshal for {0}.", GetType().Name));
            Status = Convert.ToUInt32(body, 0);
            AuthenticatorISMG = new byte[16];
            Array.Copy(body, 4, AuthenticatorISMG, 0, 16);
            Version = body[CmppConstants.PackageBodySize.CmppConnectResp - 1];
        }
    }
}
