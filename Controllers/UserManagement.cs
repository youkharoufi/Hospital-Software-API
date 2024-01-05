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
        public async Task<ActionResult<ApplicationUser>> RegisterAdmin(RegisterUser registerUser)
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

            return Ok(newUser);


        }

        [HttpPost("register-patient")]
        public async Task<ActionResult<ApplicationUser>> RegisterPatient(RegisterUser registerUser)
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


        [HttpGet("available-slots/{doctorId}")]
        public async Task<ActionResult<List<DateTime>>> availableSlots(string doctorId)
        {
            var doctorFromDb = await _userManager.FindByIdAsync(doctorId);

            var slotsCollection = _context.GetCollection<Slot>("Slots");

            var filter = Builders<Slot>.Filter.Eq(s => s.DoctorId, doctorId) & Builders<Slot>.Filter.Eq(s => s.Booked, false);
            var availableSlots = await slotsCollection.Find(filter).ToListAsync();

            return Ok(availableSlots);
        }


        [HttpPost("patient-books-slot/{patientId}/{doctorId}/{slotId}")]
        public async Task<ActionResult<Slot>> patientBooksSlot(string patientId, string doctorId, int slotId)
        {
            var patient = await _userManager.FindByIdAsync(patientId);
            var doc = await _userManager.FindByIdAsync(doctorId);

            var slotsCollection = _context.GetCollection<Slot>("Slots");


            var filter = Builders<Slot>.Filter.Eq(s => s.Id, slotId);
            var slot = await slotsCollection.Find(filter).FirstOrDefaultAsync();

            slot.Booked = true;
            slot.DoctorId = doc.Id;
            slot.patientId = patient.Id;


            return Ok(slot);


        }


    }
}
