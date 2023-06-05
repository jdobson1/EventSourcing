using System.Linq.Expressions;

namespace Sagas;

public interface IConfigureSagaToEventCorrelation
{
    void ConfigureCorrelation<TSagaState, TEvent>(Expression<Func<TSagaState, object>> sagaStateProperty,
        Expression<Func<TEvent, object>> eventProperty) where TSagaState : ISagaState;
}