using Amazoom.Data.Static;
using Amazoom.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Amazoom.Data
{
    public class AppDbInitializer
    {
        public static void Seed(IApplicationBuilder applicationBuilder)
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<AppDbContext>();

                context.Database.EnsureCreated();




                //Items
                if (!context.Items.Any())
                {
                    context.Items.AddRange(new List<Item>()
                    {
                        new Item()
                        {
                            Name = "10*apples",
                            Description = "sweet",
                            Price = 1.5,
                            ImageURL = "http://dotnethow.net/images/Items/Item-3.jpeg",
                            weight = 1,
                            volume = 1,
                            stock = 1000,
 
                        
                        },
                        new Item()
                        {
                            Name = "iphone 99 pro max",
                            Description = "This is the future iphone",
                            Price = 999,
                            ImageURL = "http://dotnethow.net/images/Items/Item-1.jpeg",
                            weight = 1,
                            volume = 1,
                            stock = 1000,

                        },
                        new Item()
                        {
                            Name = "airpods pro",
                            Description = "good earphone",
                            Price = 250,
                            ImageURL = "http://dotnethow.net/images/Items/Item-4.jpeg",
                            weight = 0.5,
                            volume = 0.5,
                            stock = 1000,
                        },
                        new Item()
                        {
                            Name = "DE1-SOC",
                            Description = "EVEIL STUFF, CAN KILL PEOPLE",
                            Price = 300,
                            ImageURL = "http://dotnethow.net/images/Items/Item-6.jpeg",
                            weight = 0.8,
                            volume = 0.4,
                            stock = 1000,
                        },
                        new Item()
                        {
                            Name = "HP Prime",
                            Description = "The salt of the earth",
                            Price = 200,
                            ImageURL = "http://dotnethow.net/images/Items/Item-7.jpeg",
                            weight = 0.8,
                            volume = 0.4,
                            stock = 1000,
                        },
                        new Item()
                        {
                            Name = "breadboard*100",
                            Description = "lab kit",
                            Price = 50,
                            ImageURL = "http://dotnethow.net/images/Items/Item-8.jpeg",
                            weight = 10,
                            volume = 5,
                            stock = 1000,
                        }
                    });
                    context.SaveChanges();
                }

                
            }

        }

        public static async Task SeedUsersAndRolesAsync(IApplicationBuilder applicationBuilder)
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {

                //Roles
                var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                if (!await roleManager.RoleExistsAsync(UserRoles.Admin))
                    await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
                if (!await roleManager.RoleExistsAsync(UserRoles.User))
                    await roleManager.CreateAsync(new IdentityRole(UserRoles.User));

                //Users
                var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                string adminUserEmail = "admin@Amazoom.com";

                var adminUser = await userManager.FindByEmailAsync(adminUserEmail);
                if(adminUser == null)
                {
                    var newAdminUser = new ApplicationUser()
                    {
                        FullName = "Admin User",
                        UserName = "admin-user",
                        Email = adminUserEmail,
                        EmailConfirmed = true
                    };
                    await userManager.CreateAsync(newAdminUser, "Coding@1234?");
                    await userManager.AddToRoleAsync(newAdminUser, UserRoles.Admin);
                }


                string appUserEmail = "user@Amazoom.com";

                var appUser = await userManager.FindByEmailAsync(appUserEmail);
                if (appUser == null)
                {
                    var newAppUser = new ApplicationUser()
                    {
                        FullName = "Application User",
                        UserName = "app-user",
                        Email = appUserEmail,
                        EmailConfirmed = true
                    };
                    await userManager.CreateAsync(newAppUser, "Coding@1234?");
                    await userManager.AddToRoleAsync(newAppUser, UserRoles.User);
                }
            }
        }
    }
}
