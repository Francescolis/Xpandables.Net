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
using FluentAssertions;

namespace Xpandables.Net.Visitors.UnitTests;
public sealed partial class VisitorUnitTest
{
    [Fact]
    public async void Given_NonGenericVisitor_VisitAllVisitables()
    {
        Dictionary<IVisitor, IList<IVisitable>> Visitors = [];

        var visitables = new Visitables();
        visitables.Attach(new VisitableA());
        visitables.Attach(new VisitableB());

        var visitorA = new VisitorA(Visitors);
        var visitorB = new VisitorB(Visitors);

        Visitors.Add(visitorA, new List<IVisitable>());
        Visitors.Add(visitorB, new List<IVisitable>());

        await visitables.Accept(visitorA);
        await visitables.Accept(visitorB);

        Visitors[visitorA].Should().HaveCount(2);
        Visitors[visitorB].Should().HaveCount(2);
    }

    [Fact]
    public async Task Given_GenericVisitor_VisitTypedVisitables()
    {
        Dictionary<IVisitor, IList<IVisitable>> Visitors = [];

        var visitables = new Visitables();
        visitables.Attach(new VisitableA());
        visitables.Attach(new VisitableB());

        var visitorA = new VisitorA(Visitors);
        var visitorB = new VisitorB(Visitors);
        var visitorC = new VisitorC(Visitors);

        Visitors.Add(visitorA, new List<IVisitable>());
        Visitors.Add(visitorB, new List<IVisitable>());
        Visitors.Add(visitorC, new List<IVisitable>());

        await visitables.Accept(visitorA);
        await visitables.Accept(visitorB);
        await visitables.Accept(visitorC);

        Visitors[visitorA].Should().HaveCount(2);
        Visitors[visitorB].Should().HaveCount(2);
        Visitors[visitorC].Should().HaveCount(1);
    }
}

