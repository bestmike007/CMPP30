using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Reefoo.CMPP30.Message
{
    /// <summary>
    /// SP 向 ISMG 发起删除短信（CMPP_CANCEL）操作（SP->ISMG）。
    /// </summary>
    /// <remarks>
    /// CMPP_CANCEL 操作的目的是 SP 通过此操作可以将已经提交给 ISMG 的短信删除，ISMG 将以 CMPP_CANCEL_RESP 回应删除操作的结果。
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    internal struct CmppCancel : ICmppMessage
    {
        #region 消息体
        /// <summary>
        /// 信息标识（SP 想要删除的信息标识）。
        /// </summary>
        public ulong MsgId;
        #endregion

        public uint GetCommandId()
        {
            return CmppConstants.CommandCode.Cancel;
        }

        public byte[] ToBytes()
        {
            var buffer = new List<byte>(CmppConstants.PackageBodySize.CmppCancel);
            buffer.AddRange(Convert.ToBytes(MsgId));
            return buffer.ToArray();
        }

        public void FromBytes(byte[] body)
        {
            if (body == null || body.Length != CmppConstants.PackageBodySize.CmppCancel)
                throw new ArgumentException(string.Format("Invalid bytes to unmarshal for {0}.", GetType().Name));
            MsgId = Convert.ToUInt64(body, 0);
        }
    }
}
