using System;
using System.Threading.Tasks;

namespace Projections;

public interface ITenantViewRepository
{
    Task<View> LoadViewAsync(Guid clientId, string name);
    Task<TView> LoadViewAsync<TView>(Guid clientId, string name) where TView : new();

    Task<bool> SaveViewAsync(Guid clientId, string name, View view);
}