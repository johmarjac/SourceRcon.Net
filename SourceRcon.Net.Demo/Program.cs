﻿using SourceRcon.Net.Packets;
using System;
using System.Threading.Tasks;

namespace SourceRcon.Net.Demo
{
    class Program
    {
        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        private async Task MainAsync()
        {
            var config = new RconClientConfiguration("127.0.0.1", 7600);

            try
            {
                using (var client = new RconClient(config))
                {
                    await client.ConnectAsync();
                    await client.StartClientAsync();

                    if (client.Socket.Connected)
                    {
                        if (await client.AuthenticateAsync("test"))
                        {
                            _ = Task.Factory.StartNew(async () =>
                            {
                                while(true)
                                {
                                    await client.ExecuteCommandAsync<RconResultPacket>("string ping");
                                    await Task.Delay(1000);
                                    Console.WriteLine(client.QueuedPackets.Count);
                                }
                            });

                            Console.WriteLine("Authentication successful!");
                            while (true)
                            {
                                var inp = Console.ReadLine();
                                var result = await client.ExecuteCommandAsync<RconResultPacket>(inp);

                                Console.WriteLine(result.Result);
                            }
                        }
                        else
                            Console.WriteLine("Authentication failed!");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.ReadLine();
        }
    }
}
