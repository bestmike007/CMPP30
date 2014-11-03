using System.IO;
using System.Linq;
#if DEBUG
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using NUnit.Framework;
using Reefoo.CMPP30;
using Reefoo.CMPP30.Message;

namespace Reefoo.Mms.Vas.Test
{
    public class Cmpp30Tests
    {
        private readonly HashSet<long> _msgIds = new HashSet<long>();
        private int _totalSent;
        private static Cmpp30Configuration _config = new Cmpp30Configuration
        {
            GatewayIp = "211.137.85.115",
            GatewayPort = 7891,
            GatewayUsername = "470XXX",
            GatewayPassword = "470XXX",
            SpCode = "106575000000",
            ServiceId = "MSC0000000",
            GatewaySignature = "【中国移动】",
            PrepositiveGatewaySignature = true,
            AttemptRemoveSignature = false,
            DisableLongMessage = false,
            SendLongMessageAsShortMessages = false,
        };

        [TestCase]
        public void TestConnect()
        {
            _config.AttemptRemoveSignature = false;
            using (var client = new Cmpp30Client(_config))
            {
                client.OnMessageReport += client_OnMessageReport;
                client.Start();
                for (var i = 0; i < 100; i++)
                {
                    Thread.Sleep(50);
                    if (client.Status == Cmpp30ClientStatus.Connected) break;
                }
                if (client.Status != Cmpp30ClientStatus.Connected)
                {
                    Console.WriteLine("Client connection failed. Status: {0} {1}.", client.Status, client.StatusText);
                    return;
                }
                Console.WriteLine("Connected.");
                List<long> msgIds;
                Console.WriteLine(client.Send("", new[] { "13800138000", "13800138001" }, "测试发送短信", out msgIds));
                while (true)
                {
                    Thread.Sleep(2500);
                    Console.WriteLine(client.Status);
                }
            }
        }

        void client_OnMessageReport(object sender, ReportEventArgs e)
        {
            Console.WriteLine(e.MessageId);
            Console.WriteLine(e.StatusText);
        }

        [TestCase]
        public void TestBatchSend()
        {
            _config.AttemptRemoveSignature = true;
            using (var client = new Cmpp30Client(_config))
            {
                client.Start();
                for (var i = 0; i < 100; i++)
                {
                    Thread.Sleep(50);
                    if (client.Status == Cmpp30ClientStatus.Connected) break;
                }
                if (client.Status != Cmpp30ClientStatus.Connected)
                {
                    Console.WriteLine("Client connection failed.");
                    return;
                }
                Console.WriteLine("Connected.");
                var numbers = File.ReadAllLines("numbers.txt");
                var count = 0;
                foreach (var number in numbers.Where(x => !string.IsNullOrEmpty(x)))
                {
                    Console.WriteLine("Sending message no. {0}. ", ++count);
                    var status = Cmpp30SendStatus.Congested;
                    while (status == Cmpp30SendStatus.Congested)
                    {
                        List<long> msgIds;
                        status = client.Send("10086", number, "测试发送短信。", out msgIds, false);
                    }
                }
            }
        }

        [TestCase]
        public void SequentialTestCmpp30()
        {
            using (var client = new Cmpp30Client(_config))
            {
                client.OnMessageReport += _OnMessageReport;
                client.Start();
                for (var i = 0; i < 100; i++)
                {
                    Thread.Sleep(50);
                    if (client.Status == Cmpp30ClientStatus.Connected) break;
                }
                if (client.Status != Cmpp30ClientStatus.Connected)
                {
                    Console.WriteLine("Client connection failed.");
                    return;
                }
                Console.WriteLine("Connected.");

                for (var i = 0; i < 100; i++)
                {
                    List<long> idList;
                    var response = client.Send("999", "13800138000", "测试发送短信", out idList);
                    if (response == Cmpp30SendStatus.Congested) continue;

                    Assert.AreEqual(Cmpp30SendStatus.Success, response);
                    Assert.AreEqual(1, idList.Count);
                    _msgIds.Add(idList[0]);
                }
                while (true)
                {
                    if (_msgIds.Count == 0) break;
                    Console.WriteLine("Waiting for {0} more reports", _msgIds.Count);
                    Thread.Sleep(2000);
                }
            }
        }

        [TestCase]
        public void ParallelTestCmpp30()
        {
            using (var client = new Cmpp30Client(_config))
            {
                client.OnMessageReport += _OnMessageReport;
                client.OnMessageReceive += OnMessageReceive;
                client.Start();
                for (var i = 0; i < 100; i++)
                {
                    Thread.Sleep(50);
                    if (client.Status == Cmpp30ClientStatus.Connected) break;
                }
                if (client.Status != Cmpp30ClientStatus.Connected)
                {
                    Console.WriteLine("Client connection failed.");
                    return;
                }
                Console.WriteLine("Connected.");
                var tasks = new List<Task>();
                for (var i = 0; i < 100; i++)
                {
                    tasks.Add(Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            List<long> idList;
                            var response = client.Send("999", "13800138000", " ", out idList);
                            if (response == Cmpp30SendStatus.Congested)
                            {
                                Console.WriteLine("Congested");
                                return;
                            }

                            if (response != Cmpp30SendStatus.Success || idList.Count != 1) return;
                            //Assert.AreEqual(Cmpp30SendStatus.Success, response);
                            //Assert.AreEqual(1, idList.Count);
                            _msgIds.Add(idList[0]);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }));
                }
                Task.WaitAll(tasks.ToArray());
                while (true)
                {
                    if (_msgIds.Count == 0) break;
                    Console.WriteLine("Waiting for {0} more reports", _msgIds.Count);
                    Thread.Sleep(2000);
                }
            }
        }

        [TestCase]
        public void ContinuouslySend()
        {
            var dueTime = DateTime.Now.AddMinutes(5);
            using (var client = new Cmpp30Client(_config))
            {
                client.OnMessageReport += _OnMessageReport;
                client.OnMessageReceive += OnMessageReceive;
                client.Start();
                for (var i = 0; i < 100; i++)
                {
                    Thread.Sleep(50);
                    if (client.Status == Cmpp30ClientStatus.Connected) break;
                }
                if (client.Status != Cmpp30ClientStatus.Connected)
                {
                    Console.WriteLine("Client connection failed.");
                    return;
                }
                Console.WriteLine("Connected.");
                while (dueTime > DateTime.Now)
                {
                    var tasks = new List<Task>();
                    for (var i = 0; i < 3; i++)
                    {
                        tasks.Add(Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                List<long> idList;
                                var response = client.Send("999", "13800138000", " ", out idList);
                                if (response == Cmpp30SendStatus.Congested)
                                {
                                    //Console.WriteLine("Congested");
                                    return;
                                }
                                _totalSent++;
                                Assert.AreEqual(Cmpp30SendStatus.Success, response);
                                Assert.AreEqual(1, idList.Count);
                                lock (_msgIds) _msgIds.Add(idList[0]);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                            }
                        }));
                    }
                    Task.WaitAll(tasks.ToArray());
                    lock (_msgIds) Console.WriteLine("Waiting for {0} more reports", _msgIds.Count);
                }
                Console.WriteLine("Send {0} messages. Wait for reports.", _totalSent);
                while (true)
                {
                    if (_msgIds.Count == 0) break;
                    Console.WriteLine("Waiting for {0} more reports", _msgIds.Count);
                    Thread.Sleep(2000);
                }
            }
        }

        static void OnMessageReceive(object sender, ReceiveEventArgs e)
        {
            Console.WriteLine("Received message {0} from {1} to {2}.", e.Content, e.Source, e.Destination);
        }

        void _OnMessageReport(object sender, ReportEventArgs e)
        {
            Console.WriteLine("Received message report, {0}, {1}.", e.MessageId, e.StatusText);
            lock (_msgIds)
                if (_msgIds.Contains(e.MessageId)) _msgIds.Remove(e.MessageId);
                else Console.WriteLine("Message id not found: {0}.", e.MessageId);
        }

        [TestCase]
        public void TestTransport()
        {
            using (var transport = new Cmpp30Transport(_config))
            {
                transport.OnCmppMessageReceive += transport_OnCmppMessageReceive;
                transport.Connect();
                Assert.True(transport.Connected);
                var dt = DateTime.Now;
                transport.Send(_seq++, new CmppConnect
                {
                    TimeStamp = uint.Parse(string.Format("{0:MMddhhmmss}", dt)),
                    AuthenticatorSource = CreateAuthenticatorSource(dt),
                    Version = CmppConstants.Version,
                    SourceAddress = _config.GatewayUsername,
                });
                Assert.True(_connectEvent.WaitOne(5000));
                Console.WriteLine("Client connected.");
                transport.Send(_seq++, new CmppActiveTest());
                Assert.True(_activeResponse.WaitOne(5000));
                transport.Send(_seq++, new CmppActiveTest());
                Assert.True(_activeResponse.WaitOne(5000));
                transport.Send(_seq++, new CmppActiveTest());
                Assert.True(_activeResponse.WaitOne(5000));
                transport.Send(_seq++, new CmppActiveTest());
                Assert.True(_activeResponse.WaitOne(5000));
                transport.Send(_seq++, new CmppActiveTest());
                Assert.True(_activeResponse.WaitOne(5000));
                transport.Send(_seq++, new CmppActiveTest());
                Assert.True(_activeResponse.WaitOne(5000));
            }
        }

        private readonly ManualResetEvent _connectEvent = new ManualResetEvent(false);
        private readonly AutoResetEvent _activeResponse = new AutoResetEvent(false);
        void transport_OnCmppMessageReceive(object sender, CmppMessageReceiveEvent e)
        {
            Console.WriteLine("Recieved {0}", e.Message.GetType().Name);
            if (e.Message is CmppConnectResp)
            {
                Assert.AreEqual(0, ((CmppConnectResp)e.Message).Status);
                _connectEvent.Set();
            }
            if (e.Message is CmppActiveTestResp)
            {
                _activeResponse.Set();
            }
        }

        [TestCase]
        public void TestProtocol()
        {
            using (var client = new TcpClient(_config.GatewayIp, _config.GatewayPort))
            {
                var dt = DateTime.Now;
                SendMessage(client.Client, new CmppConnect
                {
                    TimeStamp = uint.Parse(string.Format("{0:MMddhhmmss}", dt)),
                    AuthenticatorSource = CreateAuthenticatorSource(dt),
                    Version = CmppConstants.Version,
                    SourceAddress = _config.GatewayUsername,
                });
                Assert.True(client.Client.Poll(-1, SelectMode.SelectRead));
                var buffer = new byte[12];
                Assert.AreEqual(buffer.Length, client.Client.Receive(buffer));
                var head = new CmppHead();
                head.FromBytes(buffer);
                Assert.AreEqual(CmppConstants.CommandCode.ConnectResp, head.CommandId);

                Assert.True(client.Client.Poll(-1, SelectMode.SelectRead));
                buffer = new byte[head.TotalLength - 12];
                Assert.AreEqual(buffer.Length, client.Client.Receive(buffer));
                var response = new CmppConnectResp();
                response.FromBytes(buffer);
                Assert.AreEqual(0, response.Status);
                // connected
                SendMessage(client.Client, new CmppActiveTest());

                Assert.True(client.Client.Poll(-1, SelectMode.SelectRead));
                buffer = new byte[12];
                Assert.AreEqual(buffer.Length, client.Client.Receive(buffer));
                head = new CmppHead();
                head.FromBytes(buffer);
                Assert.AreEqual(CmppConstants.CommandCode.ActiveTestResp, head.CommandId);

                Assert.True(client.Client.Poll(-1, SelectMode.SelectRead));
                buffer = new byte[head.TotalLength - 12];
                Assert.AreEqual(buffer.Length, client.Client.Receive(buffer));
                var active = new CmppActiveTestResp();
                active.FromBytes(buffer);

                Console.WriteLine("Sending message.");
                var submit = new CmppSubmit
                {
                    // 信息内容。
                    MsgContent = "Test",
                    // 信息编码。
                    MsgFmt = (byte)CmppEncoding.UCS2,
                    // SP的服务代码，将显示在最终用户手机上的短信主叫号码。
                    SrcId = _config.SpCode + "1111",
                    // 接收短信的电话号码列表。
                    DestTerminalId = new[] { "13800138000" },
                    // 业务标识（如：woodpack）。
                    ServiceId = _config.ServiceId,
                    // 是否要求返回状态报告。
                    RegisteredDelivery = 1,
                    // 资费类别。
                    FeeType = string.Format("{0:D2}", (int)FeeType.Free),
                    // 计费用户。
                    FeeUserType = (byte)FeeUserType.SP,
                    // 被计费的号码（feeUserType 值为 FeeUser 时有效）。
                    FeeTerminalId = _config.SpCode,
                    // 被计费号码的真实身份（“真实号码”或“伪码”）。
                    FeeTerminalType = 0,
                    // 信息费（以“分”为单位，如：10 分代表 1角）。
                    FeeCode = "10",
                    // 点播业务的 linkId。
                    LinkId = "",
                    PkTotal = (byte)(_config.AttemptRemoveSignature ? 3 : 1),
                    PkNumber = (byte)(_config.AttemptRemoveSignature ? 2 : 1),
                    MsgLevel = 0,
                    TPPId = 0,
                    TPUdhi = 0,
                    MsgSrc = _config.GatewayUsername,
                    ValidTime = "",
                    AtTime = ""
                };
                SendMessage(client.Client, submit);

                Assert.True(client.Client.Poll(-1, SelectMode.SelectRead));
                buffer = new byte[12];
                Assert.AreEqual(buffer.Length, client.Client.Receive(buffer));
                head = new CmppHead();
                head.FromBytes(buffer);
                Assert.AreEqual(CmppConstants.CommandCode.SubmitResp, head.CommandId);

                Assert.True(client.Client.Poll(-1, SelectMode.SelectRead));
                buffer = new byte[head.TotalLength - 12];
                Assert.AreEqual(buffer.Length, client.Client.Receive(buffer));
                var submitResponse = new CmppSubmitResp();
                submitResponse.FromBytes(buffer);
                Assert.AreEqual(0, submitResponse.Result);

                Console.WriteLine("Waiting for message report.");

                Assert.True(client.Client.Poll(-1, SelectMode.SelectRead));
                buffer = new byte[12];
                Assert.AreEqual(buffer.Length, client.Client.Receive(buffer));
                head = new CmppHead();
                head.FromBytes(buffer);
                Assert.AreEqual(CmppConstants.CommandCode.Deliver, head.CommandId);

                Assert.True(client.Client.Poll(-1, SelectMode.SelectRead));
                buffer = new byte[head.TotalLength - 12];
                Assert.AreEqual(buffer.Length, client.Client.Receive(buffer));
                var deliver = new CmppDeliver();
                deliver.FromBytes(buffer);
                Assert.AreEqual(1, deliver.RegisteredDelivery);
                var report = deliver.GetReport();
                Assert.AreEqual(submitResponse.MsgId, report.MsgId);

                Console.WriteLine("Message report: [{0}] {1}", report.MsgId, report.Stat);
            }
        }

        private static uint _seq;
        private static void SendMessage(Socket socket, ICmppMessage message)
        {
            var buffer = message.ToBytes();
            var head = new CmppHead
            {
                CommandId = message.GetCommandId(),
                SequenceId = _seq++,
                TotalLength = (uint)(CmppConstants.HeaderSize + buffer.Length)
            };
            socket.Send(head.ToBytes());
            socket.Send(buffer);
        }


        private static byte[] CreateAuthenticatorSource(DateTime timestamp)
        {
            var btContent = new byte[25 + _config.GatewayPassword.Length];
            Array.Clear(btContent, 0, btContent.Length);

            // Source_Addr，SP的企业代码（6位）。
            var iPos = 0;
            foreach (var ch in _config.GatewayUsername)
            {
                btContent[iPos] = (byte)ch;
                iPos++;
            }

            // 9字节的0。
            iPos += 9;

            // password，由 China Mobile 提供（长度不固定）。
            foreach (var ch in _config.GatewayPassword)
            {
                btContent[iPos] = (byte)ch;
                iPos++;
            }

            // 时间戳（10位）。
            foreach (var ch in string.Format("{0:MMddhhmmss}", timestamp))
            {
                btContent[iPos] = (byte)ch;
                iPos++;
            }
            return new MD5CryptoServiceProvider().ComputeHash(btContent);
        }
    }
}
#endif