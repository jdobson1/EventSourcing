using Core;
using EventStore;
using System;
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

        public event EventHandler<ChangesHandledEventArgs> OnChangesHandled;

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
            //var updatedViews = new List<UpdatedView>();

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

                        if (view.IsNewerThanCheckpoint(change))
                        {
                            projection.Apply(@event, view);

                            view.UpdateCheckpoint(change);

                            handled = await _viewRepository.SaveViewAsync(viewName, view);

                            //updatedViews.Add(new UpdatedView { Name = viewName, Payload = view.Payload });
                            OnChangesHandled?.Invoke(this, new ChangesHandledEventArgs { View = new UpdatedView { Name = viewName, Payload = view.Payload } });
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