using System.Text;

namespace SourceRcon.Net.Packets
{
    public class RconResultPacket : RconPacket
    {

        public string Result { get; private set; }

        public RconResultPacket(byte[] buffer) : base(buffer)
        {
        }

        public override void Deserialize()
        {
            base.Deserialize();

            var len = (int)(_packetWriter.BaseStream.Length - _packetWriter.BaseStream.Position - 1);
            var bData = _packetReader.ReadBytes(len);
            Result = Encoding.Default.GetString(bData);
        }
    }
}
