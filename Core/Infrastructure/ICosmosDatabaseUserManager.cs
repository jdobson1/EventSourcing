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
        /// <summary>
        /// Gets or Creates a Cosmos User
        /// </summary>
        /// <param name="userId">The Id of the user to create</param>
        /// <returns>An instance of <see cref="User"/></returns>
        Task<User> CreateUserIfNotExistsAsync(string userId);

        /// <summary>
        /// Deletes a Cosmos User
        /// </summary>
        /// <param name="userId">The Id of the user to delete</param>
        /// <returns>An instance of <see cref="UserResponse"/></returns>
        Task<UserResponse> DeleteUserAsync(string userId);

        /// <summary>
        /// Gets a Cosmos User.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<User> GetUserAsync(string userId);

        /// <summary>
        /// Gets or Creates a Cosmos User Permission if it does not already exist.
        /// </summary>
        /// <param name="userId">The Id of the user</param>
        /// <param name="permissionId">The associated permission Id</param>
        /// <param name="resource">OPTIONAL - The resource name</param>
        /// <param name="partitionKey">Partition Key of the resource - specifically the resource key. </param>
        /// <returns>An instance of <see cref="PermissionResponse"/></returns>
        Task<PermissionResponse> CreateUserPermissionIfNotExistsAsync(string userId, string permissionId, string? resource, string partitionKey);

        /// <summary>
        /// Deletes a Cosmos User Permission.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="permissionId"></param>
        /// <returns>An instance of <see cref="PermissionResponse"/></returns>
        Task<PermissionResponse> DeleteUserPermissionAsync(string userId, string permissionId);

        /// <summary>
        /// Retrieves the resource token for a given user's permission. If the cache entry for the token has expired, it will request and cache a new token.
        /// </summary>
        /// <param name="userId">The user Id</param>
        /// <param name="permissionId">The permission Id</param>
        /// <param name="streamId"></param>
        /// <returns>Cosmos resource token for the given user and permission </returns>
        Task<string> GetResourceTokenAsync(string userId, string permissionId, string streamId);

        /// <summary>
        /// Indicates if the user with the given Id exists in Cosmos.
        /// </summary>
        /// <param name="userId">The user Id</param>
        /// <returns>A <see cref="bool"/> indicating if the user exists or not. </returns>
        Task<bool> UserExists(string userId);

        /// <summary>
        /// Indicates if a resource token exists in the cache instance for the given user Id
        /// </summary>
        /// <param name="userId">The user Id</param>
        /// <returns>A <see cref="bool"/> indicating if there is a cached resource token for the given user. </returns>
        Task<bool> IsResourceTokenCached(string userId);
    }
}
