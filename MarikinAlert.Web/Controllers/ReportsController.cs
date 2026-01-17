using Microsoft.AspNetCore.Mvc;
using MarikinAlert.Web.Models;
using MarikinAlert.Web.Services;
using System.Threading.Tasks;

namespace MarikinAlert.Web.Controllers
{
    public class ReportsController : Controller
    {
        private readonly IDisasterTriageService _triageService;

        public ReportsController(IDisasterTriageService triageService)
        {
            _triageService = triageService;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // 1. Change the method signature to async
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // 2. Fetch the reports from your service (Just like the Dashboard did)
            var reports = await _triageService.GetAllReportsAsync();

            // 3. Pass the 'reports' list into the View so it isn't null
            return View(reports);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubmitReportViewModel model)
        {
            if (ModelState.IsValid)
            {
                // This sends the data to your database
                await _triageService.TriageAndAnalyzeAsync(
                    model.RawMessage,
                    model.SenderName,
                    model.ContactNumber,
                    model.Location
                );
                // Redirect to the new Success page
                return RedirectToAction("Success");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Archive()
        {
            // detailed logic can be added here later if we need to fetch specific archived data
            return View();
        }

        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }
    }
}