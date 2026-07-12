using ReserveFlow.Domain.Shared;

namespace ReserveFlow.Booking.Tests;

public class DomainSharedTests
{
    [Fact]
    public void ValueObject_Equality_ShouldCompareComponents()
    {
        var left = new TestMoney(10, "TRY");
        var right = new TestMoney(10, "TRY");

        Assert.Equal(left, right);
    }

    [Fact]
    public void AggregateRoot_ShouldCollectDomainEvents()
    {
        var aggregate = new TestAggregate(Guid.NewGuid());
        var domainEvent = new TestDomainEvent();

        aggregate.DoSomething(domainEvent);

        Assert.Single(aggregate.GetDomainEvents());
        Assert.Same(domainEvent, aggregate.GetDomainEvents()[0]);
    }

    private sealed class TestMoney(decimal amount, string currency) : ValueObject
    {
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return amount;
            yield return currency;
        }
    }

    private sealed class TestAggregate(Guid id) : AggregateRoot(id)
    {
        public void DoSomething(IDomainEvent domainEvent) => RaiseDomainEvent(domainEvent);
    }

    private sealed class TestDomainEvent : IDomainEvent
    {
        public Guid Id { get; } = Guid.NewGuid();

        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }
}
