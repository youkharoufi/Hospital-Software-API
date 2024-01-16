using Hospital_Software.Models;

namespace Hospital_Software.ChatServices
{
    public interface IChatService
    {
        Task<Message> SendMessageFromDoctorToPatient(string doctorId, string patientId, string content);
        Task<Message> SendMessageFromPatientToDoctor(string doctorId, string patientId, string content);
        Task<List<Message>> GetAllMessagesFromDoctor(string doctorId, string patientId);
        Task<List<Message>> GetAllMessagesFromPatient(string doctorId, string patientId);
        Task<List<Message>> OnPatientReadsMessages(string doctorId, string patientId);
        Task<int> OnPatientReadMessagesCount(string patientId);
        Task<int> OnDoctorReadMessagesCount(string doctorId);
        Task<List<Message>> OnDoctorReadsMessages(string doctorId, string patientId);


    }
}
