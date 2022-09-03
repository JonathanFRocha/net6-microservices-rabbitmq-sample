using GeekShopping.IdentityServer.Configuration;
using GeekShopping.IdentityServer.Model;
using GeekShopping.IdentityServer.Model.Context;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace GeekShopping.IdentityServer.Initializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly MySQLContext _context;
        private readonly UserManager<ApplicationUser> _user;
        private readonly RoleManager<IdentityRole> _role;

        public DbInitializer(MySQLContext context, UserManager<ApplicationUser> user, RoleManager<IdentityRole> role)
        {
            _context = context;
            _user = user;
            _role = role;
        }

        public async Task Initialize()
        {
            if (_role.FindByNameAsync(IdentityConfiguration.Admin).Result != null) return;
            await _role.CreateAsync(new IdentityRole(IdentityConfiguration.Admin));
            await _role.CreateAsync(new IdentityRole(IdentityConfiguration.Client));

            ApplicationUser admin = new ApplicationUser()
            {
                UserName = "jonathan-admin",
                Email = "jonathan-admin@admin.com.br",
                EmailConfirmed = true,
                PhoneNumber = "+55 (21) 98888-4444",
                FirstName = "Jonathan",
                LastName = "Admin"
            };

            await _user.CreateAsync(admin, "1234Admin!");
            await _user.AddToRoleAsync(admin, IdentityConfiguration.Admin);
            var adminClaims = await _user.AddClaimsAsync(admin, new Claim[]
            {
                new Claim(JwtClaimTypes.Name, $"{admin.FirstName} {admin.LastName}"),
                new Claim(JwtClaimTypes.GivenName, admin.FirstName),
                new Claim(JwtClaimTypes.FamilyName, admin.LastName),
                new Claim(JwtClaimTypes.Role, IdentityConfiguration.Admin)
            });

            ApplicationUser client = new ApplicationUser()
            {
                UserName = "jonathan-client",
                Email = "jonathan-client@client.com.br",
                EmailConfirmed = true,
                PhoneNumber = "+55 (21) 98888-4444",
                FirstName = "Jonathan",
                LastName = "Client"
            };

            await _user.CreateAsync(client, "1234Admin!");
            await _user.AddToRoleAsync(client, IdentityConfiguration.Client);
            var clientClaims = await _user.AddClaimsAsync(client, new Claim[]
            {
                new Claim(JwtClaimTypes.Name, $"{client.FirstName} {client.LastName}"),
                new Claim(JwtClaimTypes.GivenName, client.FirstName),
                new Claim(JwtClaimTypes.FamilyName, client.LastName),
                new Claim(JwtClaimTypes.Role, IdentityConfiguration.Client)
            });
        }
    }
}
