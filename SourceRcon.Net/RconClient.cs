using SourceRcon.Net.Packets;
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SourceRcon.Net
{
    public class RconClient : IRconClient
    {

        private bool _disposedValue;

        public Socket Socket { get; internal set; }

        public IRconConfiguration Configuration { get; }

        public RconClient()
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public RconClient(IRconConfiguration configuration) : this()
        {
            Configuration = configuration;
        }

        public Task ConnectAsync()
        {
            try
            {
                return Socket.ConnectAsync(Configuration.Address, Configuration.Port);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> AuthenticateAsync(string rconPassword)
        {
            using (var packet = new RconPacket(0, 3))
            {
                var bPassword = Encoding.Default.GetBytes(rconPassword);
                packet._packetWriter.Write(bPassword);

                await SendAsync(packet.Buffer);
            }

            return (await ReceivePacketAsync<RconAuthResultPacket>())
                .Authenticated;
        }

        private async Task<byte[]> ReceiveBytesAsync(int amountBytesExpected)
        {
            var bData = new byte[amountBytesExpected];
            var bytesReceivedTotal = 0;

            using (var stream = new MemoryStream(bData))
            using (var writer = new BinaryWriter(stream))
            {
                while (bytesReceivedTotal < amountBytesExpected)
                {
                    var bTemp = new byte[amountBytesExpected - bytesReceivedTotal];
                    var bytesReceived = await Socket.ReceiveAsync(bTemp, SocketFlags.None);
                    bytesReceivedTotal += bytesReceived;
                    writer.Write(bTemp, 0, bytesReceived);
                }
            }

            return bData;
        }

        public async Task<TPacket> ExecuteCommandAsync<TPacket>(string command) where TPacket : RconPacket
        {
            using(var packet = new RconPacket(1, 2))
            {
                var data = Encoding.Default.GetBytes(command);
                packet._packetWriter.Write(data, 0, data.Length);

                await SendAsync(packet.Buffer);
            }

            return await ReceivePacketAsync<TPacket>();
        }

        private async Task<TPacket> ReceivePacketAsync<TPacket>()
            where TPacket : RconPacket
        {
            var packetSize = BitConverter.ToInt32(await ReceiveBytesAsync(4));
            var packetData = await ReceiveBytesAsync(packetSize);
            var packet = Activator.CreateInstance(typeof(TPacket), packetData) as TPacket;

            packet.Deserialize();

            return packet;
        }

        private async Task SendAsync(byte[] bData)
        {
            var bytesSent = 0;

            while(bytesSent < bData.Length)
            {
                bytesSent += await Socket.SendAsync(bData.Skip(bytesSent).ToArray(), SocketFlags.None);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {

                }

                Socket?.Dispose();
                Socket = null;

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
