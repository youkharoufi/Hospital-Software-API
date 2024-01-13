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
            Uri uri = new Uri("https://localhost:7292");
            if (userCount == 0) // Check if the user collection is empty
            {
                var users = new List<(ApplicationUser user, string password)>
        {
            (new ApplicationUser { UserName = "Youssef", Firstname = "Youssef", Lastname = "Kharoufi", BadgeNumber = 1234, RoleName="Admin", PictureUrl=$"{uri}images/Youssef.webp" }, "Password123!"),
            (new ApplicationUser { UserName = "Estelle", Firstname = "Estelle", Lastname = "DeLaCroix", BadgeNumber = 1597, RoleName = "Doctor", PictureUrl=$"{uri}images/Estelle1.jpg", Speciality="Cardiology" }, "Password123!"),
            (new ApplicationUser { UserName = "Armand", Firstname = "Armand", Lastname = "riskaa", BadgeNumber = 1897, RoleName = "Doctor", PictureUrl=$"{uri}images/riskaa.jpg", Speciality="Dental" }, "Password123!"),
            (new ApplicationUser { UserName = "Elie", Firstname = "Elie", Lastname = "Trojan", BadgeNumber = 1595, RoleName = "Doctor", PictureUrl=$"{uri}images/Elie.jpg", Speciality="General Medecine" }, "Password123!"),
            (new ApplicationUser { UserName = "Janine", Firstname = "Janine", Lastname = "Enzo", BadgeNumber = 1797, RoleName = "Doctor", PictureUrl=$"{uri}images/Janine.jpg", Speciality="Gynecology" }, "Password123!"),
            (new ApplicationUser { UserName = "Pierre", Firstname = "Pierre", Lastname = "Lescure", BadgeNumber = 3698, RoleName = "Doctor", PictureUrl=$"{uri}images/Pierre.jpg", Speciality="Gynecology" }, "Password123!"),
            (new ApplicationUser { UserName = "Soraya", Firstname = "Soraya", Lastname = "Tieks", BadgeNumber = 1478, RoleName = "Doctor", PictureUrl=$"{uri}images/Soraya.jpg", Speciality="Gynecology" }, "Password123!"),
            (new ApplicationUser { UserName = "Miloud", Firstname = "Miloud", Lastname = "Mattat", BadgeNumber = 2586, RoleName = "Doctor", PictureUrl=$"{uri}images/Miloud.jpg", Speciality="Cardiology" }, "Password123!"),
            (new ApplicationUser { UserName = "Jean", Firstname = "Jean", Lastname = "DeLaFontaine", BadgeNumber = 3467, RoleName = "Doctor", PictureUrl=$"{uri}images/Jean.jpg", Speciality="Traumatology" }, "Password123!"),
            (new ApplicationUser { UserName = "Kakashi", Firstname = "Kakashi", Lastname = "Hatake", BadgeNumber = 1673, RoleName = "Doctor", PictureUrl=$"{uri}images/Kakashi.jpg", Speciality="Traumatology" }, "Password123!"),
            (new ApplicationUser { UserName = "Annie", Firstname = "Annie", Lastname = "Leonhart", BadgeNumber = 8945, RoleName = "Doctor", PictureUrl=$"{uri}images/annie.jpg", Speciality="Pediatric" }, "Password123!"),
            (new ApplicationUser { UserName = "Mikasa", Firstname = "Mikasa", Lastname = "Ackerman", BadgeNumber = 8765, RoleName = "Doctor", PictureUrl=$"{uri}images/mikasa.jpg", Speciality="Pediatric" }, "Password123!"),
            (new ApplicationUser { UserName = "Armin", Firstname = "Armin", Lastname = "Van Burren", BadgeNumber = 1256, RoleName = "Doctor", PictureUrl=$"{uri}images/armin.jpg", Speciality="Dental" }, "Password123!"),
            (new ApplicationUser { UserName = "Rocklee", Firstname = "Rocklee", Lastname = "Lee", BadgeNumber = 3245, RoleName = "Doctor", PictureUrl=$"{uri}images/rocklee.png", Speciality="Neurology" }, "Password123!"),
            (new ApplicationUser { UserName = "Hinata", Firstname = "Hinata", Lastname = "Hyuga", BadgeNumber = 6541, RoleName = "Doctor", PictureUrl=$"{uri}images/hinata.png", Speciality="Neurology" }, "Password123!"),
            (new ApplicationUser { UserName = "Shino", Firstname = "Shino", Lastname = "Aburame", BadgeNumber = 3785, RoleName = "Doctor", PictureUrl=$"{uri}images/shino.png", Speciality="Neurology" }, "Password123!"),

            (new ApplicationUser { UserName = "Henry", Firstname = "Henry", Lastname = "Thierry", BadgeNumber = 1289, RoleName = "Patient", PictureUrl=$"{uri}images/Henry.webp"}, "Password123!")
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

        private async static Task<List<Slot>> GenerateWeeklySlotsAsync(string doctorId)
        {
            var startDate = DateTime.Today; // Start from today
            var endDate = startDate.AddDays(7); // Generate slots for the next 7 days

            var slotsForDoctor = new Slot
            {
                Id = Guid.NewGuid().ToString(), // Use GUID for unique Id
                DoctorId = doctorId,
                SlotTime = new List<DateTime>(),
                Booked = false
            };

            for (var date = startDate; date < endDate; date = date.AddDays(1))
            {
                for (var hour = 9; hour < 17; hour++) // From 9 AM to 5 PM
                {
                    var dateTime = new DateTime(date.Year, date.Month, date.Day, hour, 0, 0); // Create DateTime for each slot
                    slotsForDoctor.SlotTime.Add(dateTime);
                }
            }

            // Return a list containing a single Slot with multiple times
            return new List<Slot> { slotsForDoctor };
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
