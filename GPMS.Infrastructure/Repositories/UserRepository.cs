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

    public async Task<User?> GetByIdAsync(string userId) => 
        await _context.Users.Include(u => u.UserRoles).Include(u => u.UserCredentials).FirstOrDefaultAsync(u => u.UserID == userId);

    public async Task<User?> GetByEmailAsync(string email) => 
        await _context.Users.Include(u => u.UserRoles).Include(u => u.UserCredentials).FirstOrDefaultAsync(u => u.Email == email);

    public async Task<User?> GetByUsernameOrEmailAsync(string identifier) =>
        await _context.Users
            .Include(u => u.UserRoles)
            .Include(u => u.UserCredentials)
            .FirstOrDefaultAsync(u => u.Email == identifier || u.Username == identifier);

    public async Task<IEnumerable<User>> GetAllAsync() => 
        await _context.Users.Include(u => u.UserRoles).ToListAsync();

    public async Task<IEnumerable<User>> GetByRoleAsync(RoleName role) => 
        await _context.Users
            .Include(u => u.UserRoles)
            .Include(u => u.LecturerExpertises)
                .ThenInclude(le => le.Expertise)
            .Include(u => u.LecturerExpertises)
                .ThenInclude(le => le.Expertise.Major)
            .Where(u => u.UserRoles.Any(ur => ur.RoleName == role))
            .ToListAsync();

    public async Task AddAsync(User user) => await _context.Users.AddAsync(user);
    public async Task UpdateAsync(User user) => _context.Users.Update(user);
    public async Task<bool> ExistsAsync(string userId) => await _context.Users.AnyAsync(u => u.UserID == userId);
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    public async Task<User?> GetUserByResetTokenAsync(string token)
    => await _context.Users
            .Include(u => u.UserCredentials)
            .FirstOrDefaultAsync(u => u.UserCredentials.Any(c =>
                c.PasswordResetToken == token &&
                c.PasswordResetExpiry.HasValue &&
                c.PasswordResetExpiry > DateTime.UtcNow));
    
}
