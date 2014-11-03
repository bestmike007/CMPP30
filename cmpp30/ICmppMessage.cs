namespace Reefoo.CMPP30
{
    /// <summary>
    /// Cmpp 30 message interface.
    /// </summary>
    public interface ICmppMessage
    {
        /// <summary>
        /// Get command id of this message.
        /// </summary>
        /// <returns></returns>
        uint GetCommandId();
        /// <summary>
        /// Convert message to bytes.
        /// </summary>
        /// <returns></returns>
        byte[] ToBytes();
        /// <summary>
        /// Restore message from byte stream.
        /// </summary>
        /// <param name="body"></param>
        void FromBytes(byte[] body);
    }
}
