namespace Sagas;

public class SagaStateException : Exception
{
    public Guid SagaId { get; }
    public Type Saga { get; }

    public SagaStateException(Guid sagaId, Type saga, string message) : base(message)
    {
        SagaId = sagaId;
        Saga = saga;
    }
}