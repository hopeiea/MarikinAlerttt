using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using MarikinAlert.Web.Services;
using MarikinAlert.Web.Models;
using MarikinAlert.Web.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using System;

namespace MarikinAlert.Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly IDisasterTriageService _triageService;
        private readonly AppDbContext _context;

        public AdminController(IDisasterTriageService triageService, AppDbContext context)
        {
            _triageService = triageService;
            _context = context;
        }

        // =========================================================
        //  1. AUTHENTICATION
        // =========================================================

        [HttpGet]
        public IActionResult Login()
        {
            // Check session-based authentication
            //if (IsAuthenticated()) return RedirectToAction("Dashboard");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Username and password are required";
                return View();
            }

            // 1. MASTER KEY (Bypasses Database)
            if (username == "admin" && (password == "marikina123" || password == "admin123"))
            {
                await SetAdminSession("admin", "HeadDispatcher");
                return RedirectToAction("Dashboard");
            }

            // 2. Database Check - Optimized query
            var user = await _context.AdminUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == username);
                
            if (user != null && (user.Password == password || user.PasswordHash == password))
            {
                await SetAdminSession(user.Username, user.Role);
                return RedirectToAction("Dashboard");
            }

            // Delay to prevent timing attacks
            await Task.Delay(500);
            ViewBag.Error = "Invalid credentials";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogoutPost()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // =========================================================
        //  2. SECURE PAGES
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Dashboard(string categoryFilter = "All")
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login");
            }

            // Start with base query excluding Noise
            var query = _context.Reports
                .Where(r => r.Category != ReportCategory.Noise)
                .AsNoTracking();

            // Apply category filter if specified
            if (!string.IsNullOrEmpty(categoryFilter) && categoryFilter != "All")
            {
                if (Enum.TryParse<ReportCategory>(categoryFilter, true, out var category))
                {
                    query = query.Where(r => r.Category == category);
                }
            }

            var reports = await query.ToListAsync();

            // Sort in memory: by Category name (alphabetically), then Priority, then Timestamp
            var sortedReports = reports
                .OrderBy(r => r.Category.ToString()) // Alphabetical: BuildingCollapse, Fire, Infrastructure, Logistics, Medical
                .ThenBy(r => r.Priority)
                .ThenByDescending(r => r.Timestamp)
                .ToList();

            ViewBag.CategoryFilter = categoryFilter;
            return View(sortedReports);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login");
            }

            var report = await _context.Reports
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);
                
            if (report == null) return NotFound();
            return View(report);
        }

        [HttpGet]
        public async Task<IActionResult> Archive()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login");
            }

            // Optimized: Use AsNoTracking for read-only operations
            var archives = await _context.ArchivedReports
                .OrderByDescending(r => r.ArchivedTimestamp)
                .AsNoTracking()
                .ToListAsync();
                
            return View(archives);
        }

        // =========================================================
        //  3. ACTIONS (DISPATCH, NOISE, UPDATE)
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DispatchReport(int id)
        {
            if (!IsAuthenticated())
            {
                return Unauthorized();
            }

            var report = await _context.Reports.FindAsync(id);
            if (report == null) return NotFound();

            // Move to Archive using optimized mapping
            var archive = new ArchivedReport
            {
                OriginalReportId = report.Id,
                SenderName = report.SenderName,
                ContactNumber = report.ContactNumber,
                RawMessage = report.RawMessage,
                Location = report.Location,
                Category = report.Category.ToString(),
                Priority = report.Priority.ToString(),
                ConfidenceScore = report.ConfidenceScore,
                OriginalTimestamp = report.Timestamp,
                Status = "Dispatched",
                ArchivedTimestamp = DateTime.UtcNow
            };

            _context.ArchivedReports.Add(archive);
            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Report dispatched successfully";
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsNoise(int id)
        {
            if (!IsAuthenticated())
            {
                return Unauthorized();
            }

            var report = await _context.Reports.FindAsync(id);
            if (report == null) return NotFound();

            var archive = new ArchivedReport
            {
                OriginalReportId = report.Id,
                SenderName = report.SenderName,
                ContactNumber = report.ContactNumber,
                RawMessage = report.RawMessage,
                Location = report.Location,
                Category = "Noise",
                Priority = "Low",
                ConfidenceScore = report.ConfidenceScore,
                OriginalTimestamp = report.Timestamp,
                Status = "Dismissed as Noise",
                ArchivedTimestamp = DateTime.UtcNow
            };

            _context.ArchivedReports.Add(archive);
            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Report marked as noise";
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            if (!IsAuthenticated())
            {
                return Unauthorized();
            }

            if (!Enum.TryParse<ReportStatus>(status, true, out var reportStatus))
            {
                TempData["Error"] = "Invalid status value";
                return RedirectToAction("Dashboard");
            }

            var report = await _context.Reports.FindAsync(id);
            if (report == null) return NotFound();

            report.Status = reportStatus;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Status updated successfully";
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCategory(int id, string category)
        {
            if (!IsAuthenticated())
            {
                return Unauthorized();
            }

            if (!Enum.TryParse<ReportCategory>(category, true, out var reportCategory))
            {
                TempData["Error"] = "Invalid category value";
                return RedirectToAction("Dashboard");
            }

            var report = await _context.Reports.FindAsync(id);
            if (report == null) return NotFound();

            report.Category = reportCategory;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Category updated successfully";
            return RedirectToAction("Dashboard");
        }

        // =========================================================
        //  4. AGENCY RELAY FEATURE
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> GetAgenciesForReport(int reportId)
        {
            try
            {
                if (!IsAuthenticated())
                {
                    return Json(new { error = "Unauthorized" });
                }

                var report = await _context.Reports
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == reportId);

                if (report == null)
                {
                    return Json(new { error = "Report not found" });
                }

                // Get all active agencies
                var categoryName = report.Category.ToString();
                var allAgencies = await _context.GovernmentAgencies
                    .Where(a => a.IsActive)
                    .OrderBy(a => a.Priority)
                    .ThenBy(a => a.Name)
                    .AsNoTracking()
                    .ToListAsync();

                // Filter agencies that handle this category
                var matchingAgencies = allAgencies
                    .Where(a => 
                    {
                        if (string.IsNullOrEmpty(a.HandlesCategories))
                            return false;

                        var categories = a.HandlesCategories.Split(',')
                            .Select(c => c.Trim())
                            .ToList();

                        // Check if agency handles this category or handles all
                        return categories.Contains(categoryName) || 
                               categories.Contains("All") ||
                               (a.Acronym == "OCD"); // OCD handles all emergencies
                    })
                    .ToList();

                // Map to JSON-friendly format
                var agencyData = matchingAgencies.Select(a => new
                {
                    id = a.Id,
                    name = a.Name,
                    acronym = a.Acronym,
                    description = a.Description ?? string.Empty,
                    emergencyHotline = a.EmergencyHotline ?? string.Empty,
                    email = a.Email ?? string.Empty,
                    address = a.Address ?? string.Empty,
                    priority = a.Priority
                }).ToList();

                return Json(new { agencies = agencyData, reportCategory = categoryName });
            }
            catch (Exception ex)
            {
                // Log error in production
                return Json(new { error = "Error loading agencies", message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RelayToAgency(int reportId, int agencyId, string notes = "")
        {
            if (!IsAuthenticated())
            {
                return Unauthorized();
            }

            var report = await _context.Reports.FindAsync(reportId);
            if (report == null) return NotFound();

            var agency = await _context.GovernmentAgencies
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == agencyId);
            if (agency == null) return NotFound();

            var username = HttpContext.Session.GetString("Username") ?? "admin";

            // Create relay record
            var relay = new AgencyRelay
            {
                ReportId = reportId,
                AgencyId = agencyId,
                RelayedBy = username,
                RelayedAt = DateTime.UtcNow,
                Notes = notes ?? string.Empty,
                Status = "Pending"
            };

            _context.AgencyRelays.Add(relay);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Report relayed to {agency.Name} ({agency.Acronym}) successfully";
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> GetRelayHistory(int reportId)
        {
            if (!IsAuthenticated())
            {
                return Unauthorized();
            }

            // Manual join since we don't have navigation properties configured yet
            var relaysWithAgency = await (from relay in _context.AgencyRelays
                                        join agency in _context.GovernmentAgencies on relay.AgencyId equals agency.Id
                                        where relay.ReportId == reportId
                                        select new
                                        {
                                            relay.Id,
                                            relay.RelayedAt,
                                            relay.Status,
                                            relay.Notes,
                                            relay.RelayedBy,
                                            AgencyName = agency.Name,
                                            AgencyAcronym = agency.Acronym
                                        })
                                        .OrderByDescending(r => r.RelayedAt)
                                        .ToListAsync();

            return Json(new { relays = relaysWithAgency });
        }

        // =========================================================
        //  HELPER METHODS
        // =========================================================

        private bool IsAuthenticated()
        {
            var username = HttpContext.Session.GetString("Username");
            return !string.IsNullOrEmpty(username);
        }

        private async Task SetAdminSession(string username, string role)
        {
            HttpContext.Session.SetString("Username", username);
            HttpContext.Session.SetString("Role", role);

            // Also set cookie authentication for consistency
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }
    }
}