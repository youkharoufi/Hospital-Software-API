

using Hospital_Software.Models;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;

namespace Hospital_Software.Data.MongoDbStores
{
    public class MyMongoDbRoleStore : IRoleStore<ApplicationRole>
    {
        private readonly IMongoCollection<ApplicationRole> _rolesCollection;

        public MyMongoDbRoleStore(IMongoDatabase database)
        {
            _rolesCollection = database.GetCollection<ApplicationRole>("Roles");
        }

        public async Task<IdentityResult> CreateAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            await _rolesCollection.InsertOneAsync(role, cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            await _rolesCollection.DeleteOneAsync(r => r.Id == role.Id, cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<ApplicationRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            return await _rolesCollection.Find(r => r.Id == roleId).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<ApplicationRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            return await _rolesCollection.Find(r => r.NormalizedName == normalizedRoleName).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IdentityResult> UpdateAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            await _rolesCollection.ReplaceOneAsync(r => r.Id == role.Id, role, cancellationToken: cancellationToken);
            return IdentityResult.Success;
        }

        public Task<string> GetRoleIdAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Id.ToString());
        }

        public Task<string> GetRoleNameAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Name);
        }

        public Task SetRoleNameAsync(ApplicationRole role, string roleName, CancellationToken cancellationToken)
        {
            role.Name = roleName;
            return Task.CompletedTask;
        }

        public Task<string> GetNormalizedRoleNameAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.NormalizedName);
        }

        public Task SetNormalizedRoleNameAsync(ApplicationRole role, string normalizedName, CancellationToken cancellationToken)
        {
            role.NormalizedName = normalizedName;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            // Dispose any resources if necessary
        }
    }
}
