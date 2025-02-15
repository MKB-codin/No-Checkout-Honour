using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SelfCheckoutApp.Services
{
    public class ServerStatusService
    {
        private readonly HttpClient _httpClient;
        private Timer _timer;
        public bool IsServerOnline { get; private set; } = true;

        public event EventHandler ServerOffline;

        public ServerStatusService(string baseUrl)
        {
            _httpClient = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
            })
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromSeconds(5)
            };
        }


        public void StartChecking()
        {
            // Check every 10 seconds.
            _timer = new Timer(async _ => await CheckServerStatusAsync(), null, 0, 10000);
        }


        public void StopChecking()
        {
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
            _timer?.Dispose();
        }

        private async Task CheckServerStatusAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/health");
                bool online = response.IsSuccessStatusCode;
                if (!online && IsServerOnline)
                {
                    IsServerOnline = false;
                    OnServerOffline();
                }
                else if (online)
                {
                    IsServerOnline = true;
                }
            }
            catch
            {
                if (IsServerOnline)
                {
                    IsServerOnline = false;
                    OnServerOffline();
                }
            }
        }

        protected virtual void OnServerOffline()
        {
            ServerOffline?.Invoke(this, EventArgs.Empty);
        }
    }
}
