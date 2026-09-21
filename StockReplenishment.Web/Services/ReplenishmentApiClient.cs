using System.Net.Http.Json;

using StockReplenishment.Application.DTOs.Common;
using StockReplenishment.Application.DTOs.ReplenishmentRequests;
using StockReplenishment.Application.DTOs.StockLocations;
using StockReplenishment.Domain.Enums;

namespace StockReplenishment.Web.Services;

public class ReplenishmentApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ReplenishmentApiClient(
        IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private HttpClient CreateClient()
    {
        return _httpClientFactory.CreateClient(
            "StockReplenishmentApi");
    }

    // =========================================================
    // GET - Request List
    // =========================================================

    public async Task<
        PagedResultDto<ReplenishmentRequestListDto>>
        GetRequestsAsync(
            RequestStatus? status = null,
            RequestPriority? priority = null,
            int? stockLocationId = null,
            int page = 1,
            int pageSize = 10)
    {
        var client = CreateClient();

        var queryParameters =
            new List<string>
            {
                $"page={page}",
                $"pageSize={pageSize}"
            };

        if (status.HasValue)
        {
            queryParameters.Add(
                $"status={(int)status.Value}");
        }

        if (priority.HasValue)
        {
            queryParameters.Add(
                $"priority={(int)priority.Value}");
        }

        if (stockLocationId.HasValue)
        {
            queryParameters.Add(
                $"stockLocationId={stockLocationId.Value}");
        }

        var url =
            "/api/replenishment-requests?" +
            string.Join("&", queryParameters);

        return await client.GetFromJsonAsync<
            PagedResultDto<ReplenishmentRequestListDto>>(url)
            ?? new PagedResultDto<ReplenishmentRequestListDto>();
    }

    // =========================================================
    // GET - Request Details
    // =========================================================

    public async Task<
        ReplenishmentRequestDetailsDto?>
        GetRequestAsync(int id)
    {
        var client = CreateClient();

        var response =
            await client.GetAsync(
                $"/api/replenishment-requests/{id}");

        if (response.StatusCode ==
            System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<
                ReplenishmentRequestDetailsDto>();
    }

    // =========================================================
    // GET - Stock Locations
    // =========================================================

    public async Task<List<StockLocationDto>>
        GetStockLocationsAsync()
    {
        var client = CreateClient();

        return await client.GetFromJsonAsync<
            List<StockLocationDto>>(
                "/api/replenishment-requests/locations")
            ?? new List<StockLocationDto>();
    }

    // =========================================================
    // POST - Create Draft
    // =========================================================

    public async Task<
        ReplenishmentRequestDetailsDto>
        CreateRequestAsync(
            CreateReplenishmentRequestDto request)
    {
        var client = CreateClient();

        var response =
            await client.PostAsJsonAsync(
                "/api/replenishment-requests",
                request);

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<
                ReplenishmentRequestDetailsDto>()
            ?? throw new InvalidOperationException(
                "The API returned an empty response.");
    }

    // =========================================================
    // PUT - Update Draft
    // =========================================================

    public async Task<
        ReplenishmentRequestDetailsDto>
        UpdateRequestAsync(
            int id,
            UpdateReplenishmentRequestDto request)
    {
        var client = CreateClient();

        var response =
            await client.PutAsJsonAsync(
                $"/api/replenishment-requests/{id}",
                request);

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<
                ReplenishmentRequestDetailsDto>()
            ?? throw new InvalidOperationException(
                "The API returned an empty response.");
    }

    // =========================================================
    // POST - Submit
    // =========================================================

    public async Task SubmitRequestAsync(int id)
    {
        var client = CreateClient();

        var response =
            await client.PostAsync(
                $"/api/replenishment-requests/{id}/submit",
                null);

        response.EnsureSuccessStatusCode();
    }

    // =========================================================
    // POST - Approve
    // =========================================================

    public async Task ApproveRequestAsync(int id)
    {
        var client = CreateClient();

        var response =
            await client.PostAsync(
                $"/api/replenishment-requests/{id}/approve",
                null);

        response.EnsureSuccessStatusCode();
    }

    // =========================================================
    // POST - Reject
    // =========================================================

    public async Task RejectRequestAsync(
        int id,
        RejectReplenishmentRequestDto request)
    {
        var client = CreateClient();

        var response =
            await client.PostAsJsonAsync(
                $"/api/replenishment-requests/{id}/reject",
                request);

        response.EnsureSuccessStatusCode();
    }

    // =========================================================
    // POST - Fulfill
    // =========================================================

    public async Task FulfillRequestAsync(
        int id,
        FulfillReplenishmentRequestDto request)
    {
        var client = CreateClient();

        var response =
            await client.PostAsJsonAsync(
                $"/api/replenishment-requests/{id}/fulfill",
                request);

        response.EnsureSuccessStatusCode();
    }
}