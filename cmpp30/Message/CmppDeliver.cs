using System;
using System.Runtime.InteropServices;

namespace Reefoo.CMPP30.Message
{
    /// <summary>
    /// ISMG 向 SP 送交短信（CMPP_DELIVER）操作（ISMG->SP）。
    /// </summary>
    /// <remarks>
    /// CMPP_DELIVER 操作的目的是 ISMG 把从短信中心或其它 ISMG 转发来的短信送交 SP，SP 以 CMPP_DELIVER_RESP 消息回应。
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    internal struct CmppDeliver : ICmppMessage
    {
        #region 消息体
        /// <summary>
        /// 信息标识；生成算法如下：采用64位（8字节）的整数：（1）时间（格式为MMDDHHMMSS，即月日时分秒）：bit64~bit39，其中bit64~bit61：月份的二进制表示；bit60~bit56：日的二进制表示；bit55~bit51：小时的二进制表示；bit50~bit45：分的二进制表示；bit44~bit39：秒的二进制表示；（2）短信网关代码：bit38~bit17，把短信网关的代码转换为整数填写到该字段中；（3）序列号：bit16~bit1，顺序增加，步长为1，循环使用。各部分如不能填满，左补零，右对齐。
        /// </summary>
        public ulong MsgId;
        /// <summary>
        /// 目的号码（SP的服务代码，一般4--6位，或者是前缀为服务代码的长号码；该号码是手机用户短消息的被叫号码；SP的服务代码：服务代码是在使用短信方式的上行类业务中，提供给用户使用的服务提供商代码。服务代码以数字表示，全国业务服务代码长度为4位，即“1000”－“9999”；本地业务服务代码长度统一为5位，即“01000”－“09999”；信产部对新的SP的服务代码分配提出了新的要求，要求以“1061”－“1069”作为前缀，目前中国移动进行了如下分配：1062：用于省内SP服务代码1066：用于全国SP服务代码其它号段保留）。
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 21)]
        public string DestId;
        /// <summary>
        /// 业务标识，是数字、字母和符号的组合（SP的业务类型，数字、字母和符号的组合，由SP自定，如图片传情可定为TPCQ，股票查询可定义为11）。
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
        public string ServiceId;
        /// <summary>
        /// GSM协议类型（详细解释请参考GSM03.40中的9.2.3.9）。
        /// </summary>
        public byte TPPId;
        /// <summary>
        /// GSM协议类型（详细解释请参考GSM03.40中的9.2.3.23，仅使用1位，右对齐）。
        /// </summary>
        public byte TPUdhi;
        /// <summary>
        /// 信息格式（0：ASCII串；3：短信写卡操作；4：二进制信息；8：UCS2编码；15：含GB汉字）。
        /// </summary>
        public byte MsgFmt;
        /// <summary>
        /// 源终端MSISDN号码（状态报告时填为CMPP_SUBMIT消息的目的终端号码）。
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
        public string SrcTerminalId;
        /// <summary>
        /// 源终端号码类型（0：真实号码；1：伪码）。
        /// </summary>
        public byte SrcTerminalType;
        /// <summary>
        /// 是否为状态报告（0：非状态报告；1：状态报告）。
        /// </summary>
        public byte RegisteredDelivery;
        /// <summary>
        /// 消息长度，取值大于或等于0。
        /// </summary>
        public byte MsgLength;
        /// <summary>
        /// 消息内容。
        /// </summary>
        public string MsgContent;
        /// <summary>
        /// 点播业务使用的LinkID，非点播类业务的MT流程不使用该字段。
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string LinkId;
        #endregion

        #region 公有方法
        /// <summary>
        /// 初始化 CMPP_DELIVER。
        /// </summary>
        public bool Init(byte[] buffer)
        {
            var position = 0;
            try
            {
                MsgId = BitConverter.ToUInt64(buffer, 0);
                position = position + 8;

                DestId = Convert.ToString(buffer, position, 21, CmppEncoding.ASCII);
                position = position + 21;

                ServiceId = Convert.ToString(buffer, position, 10, CmppEncoding.ASCII);
                position = position + 10;

                TPPId = buffer[position];
                position++;

                TPUdhi = buffer[position];
                position++;

                MsgFmt = buffer[position];
                position++;

                SrcTerminalId = Convert.ToString(buffer, position, 32, CmppEncoding.ASCII);
                position = position + 32;

                SrcTerminalType = buffer[position];
                position++;

                RegisteredDelivery = buffer[position];
                position++;

                MsgLength = buffer[position];
                position++;

                MsgContent = RegisteredDelivery == 0
                    ? Convert.ToString(buffer, position, MsgLength, (CmppEncoding)MsgFmt)
                    : System.Convert.ToBase64String(buffer, position, MsgLength);

                position = position + MsgLength;
                LinkId = Convert.ToString(buffer, position, 20, CmppEncoding.ASCII);
            }
            catch
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 获取 CMPP_SUBMIT 的状态报告（只有在 CMPP_SUBMIT 中的 RegisteredDelivery 被设置为1时，ISMG才会向SP发送状态报告）。
        /// </summary>
        /// <returns></returns>
        public CmppReport GetReport()
        {
            var report = new CmppReport();
            if (RegisteredDelivery != 1) return report;
            var bytes = System.Convert.FromBase64String(MsgContent);
            if ((bytes.Length > 0)) report.FromBytes(bytes);
            return report;
        }
        /// <summary>
        /// 
        /// </summary>
        public override string ToString()
        {
            return MsgContent;
        }
        #endregion

        public uint GetCommandId()
        {
            return CmppConstants.CommandCode.Deliver;
        }

        public byte[] ToBytes()
        {
            throw new NotImplementedException();
        }

        public void FromBytes(byte[] buffer)
        {
            var position = 0;
            MsgId = BitConverter.ToUInt64(buffer, 0);
            position = position + 8;

            DestId = Convert.ToString(buffer, position, 21, CmppEncoding.ASCII);
            position = position + 21;

            ServiceId = Convert.ToString(buffer, position, 10, CmppEncoding.ASCII);
            position = position + 10;

            TPPId = buffer[position];
            position++;

            TPUdhi = buffer[position];
            position++;

            MsgFmt = buffer[position];
            position++;

            SrcTerminalId = Convert.ToString(buffer, position, 32, CmppEncoding.ASCII);
            position = position + 32;

            SrcTerminalType = buffer[position];
            position++;

            RegisteredDelivery = buffer[position];
            position++;

            MsgLength = buffer[position];
            position++;

            MsgContent = RegisteredDelivery == 0
                ? Convert.ToString(buffer, position, MsgLength, (CmppEncoding)MsgFmt)
                : System.Convert.ToBase64String(buffer, position, MsgLength);

            position = position + MsgLength;
            LinkId = Convert.ToString(buffer, position, 20, CmppEncoding.ASCII);
        }
    }
}
