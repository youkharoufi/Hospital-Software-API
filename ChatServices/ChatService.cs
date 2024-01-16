using Hospital_Software.Data;
using Hospital_Software.Models;
using Hospital_Software.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace Hospital_Software.ChatServices
{
    public class ChatService : IChatService
    {
        private readonly IMongoCollection<Message> _messagesCollection;
        private readonly MongoDbContext _context;

        public ChatService(IMongoDatabase database, MongoDbContext context)
        {
            _messagesCollection = database.GetCollection<Message>("Messages");
            _context = context;
        }

        public async Task<Message> SendMessageFromDoctorToPatient(string doctorId, string patientId, string content)
        {
                var message = new Message
                {
                    Id = Guid.NewGuid().ToString(),
                    SenderId = doctorId,
                    ReceivingId = patientId,
                    Time = DateTime.UtcNow, 
                    Content = content,
                    Read = false
                };
                await _messagesCollection.InsertOneAsync(message);

                return message;
        }

        public async Task<Message> SendMessageFromPatientToDoctor(string doctorId, string patientId, string content)
        {
            var message = new Message
            {
                Id= Guid.NewGuid().ToString(),
                SenderId = patientId,
                ReceivingId = doctorId,
                Time = DateTime.UtcNow,
                Content = content, 
                Read = false
            };
            await _messagesCollection.InsertOneAsync(message);

            return message;
        }

        public async Task<List<Message>> GetAllMessagesFromDoctor(string doctorId, string patientId)
        {
            var filter = Builders<Message>.Filter.Eq(msg => msg.SenderId, doctorId) &
                         Builders<Message>.Filter.Eq(msg => msg.ReceivingId, patientId);

            return await _messagesCollection.Find(filter)
                                            .Sort(Builders<Message>.Sort.Ascending(msg => msg.Time))
                                            .ToListAsync();
        }


        public async Task<List<Message>> GetAllMessagesFromPatient(string doctorId, string patientId)
        {
            var filter = Builders<Message>.Filter.Eq(msg => msg.SenderId, patientId) &
                         Builders<Message>.Filter.Eq(msg => msg.ReceivingId, doctorId);

            return await _messagesCollection.Find(filter)
                                            .Sort(Builders<Message>.Sort.Ascending(msg => msg.Time))
                                            .ToListAsync();
        }

        public async Task<List<Message>> OnPatientReadsMessages(string doctorId, string patientId)
        { 
            var filter = Builders<Message>.Filter.Eq(m => m.SenderId, patientId) & Builders<Message>.Filter.Eq(m => m.ReceivingId, doctorId) & Builders<Message>.Filter.Eq(m => m.Read, false);

            var unreadMessages = await _messagesCollection.Find(filter).ToListAsync();

            unreadMessages.ForEach((msg) => { msg.Read = true; });

            return unreadMessages;
        }

        public async Task<int> OnPatientReadMessagesCount(string patientId)
        {
            var filter = Builders<Message>.Filter.Eq(m => m.ReceivingId, patientId) & Builders<Message>.Filter.Eq(m => m.Read, false);

            var unreadMessages = await _messagesCollection.Find(filter).ToListAsync();

            return unreadMessages.Count();
        }

        public async Task<int> OnDoctorReadMessagesCount(string doctorId)
        {
            var filter = Builders<Message>.Filter.Eq(m => m.ReceivingId, doctorId) & Builders<Message>.Filter.Eq(m => m.Read, false);

            var unreadMessages = await _messagesCollection.Find(filter).ToListAsync();

            return unreadMessages.Count();
        }

        public async Task<List<Message>> OnDoctorReadsMessages(string doctorId, string patientId)
        {
            var filter = Builders<Message>.Filter.Eq(m => m.SenderId, doctorId) & Builders<Message>.Filter.Eq(m => m.ReceivingId, patientId) & Builders<Message>.Filter.Eq(m => m.Read, false);

            var unreadMessages = await _messagesCollection.Find(filter).ToListAsync();

            unreadMessages.ForEach((msg) => { msg.Read = true; });

            return unreadMessages;
        }


    }

}
