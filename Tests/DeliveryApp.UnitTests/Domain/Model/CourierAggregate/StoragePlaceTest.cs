using System;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.Model.CourierAggregate;

public class CourierAggregateShould
{
    [Theory]
    [InlineData("bag", 10)]
    [InlineData("backpack", 5)]
    public void CreateStorageWhenCorrectParameters(string name, int volume)
    {
        // Arrange

        // Act
        var storagePlace = StoragePlace.Create(name, volume);

        // Assert
        storagePlace.IsSuccess.Should().BeTrue();
        storagePlace.Value.Name.Should().Be(name);
        storagePlace.Value.TotalVolume.Should().Be(volume);
    }

    [Theory]
    [InlineData("", 10)]
    [InlineData(" ", 10)]
    [InlineData(null, 10)]
    [InlineData("bag", 0)]
    [InlineData("bag", -1)]
    public void ReturnErrorWhenIncorrectParameters(string name, int volume)
    {
        // Arrange

        // Act
        var storagePlace = StoragePlace.Create(name, volume);

        // Assert
        storagePlace.IsSuccess.Should().BeFalse();
        storagePlace.Error.Should().NotBeNull();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    public void AllowStoreWhenItemSmallerStorage(int volume)
    {
        // Arrange
        var storagePlaceResult = StoragePlace.Create("bag", 5);
        storagePlaceResult.IsSuccess.Should().BeTrue();
        var storagePlace = storagePlaceResult.Value;

        // Act
        var result = storagePlace.CanStore(volume);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(6)]
    [InlineData(10)]
    public void NotAllowStoreWhenItemLargerThanStorage(int volume)
    {
        // Arrange
        var storagePlaceResult = StoragePlace.Create("bag", 5);
        storagePlaceResult.IsSuccess.Should().BeTrue();
        var storagePlace = storagePlaceResult.Value;

        // Act
        var result = storagePlace.CanStore(volume);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(StoragePlace.Errors.OrderVolumeExceedStorageVolume());
    }

    [Fact]
    public void NotAllowStoreWhenOccupied()
    {
        // Arrange
        var storagePlaceResult = StoragePlace.Create("bag", 5);
        storagePlaceResult.IsSuccess.Should().BeTrue();
        var storagePlace = storagePlaceResult.Value;
        storagePlace.Store(Guid.NewGuid(), 5);

        // Act
        var result = storagePlace.CanStore(5);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(StoragePlace.Errors.StorageIsOccupied());

    }

    [Fact]
    public void AllowStoreAfterStorageCleared()
    {
        // Arrange
        var storagePlaceResult = StoragePlace.Create("bag", 5);
        storagePlaceResult.IsSuccess.Should().BeTrue();
        var storagePlace = storagePlaceResult.Value;
        var order = Guid.NewGuid();
        storagePlace.Store(order, 5);
        storagePlace.Clear(order);

        // Act
        var result = storagePlace.CanStore(5);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ReturnErrorWhenIncorrectVolume(int volume)
    {
        // Arrange
        // Arrange
        var storagePlaceResult = StoragePlace.Create("bag", 5);
        storagePlaceResult.IsSuccess.Should().BeTrue();
        var storagePlace = storagePlaceResult.Value;

        // Act
        var result = storagePlace.CanStore(volume);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    public void StoreWhenItemSmallerStorage(int volume)
    {
        // Arrange
        var storagePlaceResult = StoragePlace.Create("bag", 5);
        storagePlaceResult.IsSuccess.Should().BeTrue();
        var storagePlace = storagePlaceResult.Value;

        // Act
        var result = storagePlace.Store(Guid.NewGuid(), volume);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(6)]
    [InlineData(10)]
    public void NotStoreWhenItemLargerStorage(int volume)
    {
        // Arrange
        var storagePlaceResult = StoragePlace.Create("bag", 5);
        storagePlaceResult.IsSuccess.Should().BeTrue();
        var storagePlace = storagePlaceResult.Value;

        // Act
        var result = storagePlace.Store(Guid.NewGuid(), volume);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(StoragePlace.Errors.OrderVolumeExceedStorageVolume());
    }

    [Fact]
    public void NotStoreWhenOccupied()
    {
        // Arrange
        var storagePlaceResult = StoragePlace.Create("bag", 5);
        storagePlaceResult.IsSuccess.Should().BeTrue();
        var storagePlace = storagePlaceResult.Value;
        storagePlace.Store(Guid.NewGuid(), 5);

        // Act
        var result = storagePlace.Store(Guid.NewGuid(), 5);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void StoreAfterStorageCleared()
    {
        // Arrange
        var storagePlaceResult = StoragePlace.Create("bag", 5);
        storagePlaceResult.IsSuccess.Should().BeTrue();
        var storagePlace = storagePlaceResult.Value;
        var order = Guid.NewGuid();
        storagePlace.Store(order, 5);
        storagePlace.Clear(order);

        // Act
        var result = storagePlace.Store(Guid.NewGuid(), 5);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}