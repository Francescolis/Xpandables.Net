/*******************************************************************************
 * Copyright (C) 2024 Francis-Black EWANE
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
using System.Net.Repositories;
using System.Text.Json;

using FluentAssertions;

namespace System.Net.UnitTests.Repositories;

/// <summary>
/// Comprehensive tests for the enhanced EntityStatus implementation.
/// </summary>
public class EntityStatusEnhancementTests
{
    [Fact]
    public void EntityStatus_PredefinedValues_ShouldBeCorrect()
    {
        // Act & Assert - Test all predefined status values
        EntityStatus.ACTIVE.Value.Should().Be("ACTIVE");
        EntityStatus.PENDING.Value.Should().Be("PENDING");
        EntityStatus.PROCESSING.Value.Should().Be("PROCESSING");
        EntityStatus.DELETED.Value.Should().Be("DELETED");
        EntityStatus.SUSPENDED.Value.Should().Be("SUSPENDED");
        EntityStatus.ONERROR.Value.Should().Be("ONERROR");
        EntityStatus.PUBLISHED.Value.Should().Be("PUBLISHED");
    }

    [Fact]
    public void EntityStatus_Create_ShouldWorkCorrectly()
    {
        // Arrange & Act
        var customStatus = EntityStatus.Create("CUSTOM_STATUS");
        var activeStatus = EntityStatus.Create("active"); // Test case insensitive

        // Assert
        customStatus.Value.Should().Be("CUSTOM_STATUS");
        activeStatus.Value.Should().Be("ACTIVE"); // Should use cached value

        // Test null/empty validation
        Assert.Throws<ArgumentNullException>(() => EntityStatus.Create(null!));
        Assert.Throws<ArgumentException>(() => EntityStatus.Create(""));
        Assert.Throws<ArgumentException>(() => EntityStatus.Create("   "));
    }

    [Fact]
    public void EntityStatus_GetDefault_ShouldReturnActive()
    {
        // Act
        var defaultStatus = EntityStatus.GetDefault();

        // Assert
        defaultStatus.Should().Be(EntityStatus.ACTIVE);
        defaultStatus.Value.Should().Be("ACTIVE");
    }

    [Fact]
    public void EntityStatus_TryCreate_ShouldWorkCorrectly()
    {
        // Act & Assert - Valid cases
        EntityStatus.TryCreate("ACTIVE", out var status1).Should().BeTrue();
        status1.Should().Be(EntityStatus.ACTIVE);

        EntityStatus.TryCreate("custom", out var status2).Should().BeTrue();
        status2.Value.Should().Be("CUSTOM");

        // Invalid cases
        EntityStatus.TryCreate(null, out var status3).Should().BeFalse();
        status3.Should().Be(default);

        EntityStatus.TryCreate("", out var status4).Should().BeFalse();
        status4.Should().Be(default);
    }

    [Fact]
    public void EntityStatus_TryParse_ShouldWorkCorrectly()
    {
        // Act & Assert - Valid predefined statuses
        EntityStatus.TryParse("ACTIVE", out var status1).Should().BeTrue();
        status1.Should().Be(EntityStatus.ACTIVE);

        EntityStatus.TryParse("pending", out var status2).Should().BeTrue();
        status2.Should().Be(EntityStatus.PENDING);

        // Invalid cases
        EntityStatus.TryParse("UNKNOWN", out var status3).Should().BeFalse();
        status3.Should().Be(default);

        EntityStatus.TryParse(null, out var status4).Should().BeFalse();
        status4.Should().Be(default);
    }

    [Fact]
    public void EntityStatus_ImplicitConversions_ShouldWork()
    {
        // Arrange
        EntityStatus status = "ACTIVE"; // Implicit conversion from string

        // Act
        string stringValue = status; // Implicit conversion to string

        // Assert
        status.Should().Be(EntityStatus.ACTIVE);
        stringValue.Should().Be("ACTIVE");
    }

    [Fact]
    public void EntityStatus_Equality_ShouldWorkCorrectly()
    {
        // Arrange
        var status1 = EntityStatus.Create("ACTIVE");
        var status2 = EntityStatus.ACTIVE;
        var status3 = EntityStatus.Create("active"); // Different case
        var status4 = EntityStatus.PENDING;

        // Act & Assert
        status1.Equals(status2).Should().BeTrue();
        status1.Equals(status3).Should().BeTrue(); // Case insensitive
        status1.Equals(status4).Should().BeFalse();

        // Test GetHashCode consistency
        status1.GetHashCode().Should().Be(status2.GetHashCode());
        status1.GetHashCode().Should().Be(status3.GetHashCode());
    }

    [Fact]
    public void EntityStatus_Comparison_ShouldWorkCorrectly()
    {
        // Arrange
        var active = EntityStatus.ACTIVE;
        var pending = EntityStatus.PENDING;

        // Act & Assert
        (active < pending).Should().BeTrue(); // "ACTIVE" < "PENDING"
        (pending > active).Should().BeTrue();
        (active <= pending).Should().BeTrue();
        (pending >= active).Should().BeTrue();

        // Test CompareTo methods
        active.CompareTo(pending).Should().BeLessThan(0);
        pending.CompareTo(active).Should().BeGreaterThan(0);
        active.CompareTo(active).Should().Be(0);
    }

    [Fact]
    public void EntityStatus_UtilityMethods_ShouldWorkCorrectly()
    {
        // Test IsValidStatus
        EntityStatus.IsValidStatus("ACTIVE").Should().BeTrue();
        EntityStatus.IsValidStatus("active").Should().BeTrue();
        EntityStatus.IsValidStatus("UNKNOWN").Should().BeFalse();
        EntityStatus.IsValidStatus(null).Should().BeFalse();

        // Test state checking methods
        EntityStatus.ACTIVE.IsActive().Should().BeTrue();
        EntityStatus.PUBLISHED.IsActive().Should().BeTrue();
        EntityStatus.PENDING.IsActive().Should().BeFalse();

        EntityStatus.PENDING.IsTransitional().Should().BeTrue();
        EntityStatus.PROCESSING.IsTransitional().Should().BeTrue();
        EntityStatus.ACTIVE.IsTransitional().Should().BeFalse();

        EntityStatus.DELETED.IsTerminal().Should().BeTrue();
        EntityStatus.ONERROR.IsTerminal().Should().BeTrue();
        EntityStatus.ACTIVE.IsTerminal().Should().BeFalse();
    }

    [Fact]
    public void EntityStatus_AllStatuses_ShouldContainAllPredefined()
    {
        // Act
        var allStatuses = EntityStatus.AllStatuses;

        // Assert
        allStatuses.Should().HaveCount(7);
        allStatuses.Keys.Should().Contain(["ACTIVE", "PENDING", "PROCESSING", "DELETED", "SUSPENDED", "ONERROR", "PUBLISHED"]);

        // Test that all predefined statuses are in the collection
        allStatuses["ACTIVE"].Should().Be(EntityStatus.ACTIVE);
        allStatuses["PENDING"].Should().Be(EntityStatus.PENDING);
        allStatuses["PROCESSING"].Should().Be(EntityStatus.PROCESSING);
        allStatuses["DELETED"].Should().Be(EntityStatus.DELETED);
        allStatuses["SUSPENDED"].Should().Be(EntityStatus.SUSPENDED);
        allStatuses["ONERROR"].Should().Be(EntityStatus.ONERROR);
        allStatuses["PUBLISHED"].Should().Be(EntityStatus.PUBLISHED);
    }

    [Fact]
    public void EntityStatus_JsonSerialization_ShouldWorkCorrectly()
    {
        // Arrange
        var status = EntityStatus.ACTIVE;
        var options = new JsonSerializerOptions
        {
            Converters = { new System.Net.Abstractions.Text.PrimitiveJsonConverterFactory() }
        };

        // Act
        var json = JsonSerializer.Serialize(status, options);
        var deserialized = JsonSerializer.Deserialize<EntityStatus>(json, options);

        // Assert
        json.Should().Be("\"ACTIVE\"");
        deserialized.Should().Be(EntityStatus.ACTIVE);
        deserialized.Value.Should().Be("ACTIVE");
    }

    [Fact]
    public void EntityStatus_JsonSerialization_Array_ShouldWorkCorrectly()
    {
        // Arrange
        var statuses = new[] { EntityStatus.ACTIVE, EntityStatus.PENDING, EntityStatus.DELETED };
        var options = new JsonSerializerOptions
        {
            Converters = { new System.Net.Abstractions.Text.PrimitiveJsonConverterFactory() }
        };

        // Act
        var json = JsonSerializer.Serialize(statuses, options);
        var deserialized = JsonSerializer.Deserialize<EntityStatus[]>(json, options);

        // Assert
        json.Should().Be("[\"ACTIVE\",\"PENDING\",\"DELETED\"]");
        deserialized.Should().NotBeNull();
        deserialized!.Should().HaveCount(3);
        deserialized[0].Should().Be(EntityStatus.ACTIVE);
        deserialized[1].Should().Be(EntityStatus.PENDING);
        deserialized[2].Should().Be(EntityStatus.DELETED);
    }

    [Fact]
    public void EntityStatusValidation_PredefinedOnly_ShouldWorkCorrectly()
    {
        // Arrange
        var attribute = new EntityStatusValidationAttribute(allowCustomStatuses: false);

        // Act & Assert - Valid predefined statuses
        attribute.IsValid("ACTIVE").Should().BeTrue();
        attribute.IsValid("pending").Should().BeTrue(); // Case insensitive
        attribute.IsValid(EntityStatus.DELETED).Should().BeTrue();

        // Invalid cases
        attribute.IsValid("UNKNOWN").Should().BeFalse();
        attribute.IsValid("").Should().BeFalse();
        attribute.IsValid(123).Should().BeFalse();

        // Null handling
        attribute.AllowNull = false;
        attribute.IsValid(null).Should().BeFalse();

        attribute.AllowNull = true;
        attribute.IsValid(null).Should().BeTrue();
    }

    [Fact]
    public void EntityStatusValidation_CustomAllowed_ShouldWorkCorrectly()
    {
        // Arrange
        var attribute = new EntityStatusValidationAttribute(allowCustomStatuses: true);

        // Act & Assert - Both predefined and custom should be valid
        attribute.IsValid("ACTIVE").Should().BeTrue();
        attribute.IsValid("CUSTOM_STATUS").Should().BeTrue();
        attribute.IsValid("anything").Should().BeTrue();

        // Invalid cases
        attribute.IsValid("").Should().BeFalse();
        attribute.IsValid("   ").Should().BeFalse();
        attribute.IsValid(123).Should().BeFalse();
        attribute.IsValid(null).Should().BeFalse(); // AllowNull is false by default
    }

    [Fact]
    public void EntityStatusValidation_FormatErrorMessage_ShouldWorkCorrectly()
    {
        // Arrange
        var strictAttribute = new EntityStatusValidationAttribute(allowCustomStatuses: false);
        var flexibleAttribute = new EntityStatusValidationAttribute(allowCustomStatuses: true);

        // Act
        var strictMessage = strictAttribute.FormatErrorMessage("Status");
        var flexibleMessage = flexibleAttribute.FormatErrorMessage("Status");

        // Assert
        strictMessage.Should().Contain("ACTIVE").And.Contain("PENDING").And.Contain("DELETED");
        flexibleMessage.Should().Contain("null or whitespace");
    }

    [Fact]
    public void EntityStatus_Performance_CachingBehavior()
    {
        // This test verifies that predefined statuses use cached instances
        var status1 = EntityStatus.Create("ACTIVE");
        var status2 = EntityStatus.ACTIVE;

        // For predefined statuses, the cached instance should be used
        status1.Should().Be(status2);

        // Custom statuses should work but create new instances
        var custom1 = EntityStatus.Create("CUSTOM1");
        var custom2 = EntityStatus.Create("CUSTOM1");

        // Values should be equal but instances might be different for custom statuses
        custom1.Should().Be(custom2); // Equal based on value
        custom1.Value.Should().Be(custom2.Value);
    }
}