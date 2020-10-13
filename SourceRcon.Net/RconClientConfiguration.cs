namespace SourceRcon.Net
{
    public class RconClientConfiguration : IRconConfiguration
    {
        public string Address { get; set; }

        public int Port { get; set; }

        public RconClientConfiguration(string address, int port)
        {
            Address = address;
            Port = port;
        }
    }
}
