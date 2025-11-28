using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Shop.Domain.Entities;
using Shop.Domain.Enums;
using Shop.Features.Purchases.CreatePurchase;
using Shop.Infrastructure;

namespace Shop.Tests;

public class CreatePurchaseCommandHandlerTests
{
    private readonly ILogger<CreatePurchaseCommandHandler> _logger;
    private readonly ApplicationContext _context;
    private readonly CreatePurchaseCommandHandler _handler;

    public CreatePurchaseCommandHandlerTests()
    {
        _logger = new NullLogger<CreatePurchaseCommandHandler>();

        var options = new DbContextOptionsBuilder<ApplicationContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationContext(options);
        _handler = new CreatePurchaseCommandHandler(_context, _logger);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCreatePurchase()
    {
        // Arrange
        int quantity = 10;
        int remainQuantity = 7;

        var seller = new User
        {
            Id = Guid.NewGuid(),
            Username = "seller1",
            Login = "seller1",
            PasswordHash = "hash1",
            Role = Role.Seller
        };

        var buyer = new User
        {
            Id = Guid.NewGuid(),
            Username = "buyer1",
            Login = "buyer1",
            PasswordHash = "hash2",
            Role = Role.Buyer
        };

        var item = new Item
        {
            Id = 1,
            Name = "Test Item",
            Price = 100.50m,
            Quantity = quantity,
            SellerId = seller.Id,
            Seller = seller
        };

        await _context.Users.AddRangeAsync(seller, buyer);
        await _context.Items.AddAsync(item);
        await _context.SaveChangesAsync();

        var command = new CreatePurchaseCommand(item.Id, buyer.Id, 3);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(command.Quantity, result.Value.Quantity);
        Assert.Equal(item.Price * command.Quantity, result.Value.TotalPrice);

        var updatedItem = await _context.Items.FindAsync(item.Id);
        Assert.Equal(remainQuantity, updatedItem.Quantity);

        var purchase = await _context.Purchases.FirstOrDefaultAsync();
        Assert.NotNull(purchase);
        Assert.Equal(item.Id, purchase.ItemId);
        Assert.Equal(buyer.Id, purchase.BuyerId);
        Assert.Equal(seller.Id, purchase.SellerId);
    }

    [Fact]
    public async Task Handle_WithNonExistentItem_ShouldReturnNotFound()
    {
        // Arrange
        var command = new CreatePurchaseCommand
        (
            ItemId: 23465,
            BuyerId: Guid.NewGuid(),
            Quantity: 1
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task Handle_WithNotEnoughQuantity_ShouldReturnError()
    {
        // Arrange
        int actualQuantity = 2; // Мало товара
        int requestingQuantity = 5; // Запрашиваемое количество больше

        var seller = new User
        {
            Id = Guid.NewGuid(),
            Username = "seller1",
            Login = "seller1",
            PasswordHash = "hash1",
            Role = Role.Seller
        };

        var buyer = new User
        {
            Id = Guid.NewGuid(),
            Username = "buyer1",
            Login = "buyer1",
            PasswordHash = "hash2",
            Role = Role.Buyer
        };

        var item = new Item
        {
            Id = 1,
            Name = "Test Item",
            Price = 100.50m,
            Quantity = actualQuantity,
            SellerId = seller.Id
        };

        await _context.Users.AddRangeAsync(seller, buyer);
        await _context.Items.AddAsync(item);
        await _context.SaveChangesAsync();

        var command = new CreatePurchaseCommand
        (
            ItemId: item.Id,
            BuyerId: buyer.Id,
            Quantity: requestingQuantity
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Error, result.Status);
        Assert.Contains("Not enough items", result.Errors.First());
    }

    [Fact]
    public async Task Handle_WhenBuyerIsSeller_ShouldReturnError()
    {
        // Arrange
        var seller = new User
        {
            Id = Guid.NewGuid(),
            Username = "seller1",
            Login = "seller1",
            PasswordHash = "hash1",
            Role = Role.Seller
        };

        var item = new Item
        {
            Id = 1,
            Name = "Test Item",
            Price = 100.50m,
            Quantity = 10,
            SellerId = seller.Id,
            Seller = seller
        };

        await _context.Users.AddAsync(seller);
        await _context.Items.AddAsync(item);
        await _context.SaveChangesAsync();

        var command = new CreatePurchaseCommand
        (
            ItemId: item.Id,
            BuyerId: seller.Id,
            Quantity: 1
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Error, result.Status);
        Assert.Contains("Attempt to buy own product", result.Errors.First());
    }

    [Fact]
    public async Task Handle_WithNonExistentBuyer_ShouldReturnUnauthorized()
    {
        // Arrange
        var seller = new User
        {
            Id = Guid.NewGuid(),
            Username = "seller1",
            Login = "seller1",
            PasswordHash = "hash1",
            Role = Role.Seller
        };

        var item = new Item
        {
            Id = 1,
            Name = "Test Item",
            Price = 100.50m,
            Quantity = 10,
            SellerId = seller.Id
        };

        await _context.Users.AddAsync(seller);
        await _context.Items.AddAsync(item);
        await _context.SaveChangesAsync();

        var command = new CreatePurchaseCommand
        (
            ItemId: item.Id,
            BuyerId: Guid.NewGuid(), // Несуществующий покупатель
            Quantity: 1
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Unauthorized, result.Status);
    }

    [Fact]
    public async Task Handle_WithConcurrentRequests_ShouldProcessSequentially()
    {
        // Arrange
        var seller = new User
        {
            Id = Guid.NewGuid(),
            Username = "seller1",
            Login = "seller1",
            PasswordHash = "hash1",
            Role = Role.Seller
        };

        var buyer = new User
        {
            Id = Guid.NewGuid(),
            Username = "buyer1",
            Login = "buyer1",
            PasswordHash = "hash2",
            Role = Role.Buyer
        };

        var item = new Item
        {
            Id = 1,
            Name = "Test Item",
            Price = 50.0m,
            Quantity = 5, // Всего 5 товаров
            SellerId = seller.Id
        };

        await _context.Users.AddRangeAsync(seller, buyer);
        await _context.Items.AddAsync(item);
        await _context.SaveChangesAsync();

        var command1 = new CreatePurchaseCommand
        (
            ItemId: item.Id,
            BuyerId: buyer.Id,
            Quantity: 3
        );

        var command2 = new CreatePurchaseCommand
        (
            ItemId: item.Id,
            BuyerId: buyer.Id,
            Quantity: 3
        );

        // Act
        var task1 = _handler.Handle(command1, CancellationToken.None);
        var task2 = _handler.Handle(command2, CancellationToken.None);

        var results = await Task.WhenAll(task1, task2);

        // Assert
        Assert.True(results[0].IsSuccess); // Первый запрос успешен
        Assert.False(results[1].IsSuccess); // Второй запрос провалился

        var updatedItem = await _context.Items.FindAsync(item.Id);
        Assert.Equal(2, updatedItem.Quantity); // 5 - 3 = 2

        var purchases = await _context.Purchases.ToListAsync();
        Assert.Single(purchases); // Создалась только одна покупка
    }

    [Fact]
    public async Task Handle_ShouldCalculateTotalPriceCorrectly()
    {
        // Arrange
        int quantity = 4;
        decimal itemPrice = 25.75m;

        var seller = new User
        {
            Id = Guid.NewGuid(),
            Username = "seller1",
            Login = "seller1",
            PasswordHash = "hash1",
            Role = Role.Seller
        };

        var buyer = new User
        {
            Id = Guid.NewGuid(),
            Username = "buyer1",
            Login = "buyer1",
            PasswordHash = "hash2",
            Role = Role.Buyer
        };

        var item = new Item
        {
            Id = 1,
            Name = "Test Item",
            Price = itemPrice,
            Quantity = 10,
            SellerId = seller.Id,
            Seller = seller
        };

        await _context.Users.AddRangeAsync(seller, buyer);
        await _context.Items.AddAsync(item);
        await _context.SaveChangesAsync();

        var command = new CreatePurchaseCommand
        (
            ItemId: item.Id,
            BuyerId: buyer.Id,
            Quantity: quantity
        );

        var expectedTotalPrice = itemPrice * quantity;

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedTotalPrice, result.Value.TotalPrice);

        var purchase = await _context.Purchases.FirstOrDefaultAsync();
        Assert.Equal(expectedTotalPrice, purchase.TotalPrice);
    }
}