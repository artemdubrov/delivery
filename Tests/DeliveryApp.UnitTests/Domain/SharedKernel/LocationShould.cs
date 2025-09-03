using System;
using System.Collections;
using System.Collections.Generic;
using DeliveryApp.Core.Domain.SharedKernel;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.SharedKernel;


public class LocationShould
{
    [Theory]
    [InlineData(1,1)]
    [InlineData(10,10)]
    [InlineData(5,5)]
    public void ReturnTheCorrectLocationObject(int x, int y)
    {
        //Arrange
        
        //Act
        var location = Location.Create(x, y);

        //Assert
        location.X.Should().Be(x);
        location.Y.Should().Be(y);
    }
    
    [Fact]
    public void ReturnTheCorrectRandomLocationObject()
    {
        //Arrange
        
        //Act
        var location = Location.CreateRandom();

        //Assert
        location.X.Should().BeGreaterThanOrEqualTo(1);
        location.X.Should().BeLessThanOrEqualTo(10);
        location.Y.Should().BeGreaterThanOrEqualTo(1);
        location.Y.Should().BeLessThanOrEqualTo(10);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    [InlineData(-1)]
    [InlineData(12)]
    public void ReturnArgumentOutOfRangeExceptionIfXOutsideTheRange(int x)
    {
        //Arrange
        const int y = 5;
        
        //Act
        var act = () => Location.Create(x, y);

        //Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("Значение должно быть в диапазоне от 1 до 10*");
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    [InlineData(-1)]
    [InlineData(12)]
    public void ReturnArgumentOutOfRangeExceptionIfYOutsideTheRange(int y)
    {
        //Arrange
        const int x = 5;
        
        //Act
        var act = () => Location.Create(x, y);

        //Assert
        act.Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithMessage("Значение должно быть в диапазоне от 1 до 10*");
    }

    [Fact]
    public void BeEqualToTheLocationWithTheSameParameters()
    {
        //Arrange
        var l1 = Location.Create(1, 2);
        var l2 = Location.Create(1, 2);
        
        //Act
        var result = l1 == l2;

        //Assert
        result.Should().BeTrue();
    }
    
    [Theory]
    [InlineData(1,3)]
    [InlineData(3,2)]
    [InlineData(5,5)]
    public void NotBeEqualToTheLocationWithDifferentParameters(int x, int y)
    {
        //Arrange
        var l1 = Location.Create(1, 2);
        var l2 = Location.Create(x, y);
        
        //Act
        var result = l1 == l2;

        //Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CalculateNumberOfStepsWithTheSameCoordinates()
    {
        //Arrange
        var l1 = Location.Create(1, 1);
        var l2 = Location.Create(1, 1);

        //Act
        var steps = l1.DistansceTo(l2);

        //Assert
        steps.Should().Be(0);
    }
    
    [Theory]
    [ClassData(typeof(LocationTestData))]
    public void CalculateNumberOfStepsWithTheDifferentCoordinates(Location l1, Location l2, int shouldSteps)
    {
        //Arrange

        //Act
        var steps = l1.DistansceTo(l2);

        //Assert
        steps.Should().Be(shouldSteps);
    }
    
    [Fact]
    public void ThrowArgumentNullExceptionIfLocationIsNull()
    {
        // Arrange
        var l1 = Location.Create(1, 1);
        Location l2 = null;

        // Act
        var act = () => l1.DistansceTo(l2);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*location*"); // Проверяем, что указан правильный параметр
    }


    private class LocationTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { Location.Create(1,1), Location.Create(10, 10), 18};
            yield return new object[] { Location.Create(10,10), Location.Create(1, 1), 18};
            yield return new object[] { Location.Create(4,9), Location.Create(2, 6), 5};
            yield return new object[] { Location.Create(8,2), Location.Create(9, 2), 1};
            yield return new object[] { Location.Create(1,3), Location.Create(10, 3), 9};
            yield return new object[] { Location.Create(5,1), Location.Create(5, 10), 9};
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    }
}