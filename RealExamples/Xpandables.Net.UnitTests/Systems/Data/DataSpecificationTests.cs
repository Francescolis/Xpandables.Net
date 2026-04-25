/*******************************************************************************
 * Copyright (C) 2025-2026 Kamersoft
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

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class DataSpecificationTests
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void Where_WithPredicate_BuildsPredicate()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void InnerJoin_WithJoin_BuildsInnerJoin()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void LeftJoin_WithJoin_BuildsLeftJoin()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void RightJoin_WithJoin_BuildsRightJoin()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void FullJoin_WithJoin_BuildsFullJoin()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void CrossJoin_WithJoin_BuildsCrossJoin()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void GroupBy_WithKeySelector_BuildsGroupBy()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void Having_WithPredicate_BuildsHaving()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void OrderBy_WithSelector_BuildsOrdering()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void OrderByDescending_WithSelector_BuildsDescendingOrdering()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void Select_WithJoinSelector_BuildsSelector()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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
