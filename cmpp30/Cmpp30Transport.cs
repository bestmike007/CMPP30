using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Reefoo.CMPP30.Message;

namespace Reefoo.CMPP30
{
    internal class Cmpp30Transport : IDisposable
    {
        private readonly Cmpp30Configuration _config;
        private readonly byte[] _buffer = new byte[1024];
        private readonly Queue<byte> _receiveQueue = new Queue<byte>();
        private readonly object _lock = new object();
        private readonly ManualResetEvent _exitEvent = new ManualResetEvent(false);
        private readonly Thread _backgroundThread;

        private Socket _socket;
        private ReceiveState _state;

        public Cmpp30Transport(Cmpp30Configuration config)
        {
            _config = config;
            _backgroundThread = new Thread(Loop) { Name = string.Format("Cmpp30Transport-{0:HHmmss}", DateTime.Now) };
            _backgroundThread.Start();
        }

        public event EventHandler<CmppMessageReceiveEvent> OnCmppMessageReceive;
        public event EventHandler<EventArgs> OnDisconnected;

        public void Connect()
        {
            lock (_lock)
            {
                if (Connected) return;
                var connectEndEvent = new ManualResetEvent(false);
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

                _socket.BeginConnect(_config.GatewayIp, _config.GatewayPort, delegate(IAsyncResult result)
                {
                    try
                    {
                        var socket = (Socket)result.AsyncState;
                        socket.EndConnect(result);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Unbale to connect, error: {0}.", ex);
                    }
                    connectEndEvent.Set();
                }, _socket);

                if (connectEndEvent.WaitOne(10000) && Connected)
                {
                    _state.Size = CmppConstants.HeaderSize;
                    _state.PackageType = typeof(CmppHead);
                    _state.Header = null;
                    return;
                }

                Console.WriteLine("Fail to connect to remote host {0}:{1}", _config.GatewayIp, _config.GatewayPort);
                Disconnect();
            }
        }

        private void Loop()
        {
            while (!_exitEvent.WaitOne(0))
            {
                if (!Connected)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                try
                {
                    lock (_lock)
                    {
                        if (!_socket.Connected || _socket.Poll(0, SelectMode.SelectError))
                        {
                            Disconnect();
                            continue;
                        }

                        if (_socket.Poll(50000, SelectMode.SelectRead))
                        {
                            var size = _socket.Receive(_buffer);
                            if (size == 0)
                            {
                                Disconnect();
                                continue;
                            }

                            lock (_receiveQueue)
                                for (var i = 0; i < size; i++) _receiveQueue.Enqueue(_buffer[i]);
                        }
                        while (_receiveQueue.Count >= _state.Size) _DataReady();
                    }
                }
                catch (Exception)
                {
                    Disconnect();
                }
            }
        }

        private void _DataReady()
        {
            var buffer = new byte[_state.Size];
            lock (_receiveQueue)
            {
                for (var i = 0; i < buffer.Length; i++)
                    buffer[i] = _receiveQueue.Dequeue();
            }

            var package = Activator.CreateInstance(_state.PackageType) as ICmppMessage;
            if (package == null) throw new Exception(string.Format("Unexpected response for {0}", _state.PackageType.Name));
            package.FromBytes(buffer);
            if (package is CmppHead)
            {
                var header = (CmppHead)package;
                _state.Header = header;
                _state.PackageType = MessageTypes[header.CommandId];
                _state.Size = (int)(header.TotalLength - CmppConstants.HeaderSize);
            }
            else
            {
                if (!_state.Header.HasValue || _state.PackageType != package.GetType())
                    throw new Exception(string.Format("Unexpected response type {0} for expected package {1}",
                        package.GetType().Name, _state.PackageType.Name));

                _OnCmppMessageReceive(new CmppMessageReceiveEvent
                {
                    Header = _state.Header.Value,
                    Message = package
                });
                _state.Size = CmppConstants.HeaderSize;
                _state.PackageType = typeof(CmppHead);
                _state.Header = null;
            }
        }

        private void _OnCmppMessageReceive(CmppMessageReceiveEvent evt)
        {
            if (OnCmppMessageReceive != null) OnCmppMessageReceive(this, evt);
        }

        public void Disconnect()
        {
            lock (_lock)
            {
                try
                {
                    _state.Size = CmppConstants.HeaderSize;
                    _state.PackageType = typeof(CmppHead);
                    _state.Header = null;
                    lock (_receiveQueue) _receiveQueue.Clear();

                    if (_socket == null) return;
                    _socket.Disconnect(false);
                    _socket.Dispose();
                }
                finally
                {
                    _socket = null;
                    if (OnDisconnected != null) OnDisconnected.Invoke(this, null);
                }
            }
        }

        public bool Send(uint sequenceId, ICmppMessage message)
        {
            lock (_lock)
            {
                if (!Connected) return false;
                try
                {
                    if (_socket.Poll(0, SelectMode.SelectError) || !_socket.Poll(50000, SelectMode.SelectWrite))
                    {
                        Disconnect();
                        return false;
                    }

                    var buffer = message.ToBytes();
                    var header = new CmppHead
                    {
                        SequenceId = sequenceId,
                        TotalLength = (uint)(CmppConstants.HeaderSize + buffer.Length),
                        CommandId = message.GetCommandId()
                    };
                    _socket.Send(header.ToBytes());
                    _socket.Send(buffer);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error sending {0}. Error: {1}.", message.GetType().Name, ex);
                    Disconnect();
                    return false;
                }
                return true;
            }
        }

        public bool Connected
        {
            get
            {
                return _socket != null && _socket.Connected;
            }
        }

        public void Dispose()
        {
            _exitEvent.Set();
            Disconnect();
            _backgroundThread.Join();
        }

        private struct ReceiveState
        {
            public int Size;
            public Type PackageType;
            public CmppHead? Header;
        }

        private static readonly Dictionary<uint, Type> MessageTypes = new Dictionary<uint, Type>
        {
            { CmppConstants.CommandCode.ActiveTest, typeof(CmppActiveTest) },
            { CmppConstants.CommandCode.ActiveTestResp, typeof(CmppActiveTestResp) },
            { CmppConstants.CommandCode.Cancel, typeof(CmppCancel) },
            { CmppConstants.CommandCode.CancelResp, typeof(CmppCancelResp) },
            { CmppConstants.CommandCode.Connect, typeof(CmppConnect) },
            { CmppConstants.CommandCode.ConnectResp, typeof(CmppConnectResp) },
            { CmppConstants.CommandCode.Deliver, typeof(CmppDeliver) },
            { CmppConstants.CommandCode.DeliverResp, typeof(CmppDeliverResp) },
            { CmppConstants.CommandCode.Submit, typeof(CmppSubmit) },
            { CmppConstants.CommandCode.SubmitResp, typeof(CmppSubmitResp) },
        };
    }

    /// <summary>
    /// Receive cmpp 30 message event
    /// </summary>
    internal class CmppMessageReceiveEvent : EventArgs
    {
        /// <summary>
        /// Received header
        /// </summary>
        public CmppHead Header { get; set; }
        /// <summary>
        /// Received message body
        /// </summary>
        public ICmppMessage Message { get; set; }
    }
}
