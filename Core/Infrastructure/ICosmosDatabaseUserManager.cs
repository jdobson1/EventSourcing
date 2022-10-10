using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Core.Infrastructure
{
    public interface ICosmosDatabaseUserManager
    {
        Task<User> CreateUserIfNotExistsAsync(string userId, CosmosDatabaseContext context);
        Task<Permission> CreateUserPermissionIfNotExistsAsync(string userId, string permissionId, string resource, string partitionKey, CosmosDatabaseContext context);
    }
}
