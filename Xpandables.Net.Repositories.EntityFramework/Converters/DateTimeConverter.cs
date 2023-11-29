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
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Xpandables.Net.Converters;
/// <summary>
/// Converts a <see cref="DateOnly"/> to <see cref="DateTime"/> and vis-versa.
/// </summary>
public sealed class DateOnlyConverter : ValueConverter<DateOnly, DateTime>
{
    ///<inheritdoc/>
    public DateOnlyConverter()
        : base(
            dateOnly => dateOnly.ToDateTime(TimeOnly.MinValue),
            dateTime => DateOnly.FromDateTime(dateTime))
    { }
}

/// <summary>
/// Compares two <see cref="DateOnly"/>.
/// </summary>
public sealed class DateOnlyComparer : ValueComparer<DateOnly>
{
    ///<inheritdoc/>
    public DateOnlyComparer()
        : base(
            (d1, d2) => d1.DayNumber == d2.DayNumber,
            d => d.GetHashCode())
    { }
}

/// <summary>
/// Converts a <see cref="TimeOnly"/> to <see cref="TimeSpan"/> and vis-versa.
/// </summary>
public sealed class TimeOnlyConverter : ValueConverter<TimeOnly, TimeSpan>
{
    ///<inheritdoc/>
    public TimeOnlyConverter()
        : base(
            timeOnly => timeOnly.ToTimeSpan(),
            timeSpan => TimeOnly.FromTimeSpan(timeSpan))
    { }
}

/// <summary>
/// Compares two <see cref="TimeOnly"/>.
/// </summary>
public sealed class TimeOnlyComparer : ValueComparer<TimeOnly>
{
    ///<inheritdoc/>
    public TimeOnlyComparer()
        : base(
            (t1, t2) => t1.Ticks == t2.Ticks,
            t => t.GetHashCode())
    { }
}

