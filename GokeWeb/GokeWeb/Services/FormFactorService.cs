using Goke.Core.Interfaces;

namespace GokeWeb.Services
{
    public class FormFactorService : IFormFactor
    {
        public string GetFormFactor()
        {
            return "Web";
        }

        public string GetPlatform()
        {
            return Environment.OSVersion.ToString();
        }
    }
}
