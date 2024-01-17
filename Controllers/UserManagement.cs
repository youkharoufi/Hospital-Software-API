using Hospital_Software.Data;
using Hospital_Software.Models;
using Hospital_Software.Token;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Hospital_Software.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserManagement : ControllerBase
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _token;
        private readonly MongoDbContext _context;

        public UserManagement(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, SignInManager<ApplicationUser> signInManager,
            ITokenService token, MongoDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _token = token;
            _context = context;

        }


        [HttpPost("register-admin")]
        public async Task<ActionResult<ApplicationUser>> RegisterAdmin([FromForm] RegisterUser registerUser, [FromForm] IFormFile imageFile)
        {
            if (imageFile != null && !IsImageFile(imageFile))
            {
                return BadRequest("Invalid file type. Only image files are allowed.");
            }

            try
            {
                var imageUrl = await ProcessImageUploadAsync(imageFile);

                var newUser = new ApplicationUser
                {
                    UserName = registerUser.Firstname,
                    Firstname = registerUser.Firstname,
                    Lastname = registerUser.Lastname,
                    BadgeNumber = registerUser.BadgeNumber,
                    RoleName = "Admin",
                    PictureUrl = imageUrl

                };


                var result1 = await _userManager.CreateAsync(newUser, registerUser.Password);

                if (!result1.Succeeded)
                {
                    return BadRequest(result1.Errors.Select(e => e.Description));

                }

                var result2 = await _userManager.AddToRoleAsync(newUser, "Admin");

                if (!result2.Succeeded)
                {
                    return BadRequest(result2.Errors.Select(e => e.Description));

                }

                return Ok(newUser);


            }
            catch (Exception ex)
            {
                // Handle exceptions
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        private async static Task<List<Slot>> GenerateWeeklySlotsAsync(string doctorId)
        {
            var startDate = DateTime.Today; // Start from today
            var endDate = startDate.AddDays(7); // Generate slots for the next 7 days

            var slots = new List<Slot>();

            for (var date = startDate; date < endDate; date = date.AddDays(1))
            {
                for (var hour = 9; hour < 17; hour++) // From 9 AM to 5 PM
                {
                    var dateTime = new DateTime(date.Year, date.Month, date.Day, hour, 0, 0); // Create DateTime for each slot

                    var slot = new Slot
                    {
                        Id = Guid.NewGuid().ToString(), // Use GUID for unique Id
                        SlotTime = dateTime,
                        DoctorId = doctorId,
                        Booked = false
                    };

                    slots.Add(slot);
                }
            }

            // Return a list of slots, each representing a different time slot
            return slots;
        }



        private bool IsImageFile(IFormFile file)
        {
            // Check the file type and size here
            // For example, only allow JPEG and PNG files smaller than 5MB
            return file.Length > 0 &&
                   file.Length < 5 * 1024 * 1024 &&
                   (file.ContentType.ToLower() == "image/jpeg" ||
                    file.ContentType.ToLower() == "image/png");
        }

        private async Task<string> ProcessImageUploadAsync(IFormFile file)
        {
            // Process and store the image file
            // For example, save it in a directory on the server or upload it to cloud storage

            // Generate a unique file name to prevent overwriting existing files
            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", uniqueFileName);

            // Save the file
            using (var fileStream = new FileStream(uploadPath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Return the URL of the image
            // If using cloud storage, this URL should be the URL of the image in the cloud
            var imageUrl = $"https://localhost:7292/images/{uniqueFileName}";
            return imageUrl;
        }


        [HttpPost("register-doctor")]
        public async Task<ActionResult<ApplicationUser>> RegisterDoctor([FromForm] RegisterUser registerUser, [FromForm] IFormFile imageFile)
        {
            if (imageFile != null && !IsImageFile(imageFile))
            {
                return BadRequest("Invalid file type. Only image files are allowed.");
            }

            try
            {
                var imageUrl = await ProcessImageUploadAsync(imageFile);

                var newUser = new ApplicationUser
                {
                    UserName = registerUser.Firstname,
                    Firstname = registerUser.Firstname,
                    Lastname = registerUser.Lastname,
                    BadgeNumber = registerUser.BadgeNumber,
                    RoleName = "Doctor",
                    PictureUrl = imageUrl

                };


                var result1 = await _userManager.CreateAsync(newUser, registerUser.Password);

                if (!result1.Succeeded)
                {
                    return BadRequest(result1.Errors.Select(e => e.Description));

                }

                var result2 = await _userManager.AddToRoleAsync(newUser, "Doctor");

                if (!result2.Succeeded)
                {
                    return BadRequest(result2.Errors.Select(e => e.Description));

                }

                var slots = await GenerateWeeklySlotsAsync(newUser.Id);

                var slotsCollection = _context.GetCollection<Slot>("Slots");

                await slotsCollection.InsertManyAsync(slots);

                return Ok(newUser);


            }
            catch (Exception ex)
            {
                // Handle exceptions
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }


        [HttpPost("register-patient")]
        public async Task<ActionResult<ApplicationUser>> RegisterPatient([FromForm] RegisterUser registerUser)
        {
            if (registerUser.ImageFile != null && !IsImageFile(registerUser.ImageFile))
            {
                return BadRequest("Invalid file type. Only image files are allowed.");
            }

            try
            {
                var imageUrl = await ProcessImageUploadAsync(registerUser.ImageFile);

                var newUser = new ApplicationUser
                {
                    UserName = registerUser.Firstname,
                    Firstname = registerUser.Firstname,
                    Lastname = registerUser.Lastname,
                    BadgeNumber = registerUser.BadgeNumber,
                    RoleName = "Patient",
                    PictureUrl = imageUrl

                };


                var result1 = await _userManager.CreateAsync(newUser, registerUser.Password);

                if (!result1.Succeeded)
                {
                    return BadRequest(result1.Errors.Select(e => e.Description));

                }

                var result2 = await _userManager.AddToRoleAsync(newUser, "Patient");

                if (!result2.Succeeded)
                {
                    return BadRequest(result2.Errors.Select(e => e.Description));

                }


                return Ok(newUser);


            }
            catch (Exception ex)
            {
                // Handle exceptions
                return StatusCode(500, "An error occurred while processing your request.");
            }
        } 

        [HttpPost("login")]
        public async Task<ActionResult<ApplicationUser>> login(LoginUser loginUser)
        {
            var userFromDb = await _userManager.FindByNameAsync(loginUser.UserName);

            if (userFromDb == null) return NotFound("No such user is registered");

            var resultA = await _signInManager.CheckPasswordSignInAsync(userFromDb, loginUser.Password, false);

            if (!resultA.Succeeded) return BadRequest("Invalid Password");

            userFromDb.Token = await _token.GenerateToken(userFromDb);

            return Ok(userFromDb);
        }

        [HttpGet("all-doctors")]
        public async Task<List<ApplicationUser>> GetAllDoctors()
        {
            var allUsers = _context.GetCollection<ApplicationUser>("Users");

            var doctorRoleFilter = Builders<ApplicationUser>.Filter.Eq(user => user.RoleName, "Doctor");
            var doctors = await allUsers.Find(doctorRoleFilter).ToListAsync();

            return doctors;
        }

        [HttpGet("doctors-by-specialty/{spe}")]
        public async Task<List<ApplicationUser>> GetDoctorsBySpecialty(string spe)
        {
            var allUsers = _context.GetCollection<ApplicationUser>("Users");

            var doctorRoleFilter = Builders<ApplicationUser>.Filter.Eq(user => user.Speciality, spe);
            var doctors = await allUsers.Find(doctorRoleFilter).ToListAsync();

            return doctors;
        }

        [HttpGet("doctor-by-id/{doctorId}")]
        public async Task<ActionResult<ApplicationUser>> GetDoctorById(string doctorId)
        {
            var allUsers = _context.GetCollection<ApplicationUser>("Users");

            var doctorRoleFilter = Builders<ApplicationUser>.Filter.Eq(user => user.Id, doctorId);
            var doctor = await allUsers.Find(doctorRoleFilter).FirstOrDefaultAsync();

            if (doctor == null || doctor.RoleName != "Doctor")
            {
                return NotFound(); // Or handle the case appropriately
            }

            return doctor;
        }

        [HttpGet("all-patients")]
        public async Task<List<ApplicationUser>> GetAllPatients()
        {
            var allUsers = _context.GetCollection<ApplicationUser>("Users");

            var patientRoleFilter = Builders<ApplicationUser>.Filter.Eq(user => user.RoleName, "Patient");
            var patients = await allUsers.Find(patientRoleFilter).ToListAsync();

            return patients;
        }





    }
}
