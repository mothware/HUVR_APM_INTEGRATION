using System.Collections.Concurrent;
using HuvrApiClient;

namespace HuvrWebApp.Services
{
    public class HuvrService : IHuvrService
    {
        private readonly ConcurrentDictionary<string, HuvrApiClient.HuvrApiClient> _clients = new();

        public HuvrApiClient.HuvrApiClient? GetClient(string sessionId)
        {
            _clients.TryGetValue(sessionId, out var client);
            return client;
        }

        public void SetClient(string sessionId, HuvrApiClient.HuvrApiClient client)
        {
            _clients[sessionId] = client;
        }

        public void RemoveClient(string sessionId)
        {
            _clients.TryRemove(sessionId, out _);
        }
    }
}
