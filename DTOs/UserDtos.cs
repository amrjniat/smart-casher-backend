namespace POS.DTOs
{
    // ==================== طلبات (Requests) ====================

    public class CreateUserRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int RoleId { get; set; }
        public int? BranchId { get; set; }
    }

    public class UpdateUserRequest
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int? RoleId { get; set; }
        public int? BranchId { get; set; }
    }

    public class UpdateUserStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class ResetPasswordRequest
    {
        public string NewPassword { get; set; } = string.Empty;
    }

    // ==================== استجابات (Responses) ====================

    public class UserListItemDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public string? BranchName { get; set; }
        public int? BranchId { get; set; }
        public bool IsActive { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UserDetailDto : UserListItemDto
    {
        public List<string> Permissions { get; set; } = new();
    }

    public class UserStatsDto
    {
        public int TotalEmployees { get; set; }
        public int ActiveEmployees { get; set; }
        public int DisabledEmployees { get; set; }
        public List<RoleCountDto> RoleBreakdown { get; set; } = new();
    }

    public class RoleCountDto
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}