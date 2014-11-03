using System;
using System.Runtime.InteropServices;

namespace Reefoo.CMPP30.Message
{
    /// <summary>
    /// CMPP_CANCEL_RESP 消息定义（ISMG->SP）
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    internal struct CmppCancelResp : ICmppMessage
    {
        #region 消息体
        /// <summary>
        /// 成功标识（0：成功；1：失败）。
        /// </summary>
        public uint SuccessId;
        #endregion

        public uint GetCommandId()
        {
            return CmppConstants.CommandCode.CancelResp;
        }

        public byte[] ToBytes()
        {
            return Convert.ToBytes(SuccessId);
        }

        public void FromBytes(byte[] body)
        {
            if (body == null || body.Length != CmppConstants.PackageBodySize.CmppCancelResp)
                throw new ArgumentException(string.Format("Invalid bytes to unmarshal for {0}.", GetType().Name));
            SuccessId = Convert.ToUInt32(body, 0);
        }
    }
}
