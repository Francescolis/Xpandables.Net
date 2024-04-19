
/*******************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
********************************************************************************/
using FluentAssertions;

namespace Xpandables.Net.UnitTests;

public sealed class NotifyPropertyChangedUnitTest
{
    [Theory]
    [InlineData("Name")]
    public void Assert_Changes_WereNotifiedTo_Listeners(string name)
    {
        List<string> receivedEvents = [];

        NotifierViewModel notifier = new();
        notifier.PropertyChanged += (s, e) => receivedEvents.Add(e.PropertyName!);

        notifier.Name = name;

        receivedEvents.Should().HaveCount(2);
        receivedEvents.Should().HaveElementAt(0, nameof(NotifierViewModel.Name));
        receivedEvents.Should().HaveElementAt(1, nameof(NotifierViewModel.MyName));
    }
}

internal sealed class NotifierViewModel : NotifyPropertyChanged
{

    private string _name = string.Empty;
    public string Name { get => _name; set => SetProperty(ref _name, value); }

    [NotifyPropertyChangedFor(nameof(Name))]
    public string MyName => Name;
}
