using System;
using System.Threading;

namespace Reefoo.CMPP30
{
    /// <summary>
    /// Background worker thread base class util.
    /// </summary>
    public abstract class BackgroundThread : IDisposable
    {
        private readonly ManualResetEvent _stopEvent = new ManualResetEvent(false);
        private Thread _backgroundThread;

        /// <summary>
        /// Run next thread loop.
        /// </summary>
        protected abstract void NextLoop();

        private void Loop()
        {
            while (!_stopEvent.WaitOne(0))
            {
                try
                {
                    NextLoop();
                }
                catch (Exception ex)
                {
                    if (ex is ThreadAbortException) break;
                    Console.WriteLine("Error to execute background service. error: {0}.", ex);
                    Thread.Sleep(1000);
                }
            }
            _OnStop();
            Disposed = true;
        }

        /// <summary>
        /// When worker thread start
        /// </summary>
        protected virtual void _OnStart() { }
        /// <summary>
        /// When worker thread stopped
        /// </summary>
        protected virtual void _OnStop() { }

        /// <summary>
        /// Start worker thread.
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        public void Start()
        {
            if (Disposed) throw new ObjectDisposedException(GetType().FullName);
            _backgroundThread = new Thread(Loop) { Name = string.Format("{0}-{1:HHmmss}", GetType().Name, DateTime.Now) };
            _OnStart();
            _backgroundThread.Start();
        }

        /// <summary>
        /// Stop the thread.
        /// </summary>
        public void Stop()
        {
            Dispose();
        }

        /// <summary>
        /// Dispose the thread.
        /// </summary>
        public void Dispose()
        {
            Disposed = true;
            _stopEvent.Set();
            _backgroundThread.Join();
        }

        /// <summary>
        /// Check if disposed
        /// </summary>
        public bool Disposed { get; private set; }
    }
}
