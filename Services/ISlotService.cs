using Hospital_Software.Models;

namespace Hospital_Software.Services
{
    public interface ISlotService
    {
        //Task UpdateSlotsAsync();
        Task<IEnumerable<Slot>> GetAvailableSlotsAsync(string doctorId);

        Task<List<string>> GetAllDoctorIds();

        //Task<IEnumerable<Slot>> GenerateFutureSlots(DateTime startDate, int daysToGenerate);

        Task<List<Slot>> GenerateWeeklySlotsAsync(string doctorId);

        Task DeletePastSlotsAsync();

        Task<Slot> PatientBooksSlot(string patientId, string doctorId, string slotId);

        Task<Slot> getSlotById(string slotId);
    }

}
