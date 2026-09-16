using System.Threading;

namespace LTSFrontend.Core.Http
{
    public class TokenRefreshGate
    {
        public SemaphoreSlim Lock { get; } = new(1, 1);
    }
}