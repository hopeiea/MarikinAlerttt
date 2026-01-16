using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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
            if (User.Identity.IsAuthenticated) return RedirectToAction("Dashboard");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var adminUser = await _context.AdminUsers
                .FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == password);

            if (adminUser != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, adminUser.Username),
                    new Claim(ClaimTypes.Role, "Admin")
                };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                return RedirectToAction("Dashboard");
            }
            ViewBag.Error = "Invalid Credentials";
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // =========================================================
        //  2. SECURE PAGES
        // =========================================================

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var reports = await _triageService.GetAllReportsAsync();
            var cleanReports = reports.Where(r => r.Category != ReportCategory.Noise);

            // Sort: Critical(0) -> High(1) -> Medium(2) -> Low(3)
            // Note: Since we reordered the Enum, standard sorting works better now, 
            // but this logic ensures specific visual weight.
            var sortedReports = cleanReports
                .OrderBy(r => r.Priority) // Critical is 0, so it comes first
                .ThenByDescending(r => r.Timestamp)
                .ToList();

            return View(sortedReports);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var report = await _context.Reports.FirstOrDefaultAsync(r => r.Id == id);
            if (report == null) return NotFound();
            return View(report);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Archive()
        {
            // Fetch data from the ARCHIVE table, not the active reports
            var archives = await _context.ArchivedReports
                                         .OrderByDescending(r => r.ArchivedTimestamp)
                                         .ToListAsync();
            return View(archives);
        }

        // =========================================================
        //  3. ACTIONS (DISPATCH & NOISE)
        // =========================================================

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DispatchReport(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null) return NotFound();

            // Move to Archive
            var archive = new ArchivedReport
            {
                OriginalReportId = report.Id,
                SenderName = report.SenderName,
                ContactNumber = report.ContactNumber,
                RawMessage = report.RawMessage,
                Location = report.Location,
                Category = report.Category.ToString(),
                Priority = report.Priority.ToString(),
                ConfidenceScore = report.ConfidenceScore, // Keep the score!
                OriginalTimestamp = report.Timestamp,
                Status = "Dispatched",
                ArchivedTimestamp = DateTime.Now
            };

            _context.ArchivedReports.Add(archive);
            _context.Reports.Remove(report); // Remove from live feed
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> MarkAsNoise(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null) return NotFound();

            var archive = new ArchivedReport
            {
                OriginalReportId = report.Id,
                SenderName = report.SenderName,
                ContactNumber = report.ContactNumber,
                RawMessage = report.RawMessage,
                Location = report.Location,
                Category = "Noise", // Override category
                Priority = "Low",
                ConfidenceScore = report.ConfidenceScore,
                OriginalTimestamp = report.Timestamp,
                Status = "Dismissed as Noise",
                ArchivedTimestamp = DateTime.Now
            };

            _context.ArchivedReports.Add(archive);
            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard");
        }
    }
}