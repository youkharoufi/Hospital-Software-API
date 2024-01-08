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

        private static async Task<List<Slot>> GenerateWeeklySlotsAsync(string doctorId)
        {
            var slots = new List<Slot>();
            var startDate = DateTime.Today; // Start from today
            var endDate = startDate.AddDays(7); // Generate slots for the next 7 days

            for (var date = startDate; date < endDate; date = date.AddDays(1))
            {
                for (var hour = 9; hour < 17; hour++) // From 9 AM to 5 PM
                {
                    var dateTime = new DateTime(date.Year, date.Month, date.Day, hour, 0, 0); // Create DateTime for each slot
                    slots.Add(new Slot
                    {
                        Id = Guid.NewGuid().ToString(),
                        DateTime = dateTime,
                        DoctorId = doctorId,
                        Booked = false
                    });
                }
            }

            // No asynchronous operation here, so we can return the result directly
            return await Task.FromResult(slots);
        }


        public static async Task SeedSlotsAsync(IMongoCollection<Slot> slotsCollection, string doctorId)
        {
            var slots = await GenerateWeeklySlotsAsync(doctorId);

            // Create a list to hold new slots that do not exist in the collection
            var newSlots = new List<Slot>();

            foreach (var slot in slots)
            {
                var existingSlot = await slotsCollection.Find(s => s.Id == slot.Id).FirstOrDefaultAsync();
                if (existingSlot == null)
                {
                    newSlots.Add(slot);
                }
            }

            if (newSlots.Any())
            {
                await slotsCollection.InsertManyAsync(newSlots);
            }
        }




    }
}
