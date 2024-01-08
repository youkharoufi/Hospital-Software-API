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
        public async Task<ActionResult<ApplicationUser>> RegisterAdmin([FromForm]RegisterUser registerUser)
        {
            var newUser = new ApplicationUser
            {   
                UserName = registerUser.Firstname,
                Firstname = registerUser.Firstname,
                Lastname = registerUser.Lastname,
                BadgeNumber = registerUser.BadgeNumber,
                RoleName = "Admin"

            };
            var result1 = await _userManager.CreateAsync(newUser, registerUser.Password);

            if (!result1.Succeeded)
            {
                return BadRequest(result1.Errors.Select(e => e.Description));

            }

            var result2 = await _userManager.AddToRoleAsync(newUser, "Admin");

            if(!result2.Succeeded)
            {
                return BadRequest(result2.Errors.Select(e => e.Description));

            }

            return Ok(newUser);


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
                        Id = Guid.NewGuid().ToString(), // Use GUID for unique Id
                        DateTime = dateTime,
                        DoctorId = doctorId,
                        Booked = false
                    });
                }
            }

            // No asynchronous operation here, so we can return the result directly
            return slots;
        }



        [HttpPost("register-doctor")]
        public async Task<ActionResult<ApplicationUser>> RegisterDoctor([FromForm]RegisterUser registerUser)
        {
            var newUser = new ApplicationUser
            {
                UserName = registerUser.Firstname,
                Firstname = registerUser.Firstname,
                Lastname = registerUser.Lastname,
                BadgeNumber = registerUser.BadgeNumber,
                RoleName = "Doctor"

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

        [HttpPost("register-patient")]
        public async Task<ActionResult<ApplicationUser>> RegisterPatient([FromForm]RegisterUser registerUser)
        {
            var newUser = new ApplicationUser
            {
                UserName = registerUser.Firstname,
                Firstname = registerUser.Firstname,
                Lastname = registerUser.Lastname,
                BadgeNumber = registerUser.BadgeNumber,
                RoleName = "Patient"

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

        [HttpPost("login")]
        public async Task<ActionResult<ApplicationUser>> login([FromForm]LoginUser loginUser)
        {
            var userFromDb = await _userManager.FindByNameAsync(loginUser.UserName);

            if (userFromDb == null) return NotFound("No such user is registered");

            var resultA = await _signInManager.CheckPasswordSignInAsync(userFromDb, loginUser.Password, false);

            if (!resultA.Succeeded) return BadRequest("Invalid Password");

            userFromDb.Token = await _token.GenerateToken(userFromDb);

            return Ok(userFromDb);
        }




    }
}
