using Core;
using EventStore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Projections
{
    public class ProjectionEngine : IProjectionEngine
    {
        private readonly List<IProjection> _projections = new List<IProjection>();
        private readonly IEventTypeResolver _eventTypeResolver;
        private readonly IViewRepository _viewRepository;

        public ProjectionEngine(IEventTypeResolver eventTypeResolver, IViewRepository viewRepository)
        {
            _eventTypeResolver = eventTypeResolver;
            _viewRepository = viewRepository;
        }
        public void RegisterProjection(IProjection projection)
        {
            _projections.Add(projection);
        }

        public async Task HandleChangesAsync(IReadOnlyCollection<Change> changes)
        {
            foreach (var change in changes)
            {
                var @event = change.GetEvent(_eventTypeResolver);

                if (@event == null) continue;

                var subscribedProjections = _projections
                    .Where(projection => projection.IsSubscribedTo(@event));

                foreach (var projection in subscribedProjections)
                {
                    var viewName = projection.GetViewName(change.StreamInfo.Id, @event);

                    var handled = false;
                    while (!handled)
                    {
                        var view = await _viewRepository.LoadViewAsync(viewName);

                        // Only update if the LSN of the change is higher than the view. This will ensure
                        // that changes are only processed once.
                        // NOTE: This only works if there's just a single physical partition in Cosmos DB.
                        // TODO: To support multiple partitions we need access to the leases to store
                        // a LSN per lease in the view. This is not yet possible in the V3 SDK.
                        if (view.IsNewerThanCheckpoint(change))
                        {
                            projection.Apply(@event, view);

                            view.UpdateCheckpoint(change);

                            handled = await _viewRepository.SaveViewAsync(viewName, view);
                        }
                        else
                        {
                            // Already handled.
                            handled = true;
                        }

                        if (!handled)
                            await Task.Delay(100);
                    }
                }
            }
        }
    }
}