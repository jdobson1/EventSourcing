using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain;
using EventStore;

namespace Projections;

/// <summary>
/// A multi-tenant projection engine implementation.
/// </summary>
public class TenantProjectionEngine : ITenantProjectionEngine
{
    private readonly List<ITenantProjection> _projections = new List<ITenantProjection>();
    private readonly IEventTypeResolver _eventTypeResolver;
    private readonly ITenantViewRepository _viewRepository;

    public event EventHandler<ChangesHandledEventArgs> OnChangesHandled;

    public TenantProjectionEngine(IEventTypeResolver eventTypeResolver, ITenantViewRepository viewRepository)
    {
        _eventTypeResolver = eventTypeResolver;
        _viewRepository = viewRepository;
    }
    public void RegisterProjection(ITenantProjection projection)
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
                var clientId = projection.GetClientId(change.StreamInfo.Id);
                var handled = false;

                while (!handled)
                {
                    var view = await _viewRepository.LoadViewAsync(clientId, viewName);

                    if (view.IsNewerThanCheckpoint(change))
                    {
                        projection.Apply(@event, view);

                        view.UpdateCheckpoint(change);

                        handled = await _viewRepository.SaveViewAsync(clientId, viewName, view);

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