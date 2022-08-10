using System.Threading.Tasks;

namespace Projections
{
    public interface IViewRepository
    {
        Task<View> LoadViewAsync(string name);
        Task<TView> LoadViewAsync<TView>(string name) where TView : new();

        Task<bool> SaveViewAsync(string name, View view);
    }
}