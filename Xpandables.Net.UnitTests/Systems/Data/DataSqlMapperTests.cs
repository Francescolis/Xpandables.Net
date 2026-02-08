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
using System.Data.Common;

using FluentAssertions;

namespace Xpandables.Net.UnitTests.Systems.Data;

public sealed class DataSqlMapperTests
{
    [Fact]
    public void MapToResult_WithIdentitySelector_ReturnsEntity()
    {
        // Arrange
        var mapper = new DataSqlMapper();
        var specification = DataSpecification.For<Person>().Build();
        using var reader = CreatePersonReader(12, "Ada");

        // Act
        var result = mapper.MapToResult(specification, reader);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(12);
        result.Name.Should().Be("Ada");
    }

    [Fact]
    public void MapToResult_WithPropertySelector_ReturnsScalar()
    {
        // Arrange
        var mapper = new DataSqlMapper();
        var specification = DataSpecification.For<Person>().Select(p => p.Name);
        using var reader = CreatePersonReader(3, "Lin");

        // Act
        var result = mapper.MapToResult(specification, reader);

        // Assert
        result.Should().Be("Lin");
    }

    [Fact]
    public void MapToResult_WithAnonymousProjection_ReturnsAnonymousType()
    {
        // Arrange
        var mapper = new DataSqlMapper();
        var specification = DataSpecification.For<Person>().Select(p => new { p.Id, p.Name });
        using var reader = CreatePersonReader(7, "Marie");

        // Act
        var result = mapper.MapToResult(specification, reader);

        // Assert
        result.Id.Should().Be(7);
        result.Name.Should().Be("Marie");
    }

    [Fact]
    public void MapToResult_WithCtorProjection_ReturnsDto()
    {
        // Arrange
        var mapper = new DataSqlMapper();
        var specification = DataSpecification.For<Person>().Select(p => new PersonDto(p.Id, p.Name));
        using var reader = CreatePersonReader(21, "Noor");

        // Act
        var result = mapper.MapToResult(specification, reader);

        // Assert
        result.Id.Should().Be(21);
        result.Name.Should().Be("Noor");
    }

    [Fact]
    public void MapToResult_WithMemberInitProjection_ReturnsDto()
    {
        // Arrange
        var mapper = new DataSqlMapper();
        var specification = DataSpecification.For<Person>().Select(p => new PersonInitDto { Id = p.Id, Name = p.Name });
        using var reader = CreatePersonReader(42, "Zoe");

        // Act
        var result = mapper.MapToResult(specification, reader);

        // Assert
        result.Id.Should().Be(42);
        result.Name.Should().Be("Zoe");
    }

    [Fact]
    public void Map_WithScalarValue_ReturnsValue()
    {
        // Arrange
        var mapper = new DataSqlMapper();
        using var reader = CreateScalarReader(99);

        // Act
        var result = mapper.MapToResult<int>(reader);

        // Assert
        result.Should().Be(99);
    }

    private static DbDataReader CreatePersonReader(int id, string name)
    {
        var table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Name", typeof(string));
        table.Rows.Add(id, name);

        var reader = table.CreateDataReader();
        reader.Read();
        return reader;
    }

    private static DbDataReader CreateScalarReader(int value)
    {
        var table = new DataTable();
        table.Columns.Add("Value", typeof(int));
        table.Rows.Add(value);

        var reader = table.CreateDataReader();
        reader.Read();
        return reader;
    }

    private sealed class Person
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    private sealed record PersonDto(int Id, string Name);

    private sealed class PersonInitDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}
