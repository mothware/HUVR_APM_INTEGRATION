using Microsoft.AspNetCore.Mvc;
using HuvrWebApp.Models;
using HuvrWebApp.Services;
using HuvrApiClient;

namespace HuvrWebApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHuvrService _huvrService;
        private readonly IConfiguration _configuration;

        public AuthController(IHuvrService huvrService, IConfiguration configuration)
        {
            _huvrService = huvrService;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // If already logged in, redirect to dashboard
            if (HttpContext.Session.GetString("SessionId") != null)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var baseUrl = _configuration["HuvrApi:BaseUrl"] ?? "https://api.huvrdata.app";
                var config = new HuvrApiClientConfig
                {
                    ClientId = model.ClientId,
                    ClientSecret = model.ClientSecret,
                    BaseUrl = baseUrl
                };

                var client = new HuvrApiClient.HuvrApiClient(config);

                // Test the connection by trying to authenticate
                await client.EnsureAuthenticatedAsync();

                // Generate a session ID and store the client
                var sessionId = Guid.NewGuid().ToString();
                HttpContext.Session.SetString("SessionId", sessionId);
                HttpContext.Session.SetString("ClientId", model.ClientId);
                _huvrService.SetClient(sessionId, client);

                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                model.ErrorMessage = $"Login failed: {ex.Message}";
                return View(model);
            }
        }

        public IActionResult Logout()
        {
            var sessionId = HttpContext.Session.GetString("SessionId");
            if (sessionId != null)
            {
                _huvrService.RemoveClient(sessionId);
            }

            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
