/************************************************************************************************************
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
************************************************************************************************************/
using Xpandables.Net.Visitors;

namespace Xpandables.Net.UnitTests;

public sealed class VisitorUnitTest
{
    private static readonly Dictionary<IVisitor, IList<IVisitable>> Visitors = [];

    [Fact]
    public async Task AssertVisitIsValidAsync()
    {
        var elements = new Elements();
        elements.Attach(new ElementA());
        elements.Attach(new ElementB());

        var visitorA = new VisitorA();
        var visitorB = new VisitorB();
        var visitorC = new VisitorC();

        Visitors.Add(visitorA, new List<IVisitable>());
        Visitors.Add(visitorB, new List<IVisitable>());
        Visitors.Add(visitorC, new List<IVisitable>());

        await elements.Accept(visitorA);
        await elements.Accept(visitorB);
        await elements.Accept(visitorC);

        Assert.True(Visitors[visitorA].Count == 2);
        Assert.True(Visitors[visitorB].Count == 2);
        Assert.True(Visitors[visitorC].Count == 1);
    }

    sealed record class VisitorA : IVisitor
    {
        public ValueTask VisitAsync(object element)
        {
            if (element is IVisitable visitable)
                Visitors[this].Add(visitable);

            return ValueTask.CompletedTask;
        }
    }

    sealed record class VisitorB : IVisitor
    {
        public int Order => 1;
        public ValueTask VisitAsync(object element)
        {
            if (element is IVisitable visitable)
                Visitors[this].Add(visitable);

            return ValueTask.CompletedTask;
        }
    }

    sealed record class VisitorC : IVisitor<ElementA>
    {
        public ValueTask VisitAsync(ElementA element)
        {
            Visitors[this].Add(element);
            return ValueTask.CompletedTask;
        }
    }

    sealed record class ElementA : IVisitable { }
    sealed record class ElementB : IVisitable { }

    sealed record class Elements
    {
        private readonly List<IVisitable> elements = [];
        public void Attach(IVisitable element) => elements.Add(element);
        public void Detach(IVisitable element) => elements.Remove(element);

        public async ValueTask Accept(IVisitor visitor)
        {
            foreach (IVisitable element in elements)
                await element.AcceptAsync(visitor).ConfigureAwait(false);
        }
    }
}

