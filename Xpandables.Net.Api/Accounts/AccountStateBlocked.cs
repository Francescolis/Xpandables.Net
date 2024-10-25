﻿using Xpandables.Net.Api.Accounts.Events;

namespace Xpandables.Net.Api.Accounts;

public sealed class AccountStateBlocked : AccountState
{
    public override void UnBlock() =>
        Context.PushEvent(new AccountUnBlocked(Context));
}
