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

using FluentAssertions;

namespace Xpandables.Net.UnitTests.Systems.Data;

public sealed class DataSqlBuilderTests
{
    [Fact]
    public void BuildSelect_WithJoinWhereOrder_ReturnsExpectedSql()
    {
        // Arrange
        var builder = new MsDataSqlBuilder();
        var specification = DataSpecification.For<Person>()
            .InnerJoin<Address>((person, address) => person.AddressId == address.Id)
            .Where(person => person.IsActive == true)
            .OrderBy(person => person.Name)
            .Select((Person person, Address address) => new { person.Id, address.City });

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

        // Assert
        result.Sql.Should().Be(
            "SELECT t0.[Id] AS [Id], t1.[City] AS [City] FROM [Person] t0 INNER JOIN [Address] t1 ON (t0.[AddressId] = t1.[Id]) WHERE (t0.[IsActive] = @p0) ORDER BY t0.[Name] ASC");
    }

    [Fact]
    public void BuildSelect_WithGroupByHaving_ReturnsExpectedSql()
    {
        // Arrange
        var builder = new MsDataSqlBuilder();
        var specification = DataSpecification.For<Person>()
            .GroupBy(person => person.AddressId)
            .Having(person => person.AddressId > 0)
            .Select(person => new { person.AddressId });

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

        // Assert
        result.Sql.Should().Be(
            "SELECT t0.[AddressId] AS [AddressId] FROM [Person] t0 GROUP BY t0.[AddressId] HAVING (t0.[AddressId] > @p0)");
    }

    [Fact]
    public void BuildSelect_WithBareBooleanProperty_ReturnsColumnEquals1()
    {
        // Arrange — .Where(p => p.IsActive) is a bare boolean MemberExpression
        var builder = new MsDataSqlBuilder();
		DataSpecification<Person, Person> specification = DataSpecification.For<Person>()
            .Where(person => person.IsActive)
            .Build();

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

        // Assert
        result.Sql.Should().Contain("WHERE t0.[IsActive]");
    }

    [Fact]
    public void BuildSelect_WithNegatedBooleanProperty_ReturnsColumnEquals0()
    {
        // Arrange — .Where(p => !p.IsActive) generates Not(MemberAccess(bool))
        var builder = new MsDataSqlBuilder();
		DataSpecification<Person, Person> specification = DataSpecification.For<Person>()
            .Where(person => !person.IsActive)
            .Build();

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

        // Assert
        result.Sql.Should().Contain("WHERE (t0.[IsActive] = 0)");
    }

    [Fact]
    public void BuildSelect_WithConditionalExpression_ReturnsCaseWhen()
    {
        // Arrange — ternary operator: p.IsActive ? "Yes" : "No"
        var builder = new MsDataSqlBuilder();
		DataSpecification<Person, string> specification = DataSpecification.For<Person>()
            .Select(person => person.IsActive ? "Yes" : "No");

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

        // Assert
        result.Sql.Should().Contain("CASE WHEN");
        result.Sql.Should().Contain("THEN");
        result.Sql.Should().Contain("ELSE");
        result.Sql.Should().Contain("END");
    }

    [Fact]
    public void BuildSelect_WithNullableHasValue_ReturnsIsNotNull()
    {
        // Arrange — .Where(p => p.DeletedAt.HasValue)
        var builder = new MsDataSqlBuilder();
		DataSpecification<PersonExtended, PersonExtended> specification = DataSpecification.For<PersonExtended>()
            .Where(person => person.DeletedAt.HasValue)
            .Build();

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

        // Assert
        result.Sql.Should().Contain("IS NOT NULL");
    }

    [Fact]
    public void BuildSelect_WithNegatedNullableHasValue_ReturnsIsNull()
    {
        // Arrange — .Where(p => !p.DeletedAt.HasValue)
        var builder = new MsDataSqlBuilder();
		DataSpecification<PersonExtended, PersonExtended> specification = DataSpecification.For<PersonExtended>()
            .Where(person => !person.DeletedAt.HasValue)
            .Build();

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

        // Assert
        result.Sql.Should().Contain("IS NULL");
    }

    [Fact]
    public void BuildSelect_WithNullableValue_ReturnsUnwrappedColumn()
    {
        // Arrange — .Where(p => p.Score.Value > 10)
        var builder = new MsDataSqlBuilder();
		DataSpecification<PersonExtended, PersonExtended> specification = DataSpecification.For<PersonExtended>()
            .Where(person => person.Score!.Value > 10)
            .Build();

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

        // Assert
        result.Sql.Should().Contain("[Score]");
        result.Sql.Should().Contain("> @p0");
        result.Parameters.Should().ContainSingle(p => (int)p.Value! == 10);
    }

    [Fact]
    public void BuildSelect_WithStringIsNullOrEmpty_ReturnsIsNullOrEquals()
    {
        // Arrange — .Where(p => !string.IsNullOrEmpty(p.Name))
        var builder = new MsDataSqlBuilder();
		DataSpecification<Person, Person> specification = DataSpecification.For<Person>()
            .Where(person => !string.IsNullOrEmpty(person.Name))
            .Build();

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

        // Assert
        result.Sql.Should().Contain("IS NULL OR");
        result.Sql.Should().Contain("= ''");
    }

    [Fact]
    public void BuildSelect_WithStringIsNullOrWhiteSpace_ReturnsLtrimRtrim()
    {
        // Arrange — .Where(p => string.IsNullOrWhiteSpace(p.Name))
        var builder = new MsDataSqlBuilder();
		DataSpecification<Person, Person> specification = DataSpecification.For<Person>()
            .Where(person => string.IsNullOrWhiteSpace(person.Name))
            .Build();

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

        // Assert
        result.Sql.Should().Contain("LTRIM(RTRIM(");
    }

    [Fact]
    public void BuildSelect_WithCoalesceOperator_ReturnsCoalesce()
    {
        // Arrange — p.Nickname ?? p.Name
        var builder = new MsDataSqlBuilder();
		DataSpecification<PersonExtended, string> specification = DataSpecification.For<PersonExtended>()
            .Select(person => person.Nickname ?? person.Name);

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

        // Assert
        result.Sql.Should().Contain("COALESCE(");
    }

    [Fact]
    public void BuildSelect_WithEnumComparison_ParameterizesUnderlyingType()
    {
        // Arrange — .Where(p => p.Status == PersonStatus.Active)
        var builder = new MsDataSqlBuilder();
		DataSpecification<PersonExtended, PersonExtended> specification = DataSpecification.For<PersonExtended>()
            .Where(person => person.Status == PersonStatus.Active)
            .Build();

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

        // Assert — parameter should be the int value, not the enum
        result.Parameters.Should().ContainSingle(p => p.Value != null && p.Value.Equals((int)PersonStatus.Active));
    }

    [Fact]
    public void BuildSelect_WithSelectConditional_ReturnsFullCaseWhenSql()
    {
        // Arrange
        var builder = new MsDataSqlBuilder();
        var specification = DataSpecification.For<PersonExtended>()
            .Select(person => new { Label = person.Status == PersonStatus.Active ? "Active" : "Inactive" });

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

        // Assert
        result.Sql.Should().Contain("CASE WHEN (t0.[Status] =");
        result.Sql.Should().Contain("THEN");
        result.Sql.Should().Contain("ELSE");
        result.Sql.Should().Contain("END) AS [Label]");
    }

    private sealed class Person
    {
        public int Id { get; set; }

        public int AddressId { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }

    private sealed class PersonExtended
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Nickname { get; set; }

        public DateTime? DeletedAt { get; set; }

        public int? Score { get; set; }

        public PersonStatus Status { get; set; }

        public bool IsActive { get; set; }
    }

    private sealed class Address
    {
        public int Id { get; set; }

        public string City { get; set; } = string.Empty;
    }

    private enum PersonStatus
    {
        Inactive = 0,
        Active = 1
    }
}
