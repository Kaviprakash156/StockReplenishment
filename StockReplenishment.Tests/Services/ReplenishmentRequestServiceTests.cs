using NSubstitute;
using StockReplenishment.Application.DTOs.ReplenishmentRequests;
using StockReplenishment.Application.Interfaces;
using StockReplenishment.Application.Services;
using StockReplenishment.Domain.Entities;
using StockReplenishment.Domain.Enums;

namespace StockReplenishment.Tests.Services;

public class ReplenishmentRequestServiceTests
{
    private IReplenishmentRequestRepository _repository = null!;
    private ReplenishmentRequestService _service = null!;
    private IStockValidationQueue _stockValidationQueue = null!;

    [SetUp]
    public void Setup()
    {
        _repository =
            Substitute.For<IReplenishmentRequestRepository>();

        _stockValidationQueue =
            Substitute.For<IStockValidationQueue>();

        _service =
            new ReplenishmentRequestService(
                _repository,
                _stockValidationQueue);
    }

    [Test]
    public async Task CreateAsync_WithValidRequest_CreatesDraftRequest()
    {
        // Arrange
        var location = new StockLocation
        {
            Id = 1,
            Code = "LINE-A-01",
            Name = "Production Line A - Station 01"
        };

        _repository
            .GetStockLocationByIdAsync(1)
            .Returns(location);

        var request = new CreateReplenishmentRequestDto
        {
            StockLocationId = 1,
            Priority = (int)RequestPriority.Urgent,
            CreatedBy = "worker01",
            Items =
            [
                new CreateReplenishmentRequestItemDto
            {
                ArticleNumber = "MAT-1001",
                Description = "Steel Bolt",
                RequestedQuantity = 50
            }
            ]
        };

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Assert.That(result.Status, Is.EqualTo("Draft"));
        Assert.That(result.Priority, Is.EqualTo("Urgent"));
        Assert.That(result.Items, Has.Count.EqualTo(1));
        Assert.That(
            result.Items[0].RequestedQuantity,
            Is.EqualTo(50));

        await _repository
            .Received(1)
            .AddAsync(Arg.Any<ReplenishmentRequest>());

        await _repository
            .Received(1)
            .SaveChangesAsync();
    }

    [Test]
    public void CreateAsync_WithInvalidLocation_ThrowsArgumentException()
    {
        // Arrange
        _repository
            .GetStockLocationByIdAsync(999)
            .Returns((StockLocation?)null);

        var request = new CreateReplenishmentRequestDto
        {
            StockLocationId = 999,
            Priority = (int)RequestPriority.Normal,
            CreatedBy = "worker01",
            Items =
            [
                new CreateReplenishmentRequestItemDto
            {
                ArticleNumber = "MAT-1001",
                Description = "Steel Bolt",
                RequestedQuantity = 10
            }
            ]
        };

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.CreateAsync(request));
    }

    [Test]
    public void CreateAsync_WithNoItems_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateReplenishmentRequestDto
        {
            StockLocationId = 1,
            Priority = (int)RequestPriority.Normal,
            CreatedBy = "worker01",
            Items = []
        };

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.CreateAsync(request));
    }

    private static ReplenishmentRequest CreateDraftRequest()
    {
        return new ReplenishmentRequest
        {
            Id = 1,
            StockLocationId = 1,
            StockLocation = new StockLocation
            {
                Id = 1,
                Code = "LINE-A-01",
                Name = "Production Line A - Station 01"
            },
            Priority = RequestPriority.Normal,
            Status = RequestStatus.Draft,
            CreatedBy = "worker01",
            CreatedAt = DateTime.UtcNow,
            StockValidationStatus =
                StockValidationStatus.NotStarted,
            Items =
            [
                new ReplenishmentRequestItem
            {
                Id = 1,
                ReplenishmentRequestId = 1,
                ArticleNumber = "MAT-1001",
                Description = "Steel Bolt",
                RequestedQuantity = 50,
                FulfilledQuantity = 0
            }
            ]
        };
    }

    [Test]
    public async Task SubmitAsync_WhenDraft_ChangesStatusToSubmitted()
    {
        // Arrange
        var request = CreateDraftRequest();

        _repository
            .GetByIdAsync(1)
            .Returns(request);

        // Act
        await _service.SubmitAsync(1);

        await _stockValidationQueue
    .Received(1)
    .QueueAsync(request.Id);

        // Assert
        Assert.That(
            request.Status,
            Is.EqualTo(RequestStatus.Submitted));

        Assert.That(
            request.StockValidationStatus,
            Is.EqualTo(StockValidationStatus.Pending));

        Assert.That(
            request.SubmittedAt,
            Is.Not.Null);

        await _repository
            .Received(1)
            .SaveChangesAsync();
    }

    [Test]
    public void SubmitAsync_WhenAlreadyApproved_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = CreateDraftRequest();

        request.Status = RequestStatus.Approved;

        _repository
            .GetByIdAsync(1)
            .Returns(request);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.SubmitAsync(1));
    }

    [Test]
    public async Task ApproveAsync_WhenValidationIsAvailable_ApprovesRequest()
    {
        // Arrange
        var request = CreateDraftRequest();

        request.Status = RequestStatus.Submitted;

        request.StockValidationStatus =
            StockValidationStatus.Available;

        _repository
            .GetByIdAsync(1)
            .Returns(request);

        // Act
        await _service.ApproveAsync(1);

        // Assert
        Assert.That(
            request.Status,
            Is.EqualTo(RequestStatus.Approved));

        Assert.That(
            request.ApprovedAt,
            Is.Not.Null);

        await _repository
            .Received(1)
            .SaveChangesAsync();
    }

    [Test]
    public void ApproveAsync_WhenValidationIsPending_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = CreateDraftRequest();

        request.Status = RequestStatus.Submitted;

        request.StockValidationStatus =
            StockValidationStatus.Pending;

        _repository
            .GetByIdAsync(1)
            .Returns(request);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.ApproveAsync(1));
    }

    [Test]
    public void RejectAsync_WithoutReason_ThrowsArgumentException()
    {
        // Arrange
        var request = CreateDraftRequest();

        request.Status = RequestStatus.Submitted;

        _repository
            .GetByIdAsync(1)
            .Returns(request);

        var rejection = new RejectReplenishmentRequestDto
        {
            Reason = ""
        };

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.RejectAsync(
                1,
                rejection));
    }

    [Test]
    public async Task RejectAsync_WithReason_RejectsRequest()
    {
        // Arrange
        var request = CreateDraftRequest();

        request.Status = RequestStatus.Submitted;

        _repository
            .GetByIdAsync(1)
            .Returns(request);

        var rejection = new RejectReplenishmentRequestDto
        {
            Reason = "Insufficient stock priority."
        };

        // Act
        await _service.RejectAsync(
            1,
            rejection);

        // Assert
        Assert.That(
            request.Status,
            Is.EqualTo(RequestStatus.Rejected));

        Assert.That(
            request.RejectionReason,
            Is.EqualTo("Insufficient stock priority."));

        Assert.That(
            request.RejectedAt,
            Is.Not.Null);
    }

    [Test]
    public async Task FulfillAsync_WithFullQuantities_FulfillsRequest()
    {
        // Arrange
        var request = CreateDraftRequest();

        request.Status = RequestStatus.Approved;

        request.Items.First().FulfilledQuantity = 0;

        _repository
            .GetByIdAsync(1)
            .Returns(request);

        var fulfillment =
            new FulfillReplenishmentRequestDto
            {
                Items =
                [
                    new FulfillReplenishmentRequestItemDto
                {
                    ItemId = 1,
                    FulfilledQuantity = 50
                }
                ]
            };

        // Act
        await _service.FulfillAsync(
            1,
            fulfillment);

        // Assert
        Assert.That(
            request.Status,
            Is.EqualTo(RequestStatus.Fulfilled));

        Assert.That(
            request.Items.First().FulfilledQuantity,
            Is.EqualTo(50));

        Assert.That(
            request.FulfilledAt,
            Is.Not.Null);
    }

    [Test]
    public void FulfillAsync_WhenQuantityExceedsRequested_ThrowsArgumentException()
    {
        // Arrange
        var request = CreateDraftRequest();

        request.Status = RequestStatus.Approved;

        _repository
            .GetByIdAsync(1)
            .Returns(request);

        var fulfillment =
            new FulfillReplenishmentRequestDto
            {
                Items =
                [
                    new FulfillReplenishmentRequestItemDto
                {
                    ItemId = 1,
                    FulfilledQuantity = 100
                }
                ]
            };

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.FulfillAsync(
                1,
                fulfillment));
    }
}