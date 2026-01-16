namespace MarikinAlert.Web.Services
{
    using MarikinAlert.Web.Models;

    public interface IDisasterTriageService
    {
        Task<DisasterReport> TriageAndAnalyzeAsync(string rawMessage, string name, string location, string contact);
        Task<IEnumerable<DisasterReport>> GetAllReportsAsync();
        Task<DisasterReport?> GetReportByIdAsync(int id);  // ← Add ? here too
    }
}