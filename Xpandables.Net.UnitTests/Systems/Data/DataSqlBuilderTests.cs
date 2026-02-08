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
        var result = builder.BuildSelect(specification);

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
        var result = builder.BuildSelect(specification);

        // Assert
        result.Sql.Should().Be(
            "SELECT t0.[AddressId] AS [AddressId] FROM [Person] t0 GROUP BY t0.[AddressId] HAVING (t0.[AddressId] > @p0)");
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
