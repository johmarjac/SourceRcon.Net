using System;
using System.Net.Sockets;

namespace SourceRcon.Net
{
    public interface IRconClient : IDisposable
    {
        /// <summary>
        /// Gets the underlying socket instance.
        /// </summary>
        Socket Socket { get; }

        /// <summary>
        /// Gets the configuration for this rcon client instance.
        /// </summary>
        IRconConfiguration Configuration { get; }
    }
}
