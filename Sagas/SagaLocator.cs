namespace Sagas;

abstract class SagaLocator //SagaFinder
{
    public abstract Task<ISagaState> Locate(SagaLocatorConfiguration sagaLocatorConfiguration, object @event);
}