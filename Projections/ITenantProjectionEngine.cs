using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventStore;

namespace Projections;

public interface ITenantProjectionEngine
{
    void RegisterProjection(ITenantProjection projection);
    Task HandleChangesAsync(IReadOnlyCollection<Change> changes);
    event EventHandler<ChangesHandledEventArgs> OnChangesHandled;
}