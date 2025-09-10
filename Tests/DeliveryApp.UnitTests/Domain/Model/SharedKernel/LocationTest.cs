using System.Collections.Generic;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.Model.SharedKernel;

public class LocationShould
{
    public static IEnumerable<object[]> GetLocations()
    {
        yield return [Location.Create(1, 1).Value, 0];
        yield return [Location.Create(1, 2).Value, 1];
        yield return [Location.Create(2, 1).Value, 1];
        yield return [Location.Create(2, 2).Value, 2];
        yield return [Location.Create(10, 10).Value, 18];
    }

    [Fact]
    public void BeCorrectWhenParamsAreCorrectOnCreated()
    {
        //Arrange

        //Act
        var location = Location.Create(1, 1);

        //Assert
        location.IsSuccess.Should().BeTrue();
        location.Value.X.Should().Be(1);
        location.Value.Y.Should().Be(1);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(-1, 1)]
    [InlineData(11, 1)]
    [InlineData(1, -1)]
    [InlineData(1, 11)]
    public void ReturnErrorWhenParamsAreInCorrectOnCreated(int x, int y)
    {
        //Arrange

        //Act
        var location = Location.Create(x, y);

        //Assert
        location.IsSuccess.Should().BeFalse();
        location.Error.Should().NotBeNull();
    }

    [Fact]
    public void CanCreateRandomLocation()
    {
        //Arrange

        //Act
        var location = Location.CreateRandom();

        //Assert
        location.Should().NotBeNull();
        location.X.Should().BeGreaterThanOrEqualTo(1).And.BeLessThanOrEqualTo(10);
        location.Y.Should().BeGreaterThanOrEqualTo(1).And.BeLessThanOrEqualTo(10);
    }

    [Fact]
    public void BeEqualWhenAllPropertiesIsEqual()
    {
        //Arrange
        var first = Location.Create(1, 1).Value;
        var second = Location.Create(1, 1).Value;

        //Act
        var result = first == second;

        //Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void BeNotEqualWhenAllPropertiesIsEqual()
    {
        //Arrange
        var first = Location.Create(1, 1).Value;
        var second = Location.Create(10, 10).Value;

        //Act
        var result = first == second;

        //Assert
        result.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(GetLocations))]
    public void ReturnDistanceBetweenTwoLocations(Location anotherLocation, int distance)
    {
        //Arrange
        var location = Location.Create(1, 1).Value;

        //Act
        var result = location.DistanceTo(anotherLocation);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(distance);
    }
}