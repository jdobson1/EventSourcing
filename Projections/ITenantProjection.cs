using System;

namespace Projections;

public interface ITenantProjection : IProjection
{
    Guid GetClientId(string streamId);
}