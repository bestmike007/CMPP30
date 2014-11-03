using System;
using System.Runtime.InteropServices;

namespace Reefoo.CMPP30.Message
{
    /// <summary>
    /// CMPP_ACTIVE_TEST 定义（SP->ISMG 或 ISMG->SP）。
    /// </summary>
    /// <remarks>
    /// 链路检测（CMPP_ACTIVE_TEST）操作：本操作仅适用于通信双方采用长连接通信方式时用于保持连接。
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    internal struct CmppActiveTest : ICmppMessage
    {
        #region 消息体
        public byte Reserved;
        #endregion

        public uint GetCommandId()
        {
            return CmppConstants.CommandCode.ActiveTest;
        }

        public byte[] ToBytes()
        {
            return new[] { Reserved };
        }

        public void FromBytes(byte[] body)
        {
            if (body == null || body.Length != CmppConstants.PackageBodySize.CmppActiveTest)
                throw new ArgumentException(string.Format("Invalid bytes to unmarshal for {0}.", GetType().Name));
            Reserved = body[0];
        }
    }
}
