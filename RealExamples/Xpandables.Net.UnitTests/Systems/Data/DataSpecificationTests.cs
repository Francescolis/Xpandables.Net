/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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
using System.Data;
using System.Linq.Expressions;

using FluentAssertions;

namespace Xpandables.Net.UnitTests.Systems.Data;

public sealed class DataSpecificationTests
{
    [Fact]
    public void Where_WithPredicate_BuildsPredicate()
    {
		// Arrange
		DataSpecification<Person, Person> specification = DataSpecification.For<Person>()
            .Where(person => person.IsActive)
            .Build();

        // Act
        var predicate = (Expression<Func<Person, bool>>)specification.Predicate!;

        // Assert
        predicate.Compile()(new Person { IsActive = true }).Should().BeTrue();
    }

    [Fact]
    public void InnerJoin_WithJoin_BuildsInnerJoin()
    {
		// Arrange
		DataSpecification<Person, Person> specification = DataSpecification.For<Person>()
            .InnerJoin<Address>((person, address) => person.AddressId == address.Id)
            .Build();

		// Act
		IJoinSpecification join = specification.Joins.Single();

        // Assert
        join.JoinType.Should().Be(SqlJoinType.Inner);
    }

    [Fact]
    public void LeftJoin_WithJoin_BuildsLeftJoin()
    {
		// Arrange
		DataSpecification<Person, Person> specification = DataSpecification.For<Person>()
            .LeftJoin<Address>((person, address) => person.AddressId == address.Id)
            .Build();

		// Act
		IJoinSpecification join = specification.Joins.Single();

        // Assert
        join.JoinType.Should().Be(SqlJoinType.Left);
    }

    [Fact]
    public void RightJoin_WithJoin_BuildsRightJoin()
    {
		// Arrange
		DataSpecification<Person, Person> specification = DataSpecification.For<Person>()
            .RightJoin<Address>((person, address) => person.AddressId == address.Id)
            .Build();

		// Act
		IJoinSpecification join = specification.Joins.Single();

        // Assert
        join.JoinType.Should().Be(SqlJoinType.Right);
    }

    [Fact]
    public void FullJoin_WithJoin_BuildsFullJoin()
    {
		// Arrange
		DataSpecification<Person, Person> specification = DataSpecification.For<Person>()
            .FullJoin<Address>((person, address) => person.AddressId == address.Id)
            .Build();

		// Act
		IJoinSpecification join = specification.Joins.Single();

        // Assert
        join.JoinType.Should().Be(SqlJoinType.Full);
    }

    [Fact]
    public void CrossJoin_WithJoin_BuildsCrossJoin()
    {
		// Arrange
		DataSpecification<Person, Person> specification = DataSpecification.For<Person>()
            .CrossJoin<Address>()
            .Build();

		// Act
		IJoinSpecification join = specification.Joins.Single();

        // Assert
        join.JoinType.Should().Be(SqlJoinType.Cross);
    }

    [Fact]
    public void GroupBy_WithKeySelector_BuildsGroupBy()
    {
		// Arrange
		DataSpecification<Person, Person> specification = DataSpecification.For<Person>()
            .GroupBy(person => person.AddressId)
            .Build();

		// Act
		LambdaExpression groupBy = specification.GroupBy.Single();

        // Assert
        groupBy.Should().NotBeNull();
    }

    [Fact]
    public void Having_WithPredicate_BuildsHaving()
    {
		// Arrange
		DataSpecification<Person, Person> specification = DataSpecification.For<Person>()
            .GroupBy(person => person.AddressId)
            .Having(person => person.IsActive)
            .Build();

		// Act
		LambdaExpression? having = specification.Having;

        // Assert
        having.Should().NotBeNull();
    }

    [Fact]
    public void OrderBy_WithSelector_BuildsOrdering()
    {
		// Arrange
		DataSpecification<Person, Person> specification = DataSpecification.For<Person>()
            .OrderBy(person => person.Name)
            .Build();

		// Act
		OrderSpecification order = specification.OrderBy.Single();

        // Assert
        order.Descending.Should().BeFalse();
    }

    [Fact]
    public void OrderByDescending_WithSelector_BuildsDescendingOrdering()
    {
		// Arrange
		DataSpecification<Person, Person> specification = DataSpecification.For<Person>()
            .OrderByDescending(person => person.Name)
            .Build();

		// Act
		OrderSpecification order = specification.OrderBy.Single();

        // Assert
        order.Descending.Should().BeTrue();
    }

    [Fact]
    public void Select_WithJoinSelector_BuildsSelector()
    {
		// Arrange
		DataSpecification<Person, string> specification = DataSpecification.For<Person>()
            .InnerJoin<Address>((person, address) => person.AddressId == address.Id)
            .Select<Address, string>((person, address) => $"{person.Name}:{address.City}");

		// Act
		LambdaExpression selector = specification.Selector;

        // Assert
        selector.Should().NotBeNull();
    }

    private sealed class Person
    {
        public int Id { get; set; }

        public int AddressId { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }

    private sealed class Address
    {
        public int Id { get; set; }

        public string City { get; set; } = string.Empty;
    }
}
