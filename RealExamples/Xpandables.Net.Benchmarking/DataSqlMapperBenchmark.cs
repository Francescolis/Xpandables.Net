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
using BenchmarkDotNet.Attributes;
using System.Data;
using System.Data.Common;
using DataTable = System.Data.DataTable;
using Microsoft.VSDiagnostics;

namespace Xpandables.Net.Benchmarking;
[CPUUsageDiagnoser]
public class DataSqlMapperBenchmark
{
    private DataSqlMapper _mapper = null!;
    private DataSpecification<PersonEntity, PersonEntity> _identitySpec;
    private DataSpecification<PersonEntity, PersonRecord> _ctorSpec;
    private DataSpecification<PersonEntity, PersonInitDto> _memberInitSpec;
    private DataSpecification<PersonEntity, string> _scalarSpec;
    private DataTable _table = null!;
    [GlobalSetup]
    public void Setup()
    {
        _mapper = new DataSqlMapper();
        _identitySpec = DataSpecification.For<PersonEntity>().Build();
        _ctorSpec = DataSpecification.For<PersonEntity>().Select(p => new PersonRecord(p.Id, p.Name));
        _memberInitSpec = DataSpecification.For<PersonEntity>().Select(p => new PersonInitDto { Id = p.Id, Name = p.Name });
        _scalarSpec = DataSpecification.For<PersonEntity>().Select(p => p.Name);
        _table = new DataTable();
        _table.Columns.Add("Id", typeof(int));
        _table.Columns.Add("Name", typeof(string));
        _table.Rows.Add(42, "BenchmarkUser");
    }

    [Benchmark]
    public PersonEntity MapToResult_Identity()
    {
        using var reader = _table.CreateDataReader();
        reader.Read();
        return _mapper.MapToResult(_identitySpec, reader);
    }

    [Benchmark]
    public PersonRecord MapToResult_CtorProjection()
    {
        using var reader = _table.CreateDataReader();
        reader.Read();
        return _mapper.MapToResult(_ctorSpec, reader);
    }

    [Benchmark]
    public PersonInitDto MapToResult_MemberInit()
    {
        using var reader = _table.CreateDataReader();
        reader.Read();
        return _mapper.MapToResult(_memberInitSpec, reader);
    }

    [Benchmark]
    public string MapToResult_ScalarProjection()
    {
        using var reader = _table.CreateDataReader();
        reader.Read();
        return _mapper.MapToResult(_scalarSpec, reader);
    }

    public sealed class PersonEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public sealed record PersonRecord(int Id, string Name);
    public sealed class PersonInitDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}