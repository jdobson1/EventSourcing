using System.Linq.Expressions;
using System.Reflection;

namespace Sagas;

/// <summary>
/// Configures correlation for <see cref="Saga"/>.
/// </summary>
public class SagaCorrelator : IConfigureSagaToEventCorrelation
{
    public List<SagaLocatorConfiguration> Mappings = new();

    public void ConfigureCorrelation<TSagaState, TEvent>(Expression<Func<TSagaState, object>> sagaStateProperty, Expression<Func<TEvent, object>> eventProperty) where TSagaState : ISagaState
    {
        if (sagaStateProperty == null) throw new ArgumentNullException(nameof(sagaStateProperty));
        if (eventProperty == null) throw new ArgumentNullException(nameof(eventProperty));
        
        var sagaMember = GetMemberInfo(sagaStateProperty);
        var sagaProperty = sagaMember as PropertyInfo ?? throw new InvalidOperationException(
            $"Correlation expressions must correlate to properties. Change {sagaMember.Name} on {typeof(TSagaState).FullName} to a property.");
        
        //todo: validate mapping with eventExpression and sagaProperty

        if (sagaProperty == null) throw new Exception($"Saga state {sagaMember.Name} property cannot be null");
        
        var compiledEventExpression = eventProperty.Compile();
        var eventFunc = new Func<object, object>(x => compiledEventExpression((TEvent)x));
        var eventType = typeof(TEvent);

        Mappings.Add(new SagaLocatorConfiguration(eventType, eventType.Name, eventFunc,
            sagaProperty?.Name ?? string.Empty,
            sagaProperty?.PropertyType!));
    }

    private static MemberInfo GetMemberInfo(Expression member)
    {
        if (member == null)
        {
            throw new ArgumentNullException(nameof(member));
        }

        if (member is not LambdaExpression lambda)
        {
            throw new ArgumentException("Must be a lambda expression", nameof(member));
        }

        MemberExpression memberExpr = null;
        
        if (lambda.Body.NodeType == ExpressionType.Convert)
        {
            memberExpr = ((UnaryExpression)lambda.Body).Operand as MemberExpression;
        }

        return (memberExpr?.Member ?? null) ?? throw new Exception("Member expression is null");
    }
}