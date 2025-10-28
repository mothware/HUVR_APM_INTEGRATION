using Microsoft.AspNetCore.Mvc;
using HuvrWebApp.Services;
using HuvrApiClient;
using Newtonsoft.Json;

namespace HuvrWebApp.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IHuvrService _huvrService;

        public DashboardController(IHuvrService huvrService)
        {
            _huvrService = huvrService;
        }

        private HuvrApiClient.HuvrApiClient? GetClientFromSession()
        {
            var sessionId = HttpContext.Session.GetString("SessionId");
            if (sessionId == null)
            {
                return null;
            }
            return _huvrService.GetClient(sessionId);
        }

        public IActionResult Index()
        {
            var client = GetClientFromSession();
            if (client == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.ClientId = HttpContext.Session.GetString("ClientId");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ExecuteMethod(string entityType, string method, string? parameters)
        {
            var client = GetClientFromSession();
            if (client == null)
            {
                return Json(new { success = false, error = "Not authenticated" });
            }

            try
            {
                object? result = null;

                switch (entityType.ToLower())
                {
                    case "assets":
                        result = await ExecuteAssetMethod(client, method, parameters);
                        break;
                    case "projects":
                        result = await ExecuteProjectMethod(client, method, parameters);
                        break;
                    case "defects":
                        result = await ExecuteDefectMethod(client, method, parameters);
                        break;
                    case "measurements":
                        result = await ExecuteMeasurementMethod(client, method, parameters);
                        break;
                    case "inspectionmedia":
                        result = await ExecuteInspectionMediaMethod(client, method, parameters);
                        break;
                    case "checklists":
                        result = await ExecuteChecklistMethod(client, method, parameters);
                        break;
                    case "users":
                        result = await ExecuteUserMethod(client, method, parameters);
                        break;
                    case "workspaces":
                        result = await ExecuteWorkspaceMethod(client, method, parameters);
                        break;
                    default:
                        return Json(new { success = false, error = "Unknown entity type" });
                }

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        private async Task<object?> ExecuteAssetMethod(HuvrApiClient.HuvrApiClient client, string method, string? parameters)
        {
            return method.ToLower() switch
            {
                "list" => await client.GetAllAssetsAsync(),
                "get" => await client.GetAssetAsync(parameters ?? ""),
                _ => throw new ArgumentException("Unknown method")
            };
        }

        private async Task<object?> ExecuteProjectMethod(HuvrApiClient.HuvrApiClient client, string method, string? parameters)
        {
            return method.ToLower() switch
            {
                "list" => await client.GetAllProjectsAsync(),
                "get" => await client.GetProjectAsync(parameters ?? ""),
                _ => throw new ArgumentException("Unknown method")
            };
        }

        private async Task<object?> ExecuteDefectMethod(HuvrApiClient.HuvrApiClient client, string method, string? parameters)
        {
            return method.ToLower() switch
            {
                "list" => await client.GetAllDefectsAsync(),
                "get" => await client.GetDefectAsync(parameters ?? ""),
                _ => throw new ArgumentException("Unknown method")
            };
        }

        private async Task<object?> ExecuteMeasurementMethod(HuvrApiClient.HuvrApiClient client, string method, string? parameters)
        {
            return method.ToLower() switch
            {
                "list" => await client.GetAllMeasurementsAsync(),
                "get" => await client.GetMeasurementAsync(parameters ?? ""),
                _ => throw new ArgumentException("Unknown method")
            };
        }

        private async Task<object?> ExecuteInspectionMediaMethod(HuvrApiClient.HuvrApiClient client, string method, string? parameters)
        {
            return method.ToLower() switch
            {
                "list" => await client.GetAllInspectionMediaAsync(),
                "get" => await client.GetInspectionMediaAsync(parameters ?? ""),
                _ => throw new ArgumentException("Unknown method")
            };
        }

        private async Task<object?> ExecuteChecklistMethod(HuvrApiClient.HuvrApiClient client, string method, string? parameters)
        {
            return method.ToLower() switch
            {
                "list" => await client.ListChecklistsAsync(),
                "get" => await client.GetChecklistAsync(parameters ?? ""),
                _ => throw new ArgumentException("Unknown method")
            };
        }

        private async Task<object?> ExecuteUserMethod(HuvrApiClient.HuvrApiClient client, string method, string? parameters)
        {
            return method.ToLower() switch
            {
                "list" => await client.ListUsersAsync(),
                "get" => await client.GetUserAsync(parameters ?? ""),
                _ => throw new ArgumentException("Unknown method")
            };
        }

        private async Task<object?> ExecuteWorkspaceMethod(HuvrApiClient.HuvrApiClient client, string method, string? parameters)
        {
            return method.ToLower() switch
            {
                "list" => await client.ListWorkspacesAsync(),
                "get" => await client.GetWorkspaceAsync(parameters ?? ""),
                _ => throw new ArgumentException("Unknown method")
            };
        }
    }
}
