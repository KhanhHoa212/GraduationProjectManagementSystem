using GPMS.Domain.Enums;
using System;
using System.Collections.Generic;

namespace GPMS.Application.DTOs;

public class UserDto
{
    public string UserID { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Username { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public UserStatus Status { get; set; } 
    public List<string> Roles { get; set; } = new();
}

public class CreateUserDto
{
    public string? UserID { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Role { get; set; } = "Student";
    public UserStatus Status { get; set; } = UserStatus.Active;
}

public class UpdateUserDto
{
    public string UserID { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Role { get; set; } = "Student";
    public UserStatus Status { get; set; }
}
