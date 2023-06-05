using System.Linq.Expressions;

namespace Sagas;

public class ToSagaExpression<TSagaState, TEvent> where TSagaState : class, ISagaState
{
    readonly Expression<Func<TEvent, object>> _eventProperty;
    readonly IConfigureSagaToEventCorrelation _configureSagaToEventCorrelation;
    
    public ToSagaExpression(IConfigureSagaToEventCorrelation configureSagaToEventCorrelation, Expression<Func<TEvent, object>> eventProperty)
    {
        this._configureSagaToEventCorrelation = configureSagaToEventCorrelation;
        this._eventProperty = eventProperty;
    }
    
    public void ToSaga(Expression<Func<TSagaState, object>> sagaEntityProperty)
    {
        _configureSagaToEventCorrelation.ConfigureCorrelation(sagaEntityProperty, _eventProperty);
    }
}