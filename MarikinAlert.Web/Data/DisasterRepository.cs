using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarikinAlert.Web.Models; // <--- Fixed Namespace

namespace MarikinAlert.Web.Data // <--- Fixed Namespace
{
    public class DisasterRepository : IDisasterRepository
    {
        private readonly AppDbContext _context;

        public DisasterRepository(AppDbContext context)
        {
            _context = context;
        }

        // Added the correct return type (Task<DisasterReport>) to match interface
        public async Task<DisasterReport> AddAsync(DisasterReport report)
        {
            await _context.Reports.AddAsync(report);
            await _context.SaveChangesAsync();
            return report; // <--- The interface expects the report back
        }

        public async Task<IEnumerable<DisasterReport>> GetAllAsync()
        {
            return await _context.Reports
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<DisasterReport?> GetByIdAsync(int id)
        {
            // Use FindAsync for tracking (needed for updates)
            return await _context.Reports.FindAsync(id);
        }

        public async Task<DisasterReport?> GetByIdAsyncNoTracking(int id)
        {
            return await _context.Reports
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);
        }
    }
}