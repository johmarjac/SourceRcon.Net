using System.IO;

namespace SourceRcon.Net
{
    public class RconPacket : IRconPacket
    {

        private readonly MemoryStream _packetStream;

        public readonly BinaryWriter _packetWriter;

        public readonly BinaryReader _packetReader;
                
        public RconPacketStateType State { get; }

        public int PacketId { get; private set; }

        public int PacketType { get; private set; }

        public byte[] Buffer
        {
            get
            {
                if (State == RconPacketStateType.Write)
                {
                    long oldPosition = _packetStream.Position;

                    _packetStream.Seek(0, SeekOrigin.Begin);
                    _packetWriter.Write((int)_packetStream.Length - sizeof(int) + sizeof(byte));
                    _packetStream.Seek(0, SeekOrigin.End);
                    _packetWriter.Write((byte)'\0');
                    _packetStream.Seek((int)oldPosition, SeekOrigin.Begin);
                }

                return _packetStream.ToArray();
            }
        }

        public RconPacket(int packetId = 0, int packetType = 2)
        {
            State = RconPacketStateType.Write;
            _packetStream = new MemoryStream();
            _packetWriter = new BinaryWriter(_packetStream);
            _packetReader = new BinaryReader(_packetStream);

            _packetWriter.Write(0);           // Size
            _packetWriter.Write(packetId);    // Packet Id
            _packetWriter.Write(packetType);  // Packet Type
        }

        public RconPacket(byte[] buffer)
        {
            State = RconPacketStateType.Read;
            _packetStream = new MemoryStream(buffer);
            _packetWriter = new BinaryWriter(_packetStream);
            _packetReader = new BinaryReader(_packetStream);
        }

        public virtual void Deserialize()
        {
            PacketId = _packetReader.ReadInt32();
            PacketType = _packetReader.ReadInt32();
        }

        public void Dispose()
        {
        }
    }
}
