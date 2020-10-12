using System;

namespace SourceRcon.Net
{
    public interface IRconPacket : IDisposable
    {
        byte[] Buffer { get; }

        RconPacketStateType State { get; }
    }
}
