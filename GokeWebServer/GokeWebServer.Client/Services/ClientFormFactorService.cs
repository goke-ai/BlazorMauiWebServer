using Goke.Core.Interfaces;

namespace GokeWebServer.Client.Services
{
    public class ClientFormFactorService : IFormFactor
    {
        public string GetFormFactor()
        {
            return "WebAssembly";
        }

        public string GetPlatform()
        {
            return Environment.OSVersion.ToString();
        }
    }
}
