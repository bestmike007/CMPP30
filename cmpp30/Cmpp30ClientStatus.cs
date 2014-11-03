namespace Reefoo.CMPP30
{
    /// <summary>
    /// Status of CMPP 3.0 Client
    /// </summary>
    public enum Cmpp30ClientStatus
    {
        /// <summary>
        /// CMPP 3.0 successfully connected.
        /// </summary>
        Connected,
        /// <summary>
        /// Connecting to gateway
        /// </summary>
        Connecting,
        /// <summary>
        /// Authenticating user.
        /// </summary>
        Authenticating,
        /// <summary>
        /// Fail to authenticate, view status text for detail.
        /// </summary>
        AuthenticationFailed,
        /// <summary>
        /// Client disconnected.
        /// </summary>
        Disconnected,
        /// <summary>
        /// Client disposed.
        /// </summary>
        Disposed,
    }
}
