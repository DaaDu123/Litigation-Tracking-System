namespace LTSFrontend.Core.Http
{













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
