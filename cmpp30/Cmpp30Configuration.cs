namespace Reefoo.CMPP30
{
    /// <summary>
    /// Configurations for a CMPP30 connection session
    /// </summary>
    public struct Cmpp30Configuration
    {
        /// <summary>
        /// Gateway (CMCC) IP to connect.
        /// </summary>
        public string GatewayIp { get; set; }

        /// <summary>
        /// Gateway port to connect
        /// </summary>
        public int GatewayPort { get; set; }

        /// <summary>
        /// Username to login into gateway
        /// </summary>
        public string GatewayUsername { get; set; }

        /// <summary>
        /// Password to login into gateway
        /// </summary>
        public string GatewayPassword { get; set; }

        /// <summary>
        /// Also called the source id, code for the SP as a phone number, e.g. 1065750000.
        /// </summary>
        public string SpCode { get; set; }

        /// <summary>
        /// SP service subscription id, e.g. MSC0010501.
        /// </summary>
        public string ServiceId { get; set; }

        /// <summary>
        /// Try to remove message signature.
        /// </summary>
        public bool AttemptRemoveSignature { get; set; }

        /// <summary>
        /// Configure to allow or disallow long message.
        /// </summary>
        public bool DisableLongMessage { get; set; }

        /// <summary>
        /// The signature is placed prepositively by the gateway
        /// </summary>
        public bool PrepositiveGatewaySignature { get; set; }

        /// <summary>
        /// The signature bound to the SP code.
        /// </summary>
        public string GatewaySignature { get; set; }

        /// <summary>
        /// Configure to split long message to short messages.
        /// </summary>
        public bool SendLongMessageAsShortMessages { get; set; }
    }

}
