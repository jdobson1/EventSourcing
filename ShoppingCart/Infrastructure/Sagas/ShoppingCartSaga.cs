using System;
using Core.Domain;
using Orders.Common.Events;
using Sagas;
using ShoppingCart.Common.Events;

namespace ShoppingCart.Infrastructure.Sagas
{
    public class ShoppingCartSaga : Saga<ShoppingCartCreated, ShoppingCartSagaState>
    {
        public ShoppingCartSaga(ISagaStateProvider stateProvider) : base(stateProvider)
        {
            RegisterHandler<ShoppingCartCreated>(WhenShoppingCartCreated);
            RegisterHandler<ItemAddedToShoppingCart>(WhenItemAddedToShoppingCart);
            RegisterHandler<OrderSubmitted>(WhenOrderSubmitted);
        }
        
        public override SagaCorrelation OnCorrelating(IEvent @event)
        {
            if (@event.GetType() == typeof(ShoppingCartCreated))
                return new SagaCorrelation(true, ((ShoppingCartCreated)@event).ShoppingCartId);

            if (@event.GetType() == typeof(OrderSubmitted))
                return new SagaCorrelation(true, ((OrderSubmitted)@event).ShoppingCartId);

            return @event.GetType() == typeof(ItemAddedToShoppingCart)
                ? new SagaCorrelation(true, ((ItemAddedToShoppingCart)@event).ShoppingCartId)
                : new SagaCorrelation(false);
        }

        private void WhenShoppingCartCreated(ShoppingCartCreated item)
        {
            State.DateCreated = DateTime.UtcNow;
        }

        private void WhenItemAddedToShoppingCart(ItemAddedToShoppingCart item)
        {
            State.NumberOfItemsInCart++;
            State.ApplyDiscount();
        }

        private async void WhenOrderSubmitted(OrderSubmitted obj)
        {
            //mark completed
            await CompleteAsync();
        }
    }

    public class ShoppingCartSagaState : SagaState
    {
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
