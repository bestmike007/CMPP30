using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Reefoo.CMPP30.Message
{
    /// <summary>
    /// CMPP_SUBMIT_RESP 消息定义（ISMG->SP）。
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    internal struct CmppSubmitResp : ICmppMessage
    {
        #region 消息体
        /// <summary>
        /// 信息标识，生成算法如下：采用64位（8字节）的整数：（1）时间（格式为MMDDHHMMSS，即月日时分秒）：bit64~bit39，其中bit64~bit61：月份的二进制表示；bit60~bit56：日的二进制表示；bit55~bit51：小时的二进制表示；bit50~bit45：分的二进制表示；bit44~bit39：秒的二进制表示；（2）短信网关代码：bit38~bit17，把短信网关的代码转换为整数填写到该字段中；（3）序列号：bit16~bit1，顺序增加，步长为1，循环使用。各部分如不能填满，左补零，右对齐（SP根据请求和应答消息的Sequence_Id一致性就可得到CMPP_Submit消息的Msg_Id）。
        /// </summary>
        public ulong MsgId;
        /// <summary>
        /// 结果（0：正确；1：消息结构错；2：命令字错；3：消息序号重复；4：消息长度错；5：资费代码错；6：超过最大信息长；7：业务代码错；8：流量控制错；9：本网关不负责服务此计费号码；10：Src_Id错误；11：Msg_src错误；12：Fee_terminal_Id错误；13：Dest_terminal_Id错误...）。
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
                    return "成功";
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
                case 9:
                    return "本网关不负责服务此计费号码";
                case 10:
                    return "Src_Id 错误";
                case 11:
                    return "Msg_src 错误";
                case 12:
                    return "Fee_terminal_Id 错误";
                case 13:
                    return "Dest_terminal_Id 错误";
                default:
                    return "其他错误";
            }
        }
        #endregion

        public uint GetCommandId()
        {
            return CmppConstants.CommandCode.SubmitResp;
        }

        public byte[] ToBytes()
        {
            var buffer = new List<byte>(CmppConstants.PackageBodySize.CmppSubmitResp);
            buffer.AddRange(BitConverter.GetBytes(MsgId));
            buffer.AddRange(Convert.ToBytes(Result));
            return buffer.ToArray();
        }

        public void FromBytes(byte[] body)
        {
            if (body == null || body.Length != CmppConstants.PackageBodySize.CmppSubmitResp)
                throw new ArgumentException(string.Format("Invalid bytes to unmarshal for {0}.", GetType().Name));
            MsgId = BitConverter.ToUInt64(body, 0);
            Result = Convert.ToUInt32(body, 8);
        }
    }
}
