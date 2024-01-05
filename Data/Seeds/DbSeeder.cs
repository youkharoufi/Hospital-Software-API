using System.Xml;
using Hospital_Software.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Hospital_Software.Data.Seeds
{
    public static class DbSeeder
    {
        public static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager, MongoDbContext dbContext)
        {
            // Query MongoDB collection directly to count users
            var userCollection = dbContext.GetCollection<ApplicationUser>("Users");
            var userCount = await userCollection.CountDocumentsAsync(new BsonDocument());
            if (userCount == 0) // Check if the user collection is empty
            {
                var users = new List<(ApplicationUser user, string password)>
        {
            (new ApplicationUser { UserName = "Youssef", Firstname = "Youssef", Lastname = "Kharoufi", BadgeNumber = 1234, RoleName="Admin" }, "Password123!"),
            (new ApplicationUser { UserName = "Estelle", Firstname = "Estelle", Lastname = "DeLaCroix", BadgeNumber = 1597, RoleName = "Doctor" }, "Password123!"),
            (new ApplicationUser { UserName = "Henry", Firstname = "Henry", Lastname = "Thierry", BadgeNumber = 1289, RoleName = "Patient"}, "Password123!")
        };

                foreach (var (user, password) in users)
                {
                    if (await userManager.FindByNameAsync(user.UserName) == null)
                    {
                        await userManager.CreateAsync(user, password);

                        if(user.RoleName == "Doctor")
                        {
                            await userManager.AddToRoleAsync(user, "Doctor");
                        }
                    }
                }
            }
        }

        public static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
        {
            var roles = new List<string> { "Admin", "Doctor", "Patient" };

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new ApplicationRole { Name = role });
                    }
                }
            
        }

        public static List<Slot> GenerateWeeklySlots(string doctorId)
        {
            var slots = new List<Slot>();
            var startDate = DateOnly.FromDateTime(DateTime.Today);
            var endDate = startDate.AddDays(7);

            var startTime = new TimeOnly(9, 0); // Starting at 9 AM
            var endTime = new TimeOnly(17, 0);  // Ending at 5 PM

            int slotId = 1; // Starting ID, you might want to generate this differently

            for (var date = startDate; date < endDate; date = date.AddDays(1))
            {
                for (var time = startTime; time < endTime; time = time.AddHours(1))
                {
                    slots.Add(new Slot
                    {
                        Id = slotId++,
                        Date = date,
                        Time = time,
                        DoctorId = doctorId
                    });
                }
            }

            return slots;
        }

        public static async Task SeedSlotsAsync(IMongoCollection<Slot> slotsCollection, string doctorId)
        {
            // Example: Check if slots for this doctor already exist
            var existingSlotCount = await slotsCollection.CountDocumentsAsync(slot => slot.DoctorId == doctorId);
            if (existingSlotCount == 0)
            {
                var slots = GenerateWeeklySlots(doctorId);
                await slotsCollection.InsertManyAsync(slots);
            }
        }



    }
}
