using GPMS.Domain.Entities;
using GPMS.Domain.Enums;
using GPMS.Application.Interfaces.Repositories;
using GPMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPMS.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly GpmsDbContext _context;
    public UserRepository(GpmsDbContext context) => _context = context;

    public async Task<User?> GetByIdAsync(string userId) => await _context.Users.FindAsync(userId);
    public async Task<User?> GetByEmailAsync(string email) => await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    public async Task<IEnumerable<User>> GetAllAsync() => await _context.Users.ToListAsync();
    public async Task<IEnumerable<User>> GetByRoleAsync(RoleName role) => 
        await _context.Users.Where(u => u.UserRoles.Any(ur => ur.RoleName == role)).ToListAsync();
    public async Task AddAsync(User user) => await _context.Users.AddAsync(user);
    public async Task UpdateAsync(User user) => _context.Users.Update(user);
    public async Task<bool> ExistsAsync(string userId) => await _context.Users.AnyAsync(u => u.UserID == userId);
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
