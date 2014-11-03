using System;

namespace Reefoo.CMPP30
{
    /// <summary>
    /// MO message receive event args
    /// </summary>
    public class ReceiveEventArgs : EventArgs
    {
        /// <summary>
        /// Gateway message id 
        /// </summary>
        public long MessageId { get; set; }

        /// <summary>
        /// MO message source
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// MO message destination
        /// </summary>
        public string Destination { get; set; }

        /// <summary>
        /// MO message content
        /// </summary>
        public string Content { get; set; }
    }

    /// <summary>
    /// SMS send report event args.
    /// </summary>
    public class ReportEventArgs : EventArgs
    {
        /// <summary>
        /// Gateway message id 
        /// </summary>
        public long MessageId { get; set; }

        /// <summary>
        /// Message destination
        /// </summary>
        public string Destination { get; set; }

        /// <summary>
        /// Report status text
        /// </summary>
        public string StatusText { get; set; }
    }
}
