using SourceRcon.Net.Packets;
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
            var config = new RconClientConfiguration()
            {
                Address = "127.0.0.1",
                Port = 7779
            };

            try
            {
                using (var client = new RconClient(config))
                {
                    await client.ConnectAsync();

                    if (client.Socket.Connected)
                    {
                        if (await client.AuthenticateAsync("test"))
                        {
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
                throw ex;
            }

            Console.ReadLine();
        }
    }
}
