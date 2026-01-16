using Microsoft.AspNetCore.Mvc;
using MarikinAlert.Web.Services;
using MarikinAlert.Web.Models;
using System.Threading.Tasks;
using System;
using System.Linq; 

namespace MarikinAlert.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDisasterTriageService _triageService;

        public HomeController(IDisasterTriageService triageService)
        {
            _triageService = triageService;
        }

        // GET: / (The Landing Page)
        public IActionResult Index()
        {
            // Explicitly renders "Create.cshtml"
            return View("Create");
        }

        // POST: /Home/Analyze
        [HttpPost]
        public async Task<IActionResult> Analyze(string rawMessage, string senderName, string contactNumber, string location)
        {
            if (string.IsNullOrWhiteSpace(rawMessage))
            {
                return View("Create");
            }

            try
            {
                // 1. Process the Report (The AI still does its job in the background!)
                var report = await _triageService.TriageAndAnalyzeAsync(
                    rawMessage: rawMessage,
                    name: senderName ?? "Anonymous",
                    location: location ?? "Unknown",
                    contact: contactNumber ?? "N/A"
                );

                // 2. DO NOT show the user the result. Just tell them it worked.
                ViewBag.Success = true;

                // 3. Clear the form so they can submit another one if needed
                ModelState.Clear();

                return View("Create");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "System overload. Please try again.";
                return View("Create");
            }
        }

        // GET: /Home/Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var reports = await _triageService.GetAllReportsAsync();
            
            // Filter: Show only "Real" emergencies on the dashboard
            var activeReports = reports
                .Where(r => r.Category != ReportCategory.Noise) 
                .OrderByDescending(r => r.Timestamp)
                .ToList();

            return View(activeReports);
        }

        // GET: /Home/Archive
        [HttpGet]
        public async Task<IActionResult> Archive()
        {
            var reports = await _triageService.GetAllReportsAsync();
            
            // Show everything sorted by newest
            return View(reports.OrderByDescending(r => r.Timestamp));
        }

        // Helper
        private string GetResponseTeam(string category)
        {
            return category switch
            {
                "Fire" => "BFP Marikina Station 1",
                "Medical" => "Amang Rodriguez Medic Team",
                "Police" => "PNP Marikina (Tactical)",
                "Infrastructure" => "City Engineering Office",
                "Logistics" => "DSWD Disaster Response",
                "Building Collapse" => "Search & Rescue Unit (SRU)",
                _ => "Barangay Tanod / Hotline 161"
            };
        }
    }
}