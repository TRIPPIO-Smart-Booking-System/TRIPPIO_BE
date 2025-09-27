namespace Trippio.Core.Models.System
{
    public class RoleClaimsDto
    {
        public string? Type { get; set; }
        public string Value { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
    }

    public class PermissionDto
    {
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public List<RoleClaimsDto> RoleClaims { get; set; } = new();
    }
}
