﻿using Hospital_Software.Models;

namespace Hospital_Software.Services
{
    public interface ISlotService
    {
        //Task UpdateSlotsAsync();
        Task<IEnumerable<Slot>> GetAvailableSlotsAsync(string doctorId);

        Task<List<ApplicationUser>> GetAllDoctors();

        Task DeleteAllSlotsForDoctorAsync(string doctorId);
        Task<List<Slot>> GenerateWeeklySlotsAsync(string doctorId);

        Task<Slot> PatientBooksSlot(string patientId, string doctorId, string slotId);

        Task<Slot> getSlotById(string slotId);
    }

}
