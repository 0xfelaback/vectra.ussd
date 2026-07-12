using Microsoft.EntityFrameworkCore;
using Vectra.Ussd.Application.Interfaces;

namespace Vectra.Ussd.Infrastructure.Repositories;

public class SimSwapReadRepository : IISimActionsRepository
{
    private readonly MockDbContext _context;

    public SimSwapReadRepository(MockDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CheckSwap(string phone, CancellationToken token) =>
        await _context.MockNibssSimSwapRecords
            .AsNoTracking()
            .AnyAsync(record => record.PhoneNumber == phone && record.IsSwapped, token);

    public async Task<bool> CheckReassigned(string phone, CancellationToken token) =>
        await _context.MockNibssSimReassignedRecords
            .AsNoTracking()
            .AnyAsync(record => record.PhoneNumber == phone && record.IsReassigned, token);
}