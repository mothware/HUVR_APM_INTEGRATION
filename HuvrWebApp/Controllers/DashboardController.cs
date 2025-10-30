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

        [HttpGet]
        public async Task<IActionResult> GetAssetTypes()
        {
            var client = GetClientFromSession();
            if (client == null)
            {
                return Json(new { success = false, error = "Not authenticated" });
            }

            try
            {
                var assetTypes = await client.GetActiveAssetTypesAsync();
                return Json(new { success = true, data = assetTypes });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExecuteMethod(string entityType, string method, string? parameters, string? filters)
        {
            var client = GetClientFromSession();
            if (client == null)
            {
                return Json(new { success = false, error = "Not authenticated" });
            }

            try
            {
                // Parse filters JSON if provided
                Dictionary<string, string>? filterDict = null;
                if (!string.IsNullOrEmpty(filters))
                {
                    filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(filters);
                }

                object? result = null;

                switch (entityType.ToLower())
                {
                    case "assets":
                        result = await ExecuteAssetMethod(client, method, parameters, filterDict);
                        break;
                    case "projects":
                        result = await ExecuteProjectMethod(client, method, parameters, filterDict);
                        break;
                    case "defects":
                        result = await ExecuteDefectMethod(client, method, parameters, filterDict);
                        break;
                    case "measurements":
                        result = await ExecuteMeasurementMethod(client, method, parameters, filterDict);
                        break;
                    case "inspectionmedia":
                        result = await ExecuteInspectionMediaMethod(client, method, parameters, filterDict);
                        break;
                    case "checklists":
                        result = await ExecuteChecklistMethod(client, method, parameters, filterDict);
                        break;
                    case "users":
                        result = await ExecuteUserMethod(client, method, parameters, filterDict);
                        break;
                    case "workspaces":
                        result = await ExecuteWorkspaceMethod(client, method, parameters, filterDict);
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

        private async Task<object?> ExecuteAssetMethod(HuvrApiClient.HuvrApiClient client, string method, string? parameters, Dictionary<string, string>? filters)
        {
            return method.ToLower() switch
            {
                "list" => await client.GetAllAssetsAsync(filters),
                "get" => await client.GetAssetAsync(parameters ?? ""),
                _ => throw new ArgumentException("Unknown method")
            };
        }

        private async Task<object?> ExecuteProjectMethod(HuvrApiClient.HuvrApiClient client, string method, string? parameters, Dictionary<string, string>? filters)
        {
            return method.ToLower() switch
            {
                "list" => await client.GetAllProjectsAsync(filters),
                "get" => await client.GetProjectAsync(parameters ?? ""),
                _ => throw new ArgumentException("Unknown method")
            };
        }

        private async Task<object?> ExecuteDefectMethod(HuvrApiClient.HuvrApiClient client, string method, string? parameters, Dictionary<string, string>? filters)
        {
            return method.ToLower() switch
            {
                "list" => await client.GetAllDefectsAsync(filters),
                "get" => await client.GetDefectAsync(parameters ?? ""),
                _ => throw new ArgumentException("Unknown method")
            };
        }

        private async Task<object?> ExecuteMeasurementMethod(HuvrApiClient.HuvrApiClient client, string method, string? parameters, Dictionary<string, string>? filters)
        {
            return method.ToLower() switch
            {
                "list" => await client.GetAllMeasurementsAsync(filters),
                "get" => await client.GetMeasurementAsync(parameters ?? ""),
                _ => throw new ArgumentException("Unknown method")
            };
        }

        private async Task<object?> ExecuteInspectionMediaMethod(HuvrApiClient.HuvrApiClient client, string method, string? parameters, Dictionary<string, string>? filters)
        {
            return method.ToLower() switch
            {
                "list" => await client.GetAllInspectionMediaAsync(filters),
                "get" => await client.GetInspectionMediaAsync(parameters ?? ""),
                _ => throw new ArgumentException("Unknown method")
            };
        }

        private async Task<object?> ExecuteChecklistMethod(HuvrApiClient.HuvrApiClient client, string method, string? parameters, Dictionary<string, string>? filters)
        {
            return method.ToLower() switch
            {
                "list" => await client.ListChecklistsAsync(filters ?? new Dictionary<string, string>()),
                "get" => await client.GetChecklistAsync(parameters ?? ""),
                _ => throw new ArgumentException("Unknown method")
            };
        }

        private async Task<object?> ExecuteUserMethod(HuvrApiClient.HuvrApiClient client, string method, string? parameters, Dictionary<string, string>? filters)
        {
            return method.ToLower() switch
            {
                "list" => await client.ListUsersAsync(filters ?? new Dictionary<string, string>()),
                "get" => await client.GetUserAsync(parameters ?? ""),
                _ => throw new ArgumentException("Unknown method")
            };
        }

        private async Task<object?> ExecuteWorkspaceMethod(HuvrApiClient.HuvrApiClient client, string method, string? parameters, Dictionary<string, string>? filters)
        {
            return method.ToLower() switch
            {
                "list" => await client.ListWorkspacesAsync(filters ?? new Dictionary<string, string>()),
                "get" => await client.GetWorkspaceAsync(parameters ?? ""),
                _ => throw new ArgumentException("Unknown method")
            };
        }
    }
}
