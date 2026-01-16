namespace MarikinAlert.Web.Data
{
    using MarikinAlert.Web.Models;

    public interface IDisasterRepository
    {
        Task<DisasterReport> AddAsync(DisasterReport report);
        Task<IEnumerable<DisasterReport>> GetAllAsync();
        Task<DisasterReport?> GetByIdAsync(int id);  // ← Add ? after DisasterReport
    }
}