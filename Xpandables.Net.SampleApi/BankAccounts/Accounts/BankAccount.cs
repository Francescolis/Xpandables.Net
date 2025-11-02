using Xpandables.Net.Events.Aggregates;
using Xpandables.Net.SampleApi.EnumerationTypes;

namespace Xpandables.Net.SampleApi.BankAccounts.Accounts;

public sealed class BankAccount : AggregateStateContext<BankAccountState>, IAggregateFactory<BankAccount>
{
    public static BankAccount Initialize() => new();
    public static BankAccount Create(
        Guid accountId,
        string accountNumber,
        AccountType accountType,
        string owner,
        string email,
        decimal initialBalance)
    {
        var bankAccount = Initialize();
        var @event = new BankAccountCreatedEvent
        {
            StreamId = accountId,
            AccountNumber = accountNumber,
            AccountType = accountType,
            Owner = owner,
            Email = email,
            InitialBalance = initialBalance
        };

        bankAccount.PushEvent(@event);
        return bankAccount;
    }
    private BankAccount() : base(new BankAccountStatePending())
    {
        On<BankAccountCreatedEvent>(@event =>
        {
            StreamId = @event.StreamId;
            CurrentState.AccountNumber = @event.AccountNumber;
            CurrentState.Owner = @event.Owner;
            CurrentState.Email = @event.Email;
            CurrentState.Balance = @event.InitialBalance;
            CurrentState.AccountNumber = @event.AccountNumber;

            TransitionToState(new BankAccountStateActive());
        });

        On<MoneyDepositEvent>(@event =>
        {
            CurrentState.Balance += @event.Amount;
            CurrentState.AccountNumber = CurrentState.AccountNumber;
        });

        On<MoneyWithdrawEvent>(@event =>
        {
            CurrentState.Balance -= @event.Amount;
            CurrentState.AccountNumber = CurrentState.AccountNumber;
        });
    }

    protected override void OnStateTransitioning(BankAccountState? currentState, BankAccountState newState)
    {
        if (currentState is not null)
        {
            newState.AccountNumber = currentState.AccountNumber;
            newState.Balance = currentState.Balance;
            newState.Email = currentState.Email;
            newState.Owner = currentState.Owner;
        }
    }
}
