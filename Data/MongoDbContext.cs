using Hospital_Software.Data.Seeds;
using System.Xml;
using Hospital_Software.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.AspNetCore.Identity;

namespace Hospital_Software.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration["MongoDB:ConnectionString"]);
            _database = client.GetDatabase(configuration["MongoDB:DatabaseName"]);
        }


        // Example MongoDB collection properties
        public IMongoCollection<ApplicationUser> MyMongoDocuments => GetCollection<ApplicationUser>("Users");

        public IMongoCollection<Slot> Slots => GetCollection<Slot>("Slots");


        public IMongoCollection<T> GetCollection<T>(string name)
        {
            return _database.GetCollection<T>(name);
        }

        
    }
}
