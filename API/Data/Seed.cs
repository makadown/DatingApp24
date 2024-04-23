using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    /* Clase para implementar semilla que se ejecutar SOLAMENTE 1 vez */
    public class Seed
    { 
        public static async Task SeedUsers(UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager)
        {

            if (await userManager.Users.AnyAsync()) return;

            // Leo primero el archivo semilla
            var userData = System.IO.File.ReadAllText("Data/UserSeedData.json");
            // Serializo userData
            var users = JsonSerializer.Deserialize<List<AppUser>>(userData);
            if (users == null) return;

            var roles = new List<AppRole>
            {
                new AppRole{ Name = "Member"},
                new AppRole{ Name = "Admin"},
                new AppRole{ Name = "Moderator"}
            };

            foreach(var role in roles) {
                await roleManager.CreateAsync(role);
            }

            foreach ( var user in users )
            {
                user.UserName = user.UserName.ToLower();
                foreach(Photo foto in user.Photos)
                {
                    // Aprobando foto de forma predeterminada por ser semilla.
                    foto.IsApproved = true;
                }
                await userManager.CreateAsync(user, "password");
                await userManager.AddToRoleAsync(user, "Member");
            }

            var admin = new AppUser {
                UserName = "admin"
            };

            await userManager.CreateAsync(admin, "password");
            await userManager.AddToRolesAsync(admin, new[] {"Admin", "Moderator"});
        }
    }
}