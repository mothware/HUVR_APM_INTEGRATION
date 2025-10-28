using HuvrApiClient;

namespace HuvrWebApp.Services
{
    public interface IHuvrService
    {
        HuvrApiClient.HuvrApiClient? GetClient(string sessionId);
        void SetClient(string sessionId, HuvrApiClient.HuvrApiClient client);
        void RemoveClient(string sessionId);
    }
}
