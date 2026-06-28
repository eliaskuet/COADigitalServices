using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using COADigitalServices.Data.Models;

namespace COADigitalServices.BLL.Seed
{
    public static class SeedRunner
    {
        // Applies migrations and runs seed data from SeedDefinitions.
        public static async Task RunSeedData(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Apply migrations (recommended) so DB schema is up to date.
            await db.Database.MigrateAsync();

            foreach (var s in SeedDefinitions.Users)
            {
                if (!await db.Users.AnyAsync(u => u.Username == s.Username))
                {
                    db.Users.Add(new User
                    {
                        Username = s.Username,
                        PasswordHash = ComputeSha256Hash(s.Password),
                        RoleId = s.RoleId,
                        FirstName = s.Username,
                        LastName = string.Empty,
                        EmailAddress = $"{s.Username}@example.com",
                        MobileNumber = string.Empty
                    });
                }
            }

            await db.SaveChangesAsync();
        }

        private static string ComputeSha256Hash(string rawData)
        {
            using var sha256Hash = SHA256.Create();
            var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            var builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
                builder.Append(bytes[i].ToString("x2"));
            return builder.ToString();
        }
    }
}
