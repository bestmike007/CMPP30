using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Reefoo.CMPP30.Message
{
    /// <summary>
    /// CMPP_DELIVER_RESP 消息定义（SP->ISMG）。
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    internal struct CmppDeliverResp : ICmppMessage
    {
        #region 消息体
        /// <summary>
        /// 信息标识（CMPP_DELIVER中的 Msg_Id 字段）。
        /// </summary>
        public ulong MsgId;
        /// <summary>
        /// 结果（0：正确；1：消息结构错；2：命令字错；3：消息序号重复；4：消息长度错；5：资费代码错；6：超过最大信息长；7：业务代码错；8: 流量控制错；9~ ：其他错误）。
        /// </summary>
        public uint Result;
        #endregion

        #region 公有方法
        /// <summary>
        /// 
        /// </summary>
        public override string ToString()
        {
            switch (Result)
            {
                case 0:
                    return "正确";
                case 1:
                    return "消息结构错";
                case 2:
                    return "命令字错";
                case 3:
                    return "消息序号重复";
                case 4:
                    return "消息长度错";
                case 5:
                    return "资费代码错";
                case 6:
                    return "超过最大信息长";
                case 7:
                    return "业务代码错";
                case 8:
                    return "流量控制错";
                default:
                    return "其他错误";
            }
        }
        #endregion

        public uint GetCommandId()
        {
            return CmppConstants.CommandCode.DeliverResp;
        }

        public byte[] ToBytes()
        {
            var buffer = new List<byte>();
            buffer.AddRange(BitConverter.GetBytes(MsgId));
            buffer.AddRange(Convert.ToBytes(Result));
            return buffer.ToArray();
        }

        public void FromBytes(byte[] body)
        {
            throw new NotImplementedException();
        }
    }
}
