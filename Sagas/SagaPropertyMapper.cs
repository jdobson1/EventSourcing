using System.Linq.Expressions;

namespace Sagas;

public class SagaPropertyMapper<TSagaState> where TSagaState : class, ISagaState
{
    private readonly IConfigureSagaToEventCorrelation _configureSagaToEventCorrelation;

    internal SagaPropertyMapper(IConfigureSagaToEventCorrelation configureSagaToEventCorrelation)
    {
        _configureSagaToEventCorrelation = configureSagaToEventCorrelation;
    }

    public ToSagaExpression<TSagaState, TEvent> ConfigureMapping<TEvent>(Expression<Func<TEvent, object>> eventProperty)
    {
        return new ToSagaExpression<TSagaState, TEvent>(_configureSagaToEventCorrelation, eventProperty);
    }

    public CorrelatedSagaPropertyMapper<TSagaState> MapSaga(Expression<Func<TSagaState, object>> sagaProperty)
    {
        return new CorrelatedSagaPropertyMapper<TSagaState>(this, sagaProperty);
    }
}