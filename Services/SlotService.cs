﻿using System.Numerics;
using Hospital_Software.Data;
using Hospital_Software.Models;
using Hospital_Software.Token;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Hospital_Software.Services
{
    public class SlotService : ISlotService
    {
        private readonly IMongoCollection<Slot> _slotsCollection;
        private readonly IMongoCollection<ApplicationUser> _userCollection;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly MongoDbContext _context;

        public SlotService(IMongoDatabase database, UserManager<ApplicationUser> userManager, MongoDbContext context)
        {
            _slotsCollection = database.GetCollection<Slot>("Slots");
            _userCollection = database.GetCollection<ApplicationUser>("user");
            _userManager = userManager;
            _context = context;
        }

        public async Task UpdateSlotsAsync()
        {
            // Remove past slots
            var yesterday = DateTime.Today.AddDays(-1);
            var outdatedFilter = Builders<Slot>.Filter.Lt(slot => slot.DateTime, yesterday);
            await _slotsCollection.DeleteManyAsync(outdatedFilter);

            // Generate new slots for a defined future period
            var newSlots = await GenerateFutureSlots(DateTime.Today, 14); // Correctly await the result of the async call
            await _slotsCollection.InsertManyAsync(newSlots); // Use the awaited result here
        }



        public async Task<IEnumerable<Slot>> GetAvailableSlotsAsync(string doctorId)
        {
            var today = DateTime.Today;
            var filter = Builders<Slot>.Filter.And(
                Builders<Slot>.Filter.Eq(slot => slot.DoctorId, doctorId),
                Builders<Slot>.Filter.Gte(slot => slot.DateTime, today), // Future slots
                Builders<Slot>.Filter.Eq(slot => slot.Booked, false) // Only unbooked slots
            );

            return await _slotsCollection.Find(filter).ToListAsync();
        }




        public async Task<List<string>> GetAllDoctorIds()
        {
            // Assuming that the role of a doctor is stored in the RoleName field of the ApplicationUser model
            var doctorRoleFilter = Builders<ApplicationUser>.Filter.Eq(user => user.RoleName, "Doctor");
            var doctors = await _userCollection.Find(doctorRoleFilter).ToListAsync();

            // Extracting and returning the IDs from the retrieved doctor records
            return doctors.Select(doctor => doctor.Id).ToList();
        }


        public async Task<IEnumerable<Slot>> GenerateFutureSlots(DateTime startDate, int daysToGenerate)
        {
            var slots = new List<Slot>();
            var endDate = startDate.AddDays(daysToGenerate);

            // Assuming GetAllDoctorIds is an existing method that returns a list of doctor IDs
            var doctorIds = await GetAllDoctorIds(); // This method needs to be synchronous or called with await in an async context

            foreach (var doctorId in doctorIds)
            {
                for (var date = startDate; date < endDate; date = date.AddDays(1))
                {
                    for (var hour = 9; hour < 17; hour++) // From 9 AM to 5 PM
                    {
                        var slotDateTime = new DateTime(date.Year, date.Month, date.Day, hour, 0, 0);
                        slots.Add(new Slot
                        {
                            DoctorId = doctorId,
                            DateTime = slotDateTime, // Combining date and time
                            Booked = false
                        });
                    }
                }
            }

            return slots;
        }

        public async Task<List<Slot>> GenerateWeeklySlotsAsync(string doctorId)
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

            return slots;

        }

        public async Task DeletePastSlotsAsync()
        {
            var yesterday = DateTime.Today.AddDays(-1);
            var filter = Builders<Slot>.Filter.Lt(slot => slot.DateTime, yesterday);
            await _slotsCollection.DeleteManyAsync(filter);
        }


        public async Task<List<Slot>> availableSlots(string doctorId)
        {
            var doctorFromDb = await _userManager.FindByIdAsync(doctorId);

            var slotsCollection = _context.GetCollection<Slot>("Slots");

            var filter = Builders<Slot>.Filter.Eq(s => s.DoctorId, doctorId) & Builders<Slot>.Filter.Eq(s => s.Booked, false);
            var availableSlots = await slotsCollection.Find(filter).ToListAsync();

            return availableSlots;
        }

        public async Task<Slot> PatientBooksSlot(string patientId, string doctorId, string slotId)
        {
            var patient = await _userManager.FindByIdAsync(patientId);
            var doc = await _userManager.FindByIdAsync(doctorId);

            var slotsCollection = _context.GetCollection<Slot>("Slots");

            var filter = Builders<Slot>.Filter.Eq(s => s.Id, slotId);
            var update = Builders<Slot>.Update
                .Set(s => s.Booked, true)
                .Set(s => s.DoctorId, doc.Id)
                .Set(s => s.patientId, patient.Id);

                var updateResult = await slotsCollection.UpdateOneAsync(filter, update);

                    // The update was successful, re-fetch the slot to get updated data
                    var updatedSlot = await slotsCollection.Find(filter).FirstOrDefaultAsync();
                    return updatedSlot;
               
            
        }

        public async Task<Slot> getSlotById(string slotId)
        {
            var slotsCollection = _context.GetCollection<Slot>("Slots");
            var filter = Builders<Slot>.Filter.Eq(s => s.Id, slotId);

            var slot = await slotsCollection.Find(filter).FirstOrDefaultAsync(); 

            return slot;

        }


    }
    
}
