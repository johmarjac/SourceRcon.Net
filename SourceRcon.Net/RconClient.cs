using SourceRcon.Net.Packets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        public SynchronizedCollection<RconResultPacket> QueuedPackets { get; }

        public ConcurrentDictionary<int, RconResultPacket> QueuedCommandPackets { get; }

        private static object _lockObj = new object();

        public RconClient()
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            QueuedPackets = new SynchronizedCollection<RconResultPacket>();
            QueuedCommandPackets = new ConcurrentDictionary<int, RconResultPacket>();
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

        public Task StartClientAsync()
        {
            Task.Factory.StartNew(async () =>
            {
                while(Socket.Connected)
                {
                    var packetSize = BitConverter.ToInt32(await ReceiveBytesAsync(4));
                    var packetData = await ReceiveBytesAsync(packetSize);
                    var packet = Activator.CreateInstance(typeof(RconResultPacket), packetData) as RconResultPacket;

                    packet.Deserialize();

                    lock(_lockObj)
                    {
                        if (QueuedCommandPackets.ContainsKey(packet.PacketId))
                            QueuedCommandPackets[packet.PacketId] = packet;
                        else
                            QueuedPackets.Add(packet);
                    }
                }
            });

            return Task.CompletedTask;
        }

        public async Task<bool> AuthenticateAsync(string rconPassword)
        {
            QueuedCommandPackets.TryAdd(0, null);

            using (var packet = new RconPacket(0, 3))
            {
                var bPassword = Encoding.Default.GetBytes(rconPassword);
                packet._packetWriter.Write(bPassword);

                await SendAsync(packet.Buffer);
            }

            return (await ReceivePacketAsync<RconAuthResultPacket>(0))
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
            var packetId = Environment.TickCount;

            QueuedCommandPackets.TryAdd(packetId, null);

            using(var packet = new RconPacket(packetId, 2))
            {
                var data = Encoding.Default.GetBytes(command);
                packet._packetWriter.Write(data, 0, data.Length);

                await SendAsync(packet.Buffer);
            }

            return await ReceivePacketAsync<TPacket>(packetId);
        }

        public async Task<TPacket> ReceivePacketAsync<TPacket>(int expectedPacketId = -1)
            where TPacket : RconPacket
        {
            RconResultPacket packet = null;

            await Task.Run(() =>
            {
                while (packet == null)
                {
                    lock (_lockObj)
                    {
                        if (expectedPacketId != -1)
                            packet = QueuedCommandPackets.FirstOrDefault(x => x.Value != null && x.Value.PacketId == expectedPacketId).Value;
                        else
                            packet = QueuedPackets.FirstOrDefault();
                    }
                }
            });

            if (packet == null)
                return null;

            lock (_lockObj)
            {
                if (packet != null)
                {
                    if (QueuedCommandPackets.ContainsKey(packet.PacketId))
                        QueuedCommandPackets.Remove(packet.PacketId, out var p);
                    else
                        QueuedPackets.Remove(packet);
                }
            }

            var tPacket = Activator.CreateInstance(typeof(TPacket), packet.Buffer) as TPacket;

            tPacket.Deserialize();

            return tPacket;
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
