using System.Linq.Expressions;

namespace Sagas;

public class CorrelatedSagaPropertyMapper<TSagaState> where TSagaState : class, ISagaState
{
    private readonly SagaPropertyMapper<TSagaState> _mapper;
    private readonly Expression<Func<TSagaState, object>> _sagaExpression;
    internal CorrelatedSagaPropertyMapper(SagaPropertyMapper<TSagaState> mapper, Expression<Func<TSagaState, object>> sagaExpression)
    {
        _mapper = mapper;
        _sagaExpression = sagaExpression;
    }

    public CorrelatedSagaPropertyMapper<TSagaState> ToEvent<TEvent>(Expression<Func<TEvent, object>> eventProperty)
    {
        _mapper.ConfigureMapping(eventProperty).ToSaga(_sagaExpression);
        return this;
    }
}