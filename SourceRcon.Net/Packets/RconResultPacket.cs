using System.Collections.Generic;
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

            byte b = 0;
            var bData = new List<byte>();
            while((b = _packetReader.ReadByte()) != '\0')
            {
                bData.Add(b);
            }

            Result = Encoding.Default.GetString(bData.ToArray());
        }
    }
}
