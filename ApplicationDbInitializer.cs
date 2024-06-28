using JobWebApplicationvip.Models;
using Microsoft.AspNetCore.Identity;

namespace JobWebApplicationvip.Data
{
    public static class ApplicationDbInitializer
    {
        public static async Task Initialize(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            string[] roleNames = { ApplicationRole.AdminRole, ApplicationRole.EmployerRole, ApplicationRole.JobSeekerRole };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new ApplicationRole { Name = roleName });
                }
            }

            // Tạo tài khoản admin mặc định nếu chưa tồn tại
            var adminUser = await userManager.FindByEmailAsync("admin@example.com");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin@gmail.com",
                    Email = "admin@gmail.com",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(adminUser, "P@ssw0rd");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, ApplicationRole.AdminRole);
                }
            }
        }
    }
}