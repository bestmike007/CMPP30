namespace Reefoo.CMPP30
{
    /// <summary>
    /// ISMG response status
    /// </summary>
    public enum Cmpp30SendStatus
    {
        /// <summary>
        /// Success
        /// </summary>
        Success,
        /// <summary>
        /// Send message timeout
        /// </summary>
        Timeout,
        /// <summary>
        /// Sending too fast
        /// </summary>
        Congested,
        /// <summary>
        /// Gateway not connected
        /// </summary>
        NotConnected,
        /// <summary>
        /// Configuration error, e.g. wrong source id, wrong service id.
        /// </summary>
        ConfigError,
        /// <summary>
        /// Message exceeded the maximun allowed length.
        /// </summary>
        MessageTooLong,
        /// <summary>
        /// Other unexpected error
        /// </summary>
        Unknown
    }
}