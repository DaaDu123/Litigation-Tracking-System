namespace LTSFrontend.Core.Http
{
    /// <summary>
    /// Captures the real browser IP of the current user's circuit so ApiClient can pass it to the API
    /// in X-Forwarded-For. Without it every user looks like the frontend server's single IP and the API's
    /// per-IP rate limits are shared by everybody.
    ///
    /// Scoped (one per circuit). In Blazor Server the HttpContext is only reliably available while the
    /// circuit is being set up, so the IP is read in the constructor - this class is resolved right when
    /// the circuit's ApiClient is created (see ServiceCollectionExtensions). If it can't be determined it is
    /// simply null and the API falls back to the connection's own address, exactly as before.
    ///
    /// RemoteIpAddress is already the real client address when the frontend sits behind nginx/Cloudflare,
    /// because Program.cs runs UseForwardedHeaders() first.
    /// </summary>
    public class ClientIpAccessor
    {
        public string? IpAddress { get; }

        public ClientIpAccessor(IHttpContextAccessor httpContextAccessor)
        {
            var ip = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress;

            if (ip != null)
            {
                if (ip.IsIPv4MappedToIPv6)
                    ip = ip.MapToIPv4();

                IpAddress = ip.ToString();
            }
        }
    }
}
