using GPMS.Application.Interfaces.Repositories;
using GPMS.Domain.Entities;
using GPMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.Infrastructure.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly GpmsDbContext _context;
    public RoomRepository(GpmsDbContext context) => _context = context;

    public async Task<IEnumerable<Room>> GetAllAsync() => await _context.Rooms.ToListAsync();
    public async Task<Room?> GetByIdAsync(int roomId) => await _context.Rooms.FindAsync(roomId);
}
