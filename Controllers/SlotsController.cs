using Hospital_Software.Models;
using Hospital_Software.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace Hospital_Software.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SlotsController : ControllerBase
    {
        private readonly ISlotService _slotService;
        public SlotsController(ISlotService slotService)
        {
            _slotService = slotService; 
        }

        [HttpGet("available-slots/{doctorId}")]
        public async Task<ActionResult<List<Slot>>> availableSlots(string doctorId)
        {
            return Ok(await _slotService.GetAvailableSlotsAsync(doctorId));
        }


        [HttpPost("patient-books-slot/{patientId}/{doctorId}/{slotId}")]
        public async Task<ActionResult<Slot>> patientBooksSlot(string patientId, string doctorId, string slotId)
        {
            return Ok(await _slotService.PatientBooksSlot(patientId, doctorId, slotId));


        }

        [HttpGet("find-slot-by-id/{slotId}")]
        public async Task<ActionResult<List<Slot>>> FindSlotById(string slotId)
        {
            return Ok(await _slotService.getSlotById(slotId));
        }

        [HttpPost("generate-new-slots/{doctorId}")]
        public async Task<ActionResult<List<Slot>>> GenerateSlotsForDoctor(string doctorId)
        {
            return Ok(await _slotService.GenerateWeeklySlotsAsync(doctorId));
        }
    }
}
