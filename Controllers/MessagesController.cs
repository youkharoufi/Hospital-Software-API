using Hospital_Software.ChatServices;
using Hospital_Software.Models;
using Hospital_Software.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hospital_Software.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IChatService _chatService;

        public MessagesController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("message-from-doctor-to-patient/{doctorId}/{patientId}")]
        public async Task<ActionResult<Message>> SendMessageFromDoctorToPatient(string doctorId, string patientId, string content)
        {
            return Ok(await _chatService.SendMessageFromDoctorToPatient(doctorId, patientId, content));
        }

        [HttpPost("message-from-patient-to-doctor/{doctorId}/{patientId}")]
        public async Task<ActionResult<Message>> SendMessageFromPatientToDoctor(string doctorId, string patientId, [FromForm] string content)
        {
            return Ok(await _chatService.SendMessageFromPatientToDoctor(doctorId, patientId, content));
        }

        [HttpGet("get-all-patient-messages-from-specific-doctor/{doctorId}/{patientId}")]
        public async Task<ActionResult<List<Message>>> GetAllPatientMessagesFromSpecificDoctor(string doctorId, string patientId)
        {
            return Ok(await _chatService.GetAllMessagesFromDoctor(doctorId, patientId));
        }

        [HttpGet("get-all-doctor-messages-from-specific-patient/{doctorId}/{patientId}")]
        public async Task<ActionResult<List<Message>>> GetAllDoctorMessagesFromSpecificPatient(string doctorId, string patientId)
        {
            return Ok(await _chatService.GetAllMessagesFromPatient(doctorId, patientId));
        }

        [HttpPost("on-patient-reads-messages/{doctorId}/{patientId}")]
        public async Task<ActionResult<List<Message>>> OnPatientReadsMessages(string doctorId, string patientId)
        {
            return Ok(await _chatService.OnPatientReadsMessages(doctorId, patientId));
        }

        [HttpGet("on-patient-read-messages-count/{patientId}")]
        public async Task<ActionResult<int>> OnPatientReadMessagesCount(string patientId)
        {
            return Ok(await _chatService.OnPatientReadMessagesCount(patientId));
        }

        [HttpPost("on-doctor-reads-messages/{doctorId}/{patientId}")]
        public async Task<ActionResult<List<Message>>> OnDoctorReadsMessages(string doctorId, string patientId)
        {
            return Ok(await _chatService.OnDoctorReadsMessages(doctorId, patientId));
        }

        [HttpGet("on-doctor-read-messages-count/{doctorId}")]
        public async Task<ActionResult<int>> OnDoctorReadMessagesCount(string doctorId)
        {
            return Ok(await _chatService.OnDoctorReadMessagesCount(doctorId));
        }
    }
}
