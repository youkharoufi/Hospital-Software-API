using System.Linq;
using System.Threading;
using Hospital_Software.Data;
using Hospital_Software.Models;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;

namespace Hospital_Software.Data.MongoDbStores
{
    public class MyMongoUserStore : IUserStore<ApplicationUser>, IUserPasswordStore<ApplicationUser>, IUserRoleStore<ApplicationUser>
    {
        private readonly IMongoCollection<ApplicationUser> _usersCollection;

        public MyMongoUserStore(IMongoDatabase database)
        {
            _usersCollection = database.GetCollection<ApplicationUser>("Users");
        }

        public async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            await _usersCollection.InsertOneAsync(user, cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            await _usersCollection.DeleteOneAsync(u => u.Id == user.Id, cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<ApplicationUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            return await _usersCollection.Find(u => u.Id == userId).FirstOrDefaultAsync(cancellationToken);

        }

        public async Task<ApplicationUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return await _usersCollection.Find(u => u.NormalizedUserName == normalizedUserName).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            await _usersCollection.ReplaceOneAsync(u => u.Id == user.Id, user, cancellationToken: cancellationToken);
            return IdentityResult.Success;
        }

        public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id.ToString());
        }


        public Task<string> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }

        public Task SetUserNameAsync(ApplicationUser user, string userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.CompletedTask;
        }

        public Task<string> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedUserName);
        }

        public Task SetNormalizedUserNameAsync(ApplicationUser user, string normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetPasswordHashAsync(ApplicationUser user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        public Task<string> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash != null);

        }


        public async Task AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            var filter = Builders<ApplicationUser>.Filter.Eq(u => u.Id, user.Id);
            var update = Builders<ApplicationUser>.Update.Set(u => u.RoleName, roleName);
            await _usersCollection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        }

        public async Task RemoveFromRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            // Assuming that a user can only have one role and you want to remove it:
            var filter = Builders<ApplicationUser>.Filter.Eq(u => u.Id, user.Id);
            var update = Builders<ApplicationUser>.Update.Unset(u => u.RoleName);
            await _usersCollection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        }


        public async Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            var userFromDb = await _usersCollection.Find(u => u.Id == user.Id).FirstOrDefaultAsync(cancellationToken);
            return userFromDb != null && !string.IsNullOrEmpty(userFromDb.RoleName)
                ? new List<string> { userFromDb.RoleName }
                : new List<string>();
        }


        public async Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            var userFromDb = await _usersCollection.Find(u => u.Id == user.Id).FirstOrDefaultAsync(cancellationToken);
            return userFromDb?.RoleName == roleName;
        }


        public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            var filter = Builders<ApplicationUser>.Filter.Eq(u => u.RoleName, roleName);
            return await _usersCollection.Find(filter).ToListAsync(cancellationToken);
        }


        public void Dispose()
        {
            // Nothing to dispose in this example
        }

    }
}


