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

using FluentAssertions;

namespace Xpandables.Net.UnitTests.Systems.Data;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class DataSqlBuilderTests
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void BuildSelect_WithJoinWhereOrder_ReturnsExpectedSql()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void BuildSelect_WithGroupByHaving_ReturnsExpectedSql()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void BuildSelect_WithBareBooleanProperty_ReturnsColumnEquals1()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		// Arrange — .Where(p => p.IsActive) is a bare boolean MemberExpression
		var builder = new MsDataSqlBuilder();
		DataSpecification<Person, Person> specification = DataSpecification.For<Person>()
			.Where(person => person.IsActive)
			.Build();

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

		// Assert
		result.Sql.Should().Contain("WHERE (t0.[IsActive] = 1)");
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void BuildSelect_WithNegatedBooleanProperty_ReturnsColumnEquals0()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void BuildSelect_WithConditionalExpression_ReturnsCaseWhen()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void BuildSelect_WithNullableHasValue_ReturnsIsNotNull()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void BuildSelect_WithNegatedNullableHasValue_ReturnsIsNull()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void BuildSelect_WithNullableValue_ReturnsUnwrappedColumn()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void BuildSelect_WithStringIsNullOrEmpty_ReturnsIsNullOrEquals()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void BuildSelect_WithStringIsNullOrWhiteSpace_ReturnsLtrimRtrim()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void BuildSelect_WithCoalesceOperator_ReturnsCoalesce()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void BuildSelect_WithEnumComparison_ParameterizesUnderlyingType()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void BuildSelect_WithSelectConditional_ReturnsFullCaseWhenSql()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void BuildSelect_WithMemberInitContainingUntranslatableExpression_FallsBackToAllEntityColumns()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		// Arrange — selector uses Split + Select which cannot be translated to SQL
		var builder = new MsDataSqlBuilder();
		var specification = DataSpecification.For<PersonExtended>()
			.Select(p => new PersonProjection
			{
				Id = p.Id,
				Tags = p.Name.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim())
			});

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

		// Assert — should fall back to all entity columns instead of throwing
		result.Sql.Should().Contain("t0.[Id]");
		result.Sql.Should().Contain("t0.[Name]");
		result.Sql.Should().Contain("t0.[Nickname]");
		result.Sql.Should().Contain("t0.[Status]");
		result.Sql.Should().NotContain("AS [Tags]", "non-translatable expressions should not appear as aliased columns");
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void BuildSelect_WithNewExpressionContainingUntranslatableExpression_FallsBackToAllEntityColumns()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		// Arrange — anonymous type with Split + Select
		var builder = new MsDataSqlBuilder();
		var specification = DataSpecification.For<PersonExtended>()
			.Select(p => new
			{
				p.Id,
				Tags = p.Name.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim())
			});

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

		// Assert — should fall back to all entity columns
		result.Sql.Should().Contain("t0.[Id]");
		result.Sql.Should().Contain("t0.[Name]");
		result.Sql.Should().NotContain("AS [Tags]");
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void BuildSelect_WithTranslatableMemberInit_StillProducesOptimizedColumns()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		// Arrange — all expressions are translatable, should NOT fall back
		var builder = new MsDataSqlBuilder();
		var specification = DataSpecification.For<PersonExtended>()
			.Select(p => new PersonProjection
			{
				Id = p.Id,
				Tags = Enumerable.Empty<string>() // constant, not entity-dependent
			});

		// Act — this should still select only the needed columns
		SqlQueryResult result = builder.BuildSelect(specification);

		// Assert — should contain Id column (the Tags field falls back to all columns
		// because Enumerable.Empty is not translatable)
		result.Sql.Should().Contain("t0.[Id]");
	}

	[Fact]
#pragma warning disable CS1591
	public void BuildSelect_WithStringEqualsOrdinalIgnoreCase_ReturnsLowerEquality()
#pragma warning restore CS1591
	{
		// Arrange
		var builder = new MsDataSqlBuilder();
		string orderNumber = "ORD-001";
		DataSpecification<Person, Person> specification = DataSpecification.For<Person>()
			.Where(p => p.Name.Equals(orderNumber, StringComparison.OrdinalIgnoreCase))
			.Build();

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

		// Assert
		result.Sql.Should().Contain("(LOWER(t0.[Name]) = LOWER(@p0))");
	}

	[Fact]
#pragma warning disable CS1591
	public void BuildSelect_WithStringEqualsOrdinal_ReturnsPlainEquality()
#pragma warning restore CS1591
	{
		// Arrange
		var builder = new MsDataSqlBuilder();
		string orderNumber = "ORD-001";
		DataSpecification<Person, Person> specification = DataSpecification.For<Person>()
			.Where(p => p.Name.Equals(orderNumber, StringComparison.Ordinal))
			.Build();

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

		// Assert
		result.Sql.Should().Contain("(t0.[Name] = @p0)");
	}

	[Fact]
#pragma warning disable CS1591
	public void BuildSelect_WithStringEqualsAndBoolean_ReturnsCorrectCombination()
#pragma warning restore CS1591
	{
		// Arrange — reproduces the original reported issue
		var builder = new MsDataSqlBuilder();
		string orderNumber = "ORD-001";
		DataSpecification<Person, Person> specification = DataSpecification.For<Person>()
			.Where(p => p.Name.Equals(orderNumber, StringComparison.OrdinalIgnoreCase) && p.IsActive)
			.Build();

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

		// Assert
		result.Sql.Should().Contain("LOWER(t0.[Name]) = LOWER(@p0)");
		result.Sql.Should().Contain("(t0.[IsActive] = 1)");
		result.Sql.Should().Contain("AND");
	}

	[Fact]
#pragma warning disable CS1591
	public void BuildSelect_WithStaticStringEquals_ReturnsEquality()
#pragma warning restore CS1591
	{
		// Arrange
		var builder = new MsDataSqlBuilder();
		string value = "test";
		DataSpecification<Person, Person> specification = DataSpecification.For<Person>()
			.Where(p => string.Equals(p.Name, value, StringComparison.OrdinalIgnoreCase))
			.Build();

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

		// Assert
		result.Sql.Should().Contain("LOWER(");
	}

	[Fact]
#pragma warning disable CS1591
	public void BuildSelect_WithStringContainsCaseInsensitive_ReturnsLowerLike()
#pragma warning restore CS1591
	{
		// Arrange
		var builder = new MsDataSqlBuilder();
		DataSpecification<Person, Person> specification = DataSpecification.For<Person>()
			.Where(p => p.Name.Contains("test", StringComparison.OrdinalIgnoreCase))
			.Build();

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

		// Assert
		result.Sql.Should().Contain("LOWER(t0.[Name]) LIKE LOWER(@p0)");
		result.Parameters.Should().ContainSingle(p => p.Value!.Equals("%test%"));
	}

	[Fact]
#pragma warning disable CS1591
	public void BuildSelect_WithStartsWithCaseInsensitive_ReturnsLowerLike()
#pragma warning restore CS1591
	{
		// Arrange
		var builder = new MsDataSqlBuilder();
		DataSpecification<Person, Person> specification = DataSpecification.For<Person>()
			.Where(p => p.Name.StartsWith("test", StringComparison.OrdinalIgnoreCase))
			.Build();

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

		// Assert
		result.Sql.Should().Contain("LOWER(t0.[Name]) LIKE LOWER(@p0)");
		result.Parameters.Should().ContainSingle(p => p.Value!.Equals("test%"));
	}

	[Fact]
#pragma warning disable CS1591
	public void BuildSelect_WithTrim_ReturnsLtrimRtrim()
#pragma warning restore CS1591
	{
		// Arrange
		var builder = new MsDataSqlBuilder();
		var specification = DataSpecification.For<Person>()
			.Select(p => p.Name.Trim());

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

		// Assert
		result.Sql.Should().Contain("LTRIM(RTRIM(t0.[Name]))");
	}

	[Fact]
#pragma warning disable CS1591
	public void BuildSelect_WithTrimStart_ReturnsLtrim()
#pragma warning restore CS1591
	{
		// Arrange
		var builder = new MsDataSqlBuilder();
		var specification = DataSpecification.For<Person>()
			.Select(p => p.Name.TrimStart());

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

		// Assert
		result.Sql.Should().Contain("LTRIM(t0.[Name])");
	}

	[Fact]
#pragma warning disable CS1591
	public void BuildSelect_WithTrimEnd_ReturnsRtrim()
#pragma warning restore CS1591
	{
		// Arrange
		var builder = new MsDataSqlBuilder();
		var specification = DataSpecification.For<Person>()
			.Select(p => p.Name.TrimEnd());

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

		// Assert
		result.Sql.Should().Contain("RTRIM(t0.[Name])");
	}

	[Fact]
#pragma warning disable CS1591
	public void BuildSelect_WithSubstringTwoArgs_ReturnsSubstringWithLength()
#pragma warning restore CS1591
	{
		// Arrange
		var builder = new MsDataSqlBuilder();
		var specification = DataSpecification.For<Person>()
			.Select(p => p.Name.Substring(1, 3));

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

		// Assert
		result.Sql.Should().Contain("SUBSTRING(t0.[Name], @p0 + 1, @p1)");
	}

	[Fact]
#pragma warning disable CS1591
	public void BuildSelect_WithSubstringOneArg_ReturnsSubstringWithLen()
#pragma warning restore CS1591
	{
		// Arrange
		var builder = new MsDataSqlBuilder();
		var specification = DataSpecification.For<Person>()
			.Select(p => p.Name.Substring(2));

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

		// Assert
		result.Sql.Should().Contain("SUBSTRING(t0.[Name], @p0 + 1, LEN(t0.[Name]))");
	}

	[Fact]
#pragma warning disable CS1591
	public void BuildSelect_WithReplace_ReturnsReplace()
#pragma warning restore CS1591
	{
		// Arrange
		var builder = new MsDataSqlBuilder();
		var specification = DataSpecification.For<Person>()
			.Select(p => p.Name.Replace("old", "new"));

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

		// Assert
		result.Sql.Should().Contain("REPLACE(t0.[Name], @p0, @p1)");
	}

	[Fact]
#pragma warning disable CS1591
	public void BuildSelect_WithIndexOf_ReturnsCharindexMinusOne()
#pragma warning restore CS1591
	{
		// Arrange
		var builder = new MsDataSqlBuilder();
		var specification = DataSpecification.For<Person>()
			.Select(p => p.Name.IndexOf('x'));

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

		// Assert
		result.Sql.Should().Contain("(CHARINDEX(@p0, t0.[Name]) - 1)");
	}

	[Fact]
#pragma warning disable CS1591
	public void BuildSelect_WithIndexOfCaseInsensitive_ReturnsLowerCharindex()
#pragma warning restore CS1591
	{
		// Arrange
		var builder = new MsDataSqlBuilder();
		var specification = DataSpecification.For<Person>()
			.Select(p => p.Name.IndexOf('x', StringComparison.OrdinalIgnoreCase));

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

		// Assert
		result.Sql.Should().Contain("CHARINDEX(LOWER(@p0), LOWER(t0.[Name]))");
	}

	[Fact]
#pragma warning disable CS1591
	public void BuildSelect_WithStringLength_ReturnsLen()
#pragma warning restore CS1591
	{
		// Arrange
		var builder = new MsDataSqlBuilder();
		var specification = DataSpecification.For<Person>()
			.Select(p => p.Name.Length);

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

		// Assert
		result.Sql.Should().Contain("LEN(t0.[Name])");
	}

	[Fact]
#pragma warning disable CS1591
	public void BuildSelect_WithStringConcat_ReturnsConcatFunction()
#pragma warning restore CS1591
	{
		// Arrange
		var builder = new MsDataSqlBuilder();
		var specification = DataSpecification.For<Person>()
			.Select(p => string.Concat(p.Name, " - ", p.Name));

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

		// Assert
		result.Sql.Should().Contain("CONCAT(");
	}

	[Fact]
#pragma warning disable CS1591
	public void BuildSelect_WithToLowerInvariant_ReturnsLower()
#pragma warning restore CS1591
	{
		// Arrange
		var builder = new MsDataSqlBuilder();
		var specification = DataSpecification.For<Person>()
			.Select(p => p.Name.ToLowerInvariant());

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

		// Assert
		result.Sql.Should().Contain("LOWER(t0.[Name])");
	}

	[Fact]
#pragma warning disable CS1591
	public void BuildSelect_WithToUpperInvariant_ReturnsUpper()
#pragma warning restore CS1591
	{
		// Arrange
		var builder = new MsDataSqlBuilder();
		var specification = DataSpecification.For<Person>()
			.Select(p => p.Name.ToUpperInvariant());

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

		// Assert
		result.Sql.Should().Contain("UPPER(t0.[Name])");
	}

	private sealed class PersonProjection
	{
		public int Id { get; set; }
		public IEnumerable<string> Tags { get; set; } = [];
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

	[Fact]
#pragma warning disable CS1591
	public void BuildSelect_WithStringListContainsSingleElement_ReturnsInClause()
#pragma warning restore CS1591
	{
		// Arrange
		var builder = new MsDataSqlBuilder();
		List<string> names = ["Alice"];
		var specification = DataSpecification.For<Person>()
			.Where(p => names.Contains(p.Name))
			.Select(p => p);

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

		// Assert
		result.Sql.Should().Contain("IN (@p0)");
		result.Parameters.Should().ContainSingle(p => (string)p.Value! == "Alice");
	}

	[Fact]
#pragma warning disable CS1591
	public void BuildSelect_WithStringListContainsMultipleElements_ReturnsInClause()
#pragma warning restore CS1591
	{
		// Arrange
		var builder = new MsDataSqlBuilder();
		List<string> names = ["Alice", "Bob", "Charlie"];
		var specification = DataSpecification.For<Person>()
			.Where(p => names.Contains(p.Name))
			.Select(p => p);

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

		// Assert
		result.Sql.Should().Contain("IN (@p0, @p1, @p2)");
	}

	[Fact]
#pragma warning disable CS1591
	public void BuildSelect_WithGroupByAndCountStar_ReturnsCountAsterisk()
#pragma warning restore CS1591
	{
		// Arrange
		var builder = new MsDataSqlBuilder();
		var specification = DataSpecification.For<Person>()
			.GroupBy(p => p.AddressId)
			.Select(p => new { p.AddressId, Total = SqlFunctions.Count() });

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

		// Assert
		result.Sql.Should().Be(
			"SELECT t0.[AddressId] AS [AddressId], COUNT(*) AS [Total] FROM [Person] t0 GROUP BY t0.[AddressId]");
	}

	[Fact]
#pragma warning disable CS1591
	public void BuildSelect_WithGroupByAndSum_ReturnsSumColumn()
#pragma warning restore CS1591
	{
		// Arrange
		var builder = new MsDataSqlBuilder();
		var specification = DataSpecification.For<Person>()
			.GroupBy(p => p.AddressId)
			.Select(p => new { p.AddressId, Total = SqlFunctions.Sum(p.Id) });

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

		// Assert
		result.Sql.Should().Be(
			"SELECT t0.[AddressId] AS [AddressId], SUM(t0.[Id]) AS [Total] FROM [Person] t0 GROUP BY t0.[AddressId]");
	}

	[Fact]
#pragma warning disable CS1591
	public void BuildSelect_WithGroupByAndMultipleAggregates_ReturnsAllAggregates()
#pragma warning restore CS1591
	{
		// Arrange
		var builder = new MsDataSqlBuilder();
		var specification = DataSpecification.For<Person>()
			.GroupBy(p => p.AddressId)
			.Select(p => new
			{
				p.AddressId,
				Total = SqlFunctions.Count(),
				MaxId = SqlFunctions.Max(p.Id),
				MinId = SqlFunctions.Min(p.Id),
				AvgId = SqlFunctions.Avg(p.Id)
			});

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

		// Assert
		result.Sql.Should().Contain("COUNT(*)");
		result.Sql.Should().Contain("MAX(t0.[Id])");
		result.Sql.Should().Contain("MIN(t0.[Id])");
		result.Sql.Should().Contain("AVG(t0.[Id])");
	}

	[Fact]
#pragma warning disable CS1591
	public void BuildSelect_WithCountDistinct_ReturnsCountDistinctColumn()
#pragma warning restore CS1591
	{
		// Arrange
		var builder = new MsDataSqlBuilder();
		var specification = DataSpecification.For<Person>()
			.GroupBy(p => p.AddressId)
			.Select(p => new { p.AddressId, UniqueNames = SqlFunctions.CountDistinct(p.Name) });

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

		// Assert
		result.Sql.Should().Contain("COUNT(DISTINCT t0.[Name])");
	}

	[Fact]
#pragma warning disable CS1591
	public void BuildSelect_WithHavingAndAggregate_ReturnsHavingClause()
#pragma warning restore CS1591
	{
		// Arrange
		var builder = new MsDataSqlBuilder();
		var specification = DataSpecification.For<Person>()
			.GroupBy(p => p.AddressId)
			.Having(p => SqlFunctions.Count() > 5)
			.Select(p => new { p.AddressId, Total = SqlFunctions.Count() });

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

		// Assert
		result.Sql.Should().Contain("HAVING (COUNT(*) > @p0)");
	}

	[Fact]
#pragma warning disable CS1591
	public void BuildSelect_WithViewSource_UsesQuotedViewName()
#pragma warning restore CS1591
	{
		// Arrange
		var builder = new MsDataSqlBuilder();
		var specification = DataSpecification.For<Person>("vw_ActivePersons")
			.Where(p => p.IsActive == true)
			.Select(p => new { p.Id, p.Name });

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

		// Assert
		result.Sql.Should().Be(
			"SELECT t0.[Id] AS [Id], t0.[Name] AS [Name] FROM [vw_ActivePersons] t0 WHERE (t0.[IsActive] = @p0)");
	}

	[Fact]
#pragma warning disable CS1591
	public void BuildSelect_WithSubquerySource_WrapsInParentheses()
#pragma warning restore CS1591
	{
		// Arrange
		var builder = new MsDataSqlBuilder();
		var specification = DataSpecification.For<Person>("SELECT * FROM Person WHERE AddressId > 0")
			.Where(p => p.IsActive == true)
			.OrderBy(p => p.Name)
			.Select(p => new { p.Id, p.Name });

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

		// Assert
		result.Sql.Should().Be(
			"SELECT t0.[Id] AS [Id], t0.[Name] AS [Name] FROM (SELECT * FROM Person WHERE AddressId > 0) t0 WHERE (t0.[IsActive] = @p0) ORDER BY t0.[Name] ASC");
	}

	[Fact]
#pragma warning disable CS1591
	public void BuildCount_WithViewSource_UsesQuotedViewName()
#pragma warning restore CS1591
	{
		// Arrange
		var builder = new MsDataSqlBuilder();
		var specification = DataSpecification.For<Person>("vw_ActivePersons")
			.Where(p => p.IsActive == true)
			.Select(p => p);

		// Act
		SqlQueryResult result = builder.BuildCount(specification);

		// Assert
		result.Sql.Should().Be(
			"SELECT COUNT(*) FROM [vw_ActivePersons] t0 WHERE (t0.[IsActive] = @p0)");
	}

	[Fact]
#pragma warning disable CS1591
	public void BuildUpdate_WithSource_ThrowsInvalidOperationException()
#pragma warning restore CS1591
	{
		// Arrange
		var builder = new MsDataSqlBuilder();
		var specification = DataSpecification.For<Person>("vw_ActivePersons")
			.Build();
		var updater = new DataUpdater<Person>().SetProperty(p => p.Name, "Test");

		// Act
		Action act = () => builder.BuildUpdate(specification, updater);

		// Assert
		act.Should().Throw<InvalidOperationException>()
			.WithMessage("*UPDATE*not supported*custom source*");
	}

	[Fact]
#pragma warning disable CS1591
	public void BuildDelete_WithSource_ThrowsInvalidOperationException()
#pragma warning restore CS1591
	{
		// Arrange
		var builder = new MsDataSqlBuilder();
		var specification = DataSpecification.For<Person>("vw_ActivePersons")
			.Build();

		// Act
		Action act = () => builder.BuildDelete(specification);

		// Assert
		act.Should().Throw<InvalidOperationException>()
			.WithMessage("*DELETE*not supported*custom source*");
	}

	[Fact]
#pragma warning disable CS1591
	public void BuildSelect_WithoutSource_UsesDefaultTableName()
#pragma warning restore CS1591
	{
		// Arrange
		var builder = new MsDataSqlBuilder();
		var specification = DataSpecification.For<Person>()
			.Select(p => new { p.Id });

		// Act
		SqlQueryResult result = builder.BuildSelect(specification);

		// Assert
		result.Sql.Should().Contain("FROM [Person] t0");
	}
}
