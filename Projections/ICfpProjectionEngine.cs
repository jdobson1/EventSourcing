using System.Threading.Tasks;

namespace Projections
{
    public interface ICfpProjectionEngine
    {
        void RegisterProjection(IProjection projection);

        Task StartAsync(string instanceName);

        Task StopAsync();
    }
}