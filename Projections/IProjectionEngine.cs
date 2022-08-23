using Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Projections
{
    public interface IProjectionEngine
    {
        void RegisterProjection(IProjection projection);
        Task HandleChangesAsync(IReadOnlyCollection<Change> changes);
        event EventHandler<ChangesHandledEventArgs> OnChangesHandled;
    }
}