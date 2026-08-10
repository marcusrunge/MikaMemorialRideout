using System.Net;
using System.Net.Http.Json;
using MarcusRunge.MikaMemorialRideout.Contracts;

namespace MarcusRunge.MikaMemorialRideout.Services;

public sealed class RideoutApiClient : IRideoutApiClient
{
    private readonly HttpClient _httpClient;

    public RideoutApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PlanningStatusResponse> GetPlanningStatusAsync(CancellationToken cancellationToken) =>
        await _httpClient.GetFromJsonAsync<PlanningStatusResponse>("api/planning-status", cancellationToken)
        ?? throw new InvalidOperationException("Die Statusinformationen konnten nicht gelesen werden.");

    public async Task<PublicSummaryResponse> GetPublicSummaryAsync(CancellationToken cancellationToken) =>
        await _httpClient.GetFromJsonAsync<PublicSummaryResponse>("api/public-summary", cancellationToken)
        ?? throw new InvalidOperationException("Der Anmeldestand konnte nicht gelesen werden.");

    public async Task<CreateRegistrationResponse> CreateRegistrationAsync(CreateRegistrationRequest request, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/registrations", request, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<CreateRegistrationResponse>(cancellationToken)
            ?? new CreateRegistrationResponse("error", "Die Antwort der Anmeldung konnte nicht gelesen werden.");

        if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Conflict)
            return result;

        throw new HttpRequestException(result.Message, null, response.StatusCode);
    }

    public async Task<UpdatePlanningStatusResponse> UpdatePlanningStatusAsync(
        string key,
        UpdatePlanningStatusRequest request,
        string adminCode,
        CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Put, $"api/planning-status/{Uri.EscapeDataString(key)}")
        {
            Content = JsonContent.Create(request)
        };

        message.Headers.Add("X-Admin-Code", adminCode);

        using var response = await _httpClient.SendAsync(message, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<UpdatePlanningStatusResponse>(cancellationToken)
            ?? new UpdatePlanningStatusResponse("error", "Die Antwort der Statusänderung konnte nicht gelesen werden.");

        if (response.IsSuccessStatusCode || response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.NotFound)
            return result;

        throw new HttpRequestException(result.Message, null, response.StatusCode);
    }
}
