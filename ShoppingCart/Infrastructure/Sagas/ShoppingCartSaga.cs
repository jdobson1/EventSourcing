using System;
using System.Threading.Tasks;
using Orders.Common.Events;
using Sagas;
using Sagas.StateProviders;
using ShoppingCart.Common.Events;

namespace ShoppingCart.Infrastructure.Sagas
{
    public class ShoppingCartSaga : Saga<ShoppingCartSagaState>, IAmStartedByEvents<ShoppingCartCreated>, IHandleEvents<ItemAddedToShoppingCart>, IHandleEvents<OrderSubmitted>
    {
        public ShoppingCartSaga(ISagaStateProvider stateProvider) : base(stateProvider)
        {
        }

        public override void ConfigureSagaEventCorrelation(SagaPropertyMapper<ShoppingCartSagaState> sagaPropertyMapper)
        {
            sagaPropertyMapper.MapSaga(sagaState => sagaState.ShoppingCartId)
                .ToEvent<ShoppingCartCreated>(@event => @event.ShoppingCartId)
                .ToEvent<ItemAddedToShoppingCart>(@event => @event.ShoppingCartId)
                .ToEvent<OrderSubmitted>(@event => @event.ShoppingCartId);
        }

        public Task Handle(ItemAddedToShoppingCart @event)
        {
            State.NumberOfItemsInCart++;
            State.ApplyDiscount();
            return Task.CompletedTask;
        }

        public async Task Handle(OrderSubmitted @event)
        {
            //mark completed
            await CompleteAsync();
        }

        public Task Handle(ShoppingCartCreated @event)
        {
            State.DateCreated = @event.Timestamp;
            State.ShoppingCartId = @event.ShoppingCartId;
            
            return Task.CompletedTask;
        }
    }

    public class ShoppingCartSagaState : SagaState
    {
        public Guid ShoppingCartId { get; set; }
        public DateTime DateCreated { get; set; }
        public bool QualifiedForDiscount { get; set; }
        public int NumberOfItemsInCart { get; set; }

        public void ApplyDiscount()
        {
            if (NumberOfItemsInCart > 2)
            {
                QualifiedForDiscount = true;
                return;
            }

            QualifiedForDiscount = false;
        }
    }
}
