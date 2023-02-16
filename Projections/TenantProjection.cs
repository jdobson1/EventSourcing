using System;
using Core.Domain;

namespace Projections;

public abstract class TenantProjection<T> : Projection<T>, ITenantProjection where T : new()
{
    public override string GetViewName(string streamId, IEvent @event) => GetTenantViewName(streamId);
    public static string GetTenantViewName(string streamId) => $"{streamId}-{nameof(T)}";
    public abstract Guid GetClientId(string streamId);
}