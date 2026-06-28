using System.Collections.Generic;

namespace COADigitalServices.BLL.Seed
{
    // Simple container for plain seed user data. No database logic here.
    // Now includes RoleId instead of Role string.
    public sealed record SeedUser(string Username, string Password, int RoleId);

    public static class SeedDefinitions
    {
        public static IEnumerable<SeedUser> Users => new[]
        {
            new SeedUser("admin", "password", 1),  // 1 = Admin role
            new SeedUser("user1", "userpass", 2),  // 2 = User role
            new SeedUser("manager", "managerpass", 1),  // 1 = Admin role
        };
    }
}
