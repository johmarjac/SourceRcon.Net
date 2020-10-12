namespace SourceRcon.Net.Packets
{
    public sealed class RconAuthResultPacket : RconPacket
    {

        public bool Authenticated
        {
            get
            {
                return PacketType == 2 && PacketId == 0;
            }
        }

        public RconAuthResultPacket(byte[] buffer) : base(buffer)
        {
        }
    }
}
