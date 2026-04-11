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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;

using FluentAssertions;

namespace Xpandables.Net.UnitTests.Systems.Data;

/// <summary>
/// Tests for Phase 1 (unified translation pipeline), Phase 2 (selector evaluation boundary),
/// Phase 3 (IDataSqlMetadata interface split), and Phase 4 (enhanced mapper).
/// </summary>
public sealed class DataEvaluationBoundaryTests
{
	// ─── Phase 2: SelectorEvaluation classification ───────────────────────

	[Fact]
	public void Select_IdentitySelector_ClassifiesAsServer()
	{
		DataSpecification<Product, Product> spec = DataSpecification.For<Product>().Build();

		spec.SelectorEvaluation.Should().Be(SelectorEvaluation.Server);
	}

	[Fact]
	public void Select_MemberAccess_ClassifiesAsServer()
	{
		DataSpecification<Product, string> spec = DataSpecification.For<Product>()
			.Select(p => p.Name);

		spec.SelectorEvaluation.Should().Be(SelectorEvaluation.Server);
	}

	[Fact]
	public void Select_NewExpression_ClassifiesAsServer()
	{
		var spec = DataSpecification.For<Product>()
			.Select(p => new { p.Id, p.Name });

		spec.SelectorEvaluation.Should().Be(SelectorEvaluation.Server);
	}

	[Fact]
	public void Select_CtorProjection_ClassifiesAsServer()
	{
		DataSpecification<Product, ProductDto> spec = DataSpecification.For<Product>()
			.Select(p => new ProductDto(p.Id, p.Name));

		spec.SelectorEvaluation.Should().Be(SelectorEvaluation.Server);
	}

	[Fact]
	public void Select_MemberInitProjection_ClassifiesAsServer()
	{
		DataSpecification<Product, ProductInitDto> spec = DataSpecification.For<Product>()
			.Select(p => new ProductInitDto { Id = p.Id, Name = p.Name });

		spec.SelectorEvaluation.Should().Be(SelectorEvaluation.Server);
	}

	[Fact]
	public void Select_ConditionalExpression_ClassifiesAsServer()
	{
		DataSpecification<Product, string> spec = DataSpecification.For<Product>()
			.Select(p => p.IsActive ? "Yes" : "No");

		spec.SelectorEvaluation.Should().Be(SelectorEvaluation.Server);
	}

	[Fact]
	public void Select_CoalesceExpression_ClassifiesAsServer()
	{
		DataSpecification<Product, string> spec = DataSpecification.For<Product>()
			.Select(p => p.Category ?? p.Name);

		spec.SelectorEvaluation.Should().Be(SelectorEvaluation.Server);
	}

	[Fact]
	public void Select_KnownStringMethod_ClassifiesAsServer()
	{
		DataSpecification<Product, string> spec = DataSpecification.For<Product>()
			.Select(p => p.Name.ToUpper());

		spec.SelectorEvaluation.Should().Be(SelectorEvaluation.Server);
	}

	[Fact]
	public void Select_InstanceMethodToString_ClassifiesAsClient()
	{
		DataSpecification<Product, string> spec = DataSpecification.For<Product>()
			.Select(p => p.ToString()!);

		spec.SelectorEvaluation.Should().Be(SelectorEvaluation.Client);
	}

	[Fact]
	public void Select_UnknownStringMethod_ClassifiesAsClient()
	{
		DataSpecification<Product, string> spec = DataSpecification.For<Product>()
			.Select(p => p.Name.Trim());

		spec.SelectorEvaluation.Should().Be(SelectorEvaluation.Client);
	}

	[Fact]
	public void Select_InstanceMethodOnEntity_ClassifiesAsClient()
	{
		DataSpecification<Product, string> spec = DataSpecification.For<Product>()
			.Select(p => p.GetDisplayName());

		spec.SelectorEvaluation.Should().Be(SelectorEvaluation.Client);
	}

	[Fact]
	public void Select_JoinProjection_ClassifiesAsServer()
	{
		var spec = DataSpecification.For<Product>()
			.InnerJoin<Category>((p, c) => p.CategoryId == c.Id)
			.Select((Product p, Category c) => new { p.Name, c.Label });

		spec.SelectorEvaluation.Should().Be(SelectorEvaluation.Server);
	}

	// ─── Phase 1: Unified pipeline — BuildUpdate / BuildDelete ───────────

	[Fact]
	public void BuildUpdate_WithPredicate_GeneratesParameterizedSql()
	{
		var builder = new MsDataSqlBuilder();
		DataSpecification<Product, Product> spec = DataSpecification.For<Product>()
			.Where(p => p.IsActive)
			.Build();

		var updater = DataUpdater.For<Product>()
			.SetProperty(p => p.Name, "Updated");

		SqlQueryResult result = builder.BuildUpdate(spec, updater);

		result.Sql.Should().Contain("UPDATE");
		result.Sql.Should().Contain("SET");
		result.Sql.Should().Contain("[Name]");
		result.Sql.Should().Contain("WHERE");
		result.Parameters.Should().Contain(p => p.Value != null && p.Value.Equals("Updated"));
	}

	[Fact]
	public void BuildDelete_WithPredicate_GeneratesParameterizedSql()
	{
		var builder = new MsDataSqlBuilder();
		DataSpecification<Product, Product> spec = DataSpecification.For<Product>()
			.Where(p => p.Id == 5)
			.Build();

		SqlQueryResult result = builder.BuildDelete(spec);

		result.Sql.Should().Contain("DELETE FROM");
		result.Sql.Should().Contain("WHERE");
		result.Parameters.Should().ContainSingle(p => (int)p.Value! == 5);
	}

	// ─── Phase 2: Client evaluation → all columns; Server → projected ────

	[Fact]
	public void BuildSelect_ClientEvaluation_SelectsAllColumns()
	{
		var builder = new MsDataSqlBuilder();
		DataSpecification<Product, string> spec = DataSpecification.For<Product>()
			.Select(p => p.GetDisplayName());

		spec.SelectorEvaluation.Should().Be(SelectorEvaluation.Client);

		SqlQueryResult result = builder.BuildSelect(spec);

		// Client mode should include all entity columns
		result.Sql.Should().Contain("[Id]");
		result.Sql.Should().Contain("[Name]");
		result.Sql.Should().Contain("[IsActive]");
		result.Sql.Should().Contain("[Category]");
		result.Sql.Should().Contain("[CategoryId]");
	}

	[Fact]
	public void BuildSelect_ServerEvaluation_SelectsOnlyProjectedColumns()
	{
		var builder = new MsDataSqlBuilder();
		var spec = DataSpecification.For<Product>()
			.Select(p => new { p.Id, p.Name });

		spec.SelectorEvaluation.Should().Be(SelectorEvaluation.Server);

		SqlQueryResult result = builder.BuildSelect(spec);

		result.Sql.Should().Contain("t0.[Id] AS [Id]");
		result.Sql.Should().Contain("t0.[Name] AS [Name]");
		result.Sql.Should().NotContain("[IsActive]");
		result.Sql.Should().NotContain("[Category]");
	}

	// ─── Phase 3: IDataSqlMetadata interface ─────────────────────────────

	[Fact]
	public void DataSqlBuilder_ImplementsIDataSqlMetadata()
	{
		IDataSqlMetadata metadata = new MsDataSqlBuilder();

		metadata.Dialect.Should().Be(SqlDialect.SqlServer);
		metadata.ParameterPrefix.Should().Be("@");
		metadata.QuoteIdentifier("test").Should().Be("[test]");
	}

	[Fact]
	public void IDataSqlBuilder_InheritsFromIDataSqlMetadata()
	{
		IDataSqlBuilder builder = new MsDataSqlBuilder();

		builder.Should().BeAssignableTo<IDataSqlMetadata>();
	}

	[Fact]
	public void GetColumnMappings_ReturnsPropertyToColumnMap()
	{
		IDataSqlMetadata metadata = new MsDataSqlBuilder();

		IReadOnlyDictionary<string, string> mappings = metadata.GetColumnMappings<Product>();

		mappings.Should().ContainKey("Id");
		mappings.Should().ContainKey("Name");
		mappings.Should().ContainKey("IsActive");
	}

	// ─── Phase 4: Enhanced mapper — server-mode direct mapping ───────────

	[Fact]
	public void MapToResult_ServerMode_CtorProjection_MapsDirectly()
	{
		var mapper = new DataSqlMapper();
		DataSpecification<Product, ProductDto> spec = DataSpecification.For<Product>()
			.Select(p => new ProductDto(p.Id, p.Name));

		spec.SelectorEvaluation.Should().Be(SelectorEvaluation.Server);

		using DbDataReader reader = CreateReader(
			["Id", "Name"],
			[42, "Widget"]);

		ProductDto result = mapper.MapToResult(spec, reader);

		result.Id.Should().Be(42);
		result.Name.Should().Be("Widget");
	}

	[Fact]
	public void MapToResult_ServerMode_AnonymousProjection_MapsDirectly()
	{
		var mapper = new DataSqlMapper();
		var spec = DataSpecification.For<Product>()
			.Select(p => new { p.Id, p.Name });

		spec.SelectorEvaluation.Should().Be(SelectorEvaluation.Server);

		using DbDataReader reader = CreateReader(
			["Id", "Name"],
			[7, "Gadget"]);

		var result = mapper.MapToResult(spec, reader);

		result.Id.Should().Be(7);
		result.Name.Should().Be("Gadget");
	}

	[Fact]
	public void MapToResult_ServerMode_ScalarMember_ReadsByColumnName()
	{
		var mapper = new DataSqlMapper();
		DataSpecification<Product, string> spec = DataSpecification.For<Product>()
			.Select(p => p.Name);

		spec.SelectorEvaluation.Should().Be(SelectorEvaluation.Server);

		// Reader has columns in non-trivial order — mapper finds Name by name
		using DbDataReader reader = CreateReader(
			["Id", "Name"],
			[3, "Bolt"]);

		string result = mapper.MapToResult(spec, reader);

		result.Should().Be("Bolt");
	}

	[Fact]
	public void MapToResult_ClientMode_AppliesSelector()
	{
		var mapper = new DataSqlMapper();
		DataSpecification<Product, string> spec = DataSpecification.For<Product>()
			.Select(p => p.GetDisplayName());

		spec.SelectorEvaluation.Should().Be(SelectorEvaluation.Client);

		using DbDataReader reader = CreateReader(
			["Id", "Name", "IsActive", "Category", "CategoryId"],
			[5, "Screw", true, "Hardware", 1]);

		string result = mapper.MapToResult(spec, reader);

		result.Should().Be("Screw (Hardware)");
	}

	// ─── Phase 4: ColumnAttribute-aware mapping ──────────────────────────

	[Fact]
	public void MapToResult_WithColumnAttribute_MapsFromDatabaseColumnName()
	{
		var mapper = new DataSqlMapper();

		using DbDataReader reader = CreateReader(
			["product_id", "product_name"],
			[99, "Bolt"]);

		MappedProduct result = mapper.MapToResult<MappedProduct>(reader);

		result.Id.Should().Be(99);
		result.Name.Should().Be("Bolt");
	}

	// ─── Nested MemberInit / New — must classify as Client ──────────────

	[Fact]
	public void Select_NestedMemberInit_ClassifiesAsClient()
	{
		var spec = DataSpecification.For<UserData>()
			.Select(u => new UserContext
			{
				Name = u.Name,
				Password = new EncryptedValue
				{
					Value = u.PasswordValue,
					Key = u.PasswordKey
				}
			});

		spec.SelectorEvaluation.Should().Be(SelectorEvaluation.Client);
	}

	[Fact]
	public void Select_NestedNewExpression_ClassifiesAsClient()
	{
		var spec = DataSpecification.For<UserData>()
			.Select(u => new { u.Name, Inner = new EncryptedValue { Value = u.PasswordValue } });

		spec.SelectorEvaluation.Should().Be(SelectorEvaluation.Client);
	}

	[Fact]
	public void Select_FlatMemberInit_ClassifiesAsServer()
	{
		// No nested objects — all bindings are simple member access → Server
		var spec = DataSpecification.For<UserData>()
			.Select(u => new UserContext { Name = u.Name });

		spec.SelectorEvaluation.Should().Be(SelectorEvaluation.Server);
	}

	[Fact]
	public void BuildSelect_NestedMemberInit_SelectsAllColumns()
	{
		var builder = new MsDataSqlBuilder();
		var spec = DataSpecification.For<UserData>()
			.Select(u => new UserContext
			{
				Name = u.Name,
				Password = new EncryptedValue
				{
					Value = u.PasswordValue,
					Key = u.PasswordKey
				}
			});

		// Should not throw — classified as Client, selects all columns
		SqlQueryResult result = builder.BuildSelect(spec);

		result.Sql.Should().Contain("[Name]");
		result.Sql.Should().Contain("[PasswordValue]");
		result.Sql.Should().Contain("[PasswordKey]");
	}

	[Fact]
	public void MapToResult_NestedMemberInit_ClientMode_ReconstructsObject()
	{
		var mapper = new DataSqlMapper();
		var spec = DataSpecification.For<UserData>()
			.Select(u => new UserContext
			{
				Name = u.Name,
				Password = new EncryptedValue
				{
					Value = u.PasswordValue,
					Key = u.PasswordKey
				}
			});

		spec.SelectorEvaluation.Should().Be(SelectorEvaluation.Client);

		using DbDataReader reader = CreateReader(
			["Id", "Name", "PasswordValue", "PasswordKey"],
			[1, "Alice", "enc123", "key456"]);

		UserContext result = mapper.MapToResult(spec, reader);

		result.Name.Should().Be("Alice");
		result.Password.Should().NotBeNull();
		result.Password!.Value.Should().Be("enc123");
		result.Password.Key.Should().Be("key456");
	}

	// ─── Multi-dialect tests ─────────────────────────────────────────────

	[Fact]
	public void PostgreBuilder_BuildDelete_GeneratesDialectSql()
	{
		var builder = new PostgreDataSqlBuilder();
		DataSpecification<Product, Product> spec = DataSpecification.For<Product>()
			.Where(p => p.Name == "test")
			.Build();

		SqlQueryResult result = builder.BuildDelete(spec);

		result.Sql.Should().Contain("DELETE FROM");
		result.Sql.Should().Contain("WHERE");
		result.Parameters.Should().ContainSingle(p => p.Value != null && p.Value.Equals("test"));
	}

	[Fact]
	public void MySqlBuilder_BuildUpdate_GeneratesDialectSql()
	{
		var builder = new MyDataSqlBuilder();
		DataSpecification<Product, Product> spec = DataSpecification.For<Product>()
			.Where(p => p.Id > 0)
			.Build();

		var updater = DataUpdater.For<Product>()
			.SetProperty(p => p.IsActive, false);

		SqlQueryResult result = builder.BuildUpdate(spec, updater);

		result.Sql.Should().Contain("UPDATE");
		result.Sql.Should().Contain("SET");
		result.Sql.Should().Contain("WHERE");
	}

	// ─── Helpers ─────────────────────────────────────────────────────────

	private static DbDataReader CreateReader(string[] columns, object[] values)
	{
		var table = new DataTable();
		for (int i = 0; i < columns.Length; i++)
		{
			table.Columns.Add(columns[i], values[i]?.GetType() ?? typeof(object));
		}
		table.Rows.Add(values);

		DataTableReader reader = table.CreateDataReader();
		reader.Read();
		return reader;
	}

	// ─── Test types ──────────────────────────────────────────────────────

	private sealed class Product
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public bool IsActive { get; set; }
		public string? Category { get; set; }
		public int CategoryId { get; set; }

		public string GetDisplayName() => $"{Name} ({Category})";
	}

	private sealed class Category
	{
		public int Id { get; set; }
		public string Label { get; set; } = string.Empty;
	}

	private sealed record ProductDto(int Id, string Name);

	private sealed class ProductInitDto
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
	}

	private sealed class MappedProduct
	{
		[Column("product_id")]
		public int Id { get; set; }

		[Column("product_name")]
		public string Name { get; set; } = string.Empty;
	}

	private sealed class UserData
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string PasswordValue { get; set; } = string.Empty;
		public string PasswordKey { get; set; } = string.Empty;
	}

	private sealed class UserContext
	{
		public string Name { get; set; } = string.Empty;
		public EncryptedValue? Password { get; set; }
	}

	private sealed class EncryptedValue
	{
		public string Value { get; set; } = string.Empty;
		public string Key { get; set; } = string.Empty;
	}
}
