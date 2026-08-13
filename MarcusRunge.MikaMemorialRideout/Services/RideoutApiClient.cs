using System.Net;
using System.Net.Http.Json;
using MarcusRunge.MikaMemorialRideout.Contracts;

namespace MarcusRunge.MikaMemorialRideout.Services;

public sealed class RideoutApiClient : IRideoutApiClient
{
    private const string AdminCodeHeader = "X-Admin-Code";
    private const string ManagementApiBasePath = "api/rideout-management";
    private readonly HttpClient _httpClient;

    public RideoutApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<RegistrationStateResponse> GetRegistrationStateAsync(CancellationToken cancellationToken) => await _httpClient.GetFromJsonAsync<RegistrationStateResponse>("api/registration-state", cancellationToken) ?? throw new InvalidOperationException("Der Anmeldestatus konnte nicht gelesen werden.");
    public async Task<RegistrationStateResponse> UpdateRegistrationStateAsync(bool isOpen, string adminCode, CancellationToken cancellationToken)
    {
        using var message = CreateAdminRequest(HttpMethod.Put, $"{ManagementApiBasePath}/registration-state", adminCode, JsonContent.Create(new UpdateRegistrationStateRequest(isOpen)));
        using var response = await _httpClient.SendAsync(message, cancellationToken);
        return await ReadResponseAsync(response, new RegistrationStateResponse(isOpen, null), cancellationToken);
    }
    public async Task<AdminRegistrationMutationResponse> SetRegistrationRespondedAsync(string id, string version, bool isResponded, string adminCode, CancellationToken cancellationToken)
    {
        using var message = CreateAdminRequest(HttpMethod.Put, $"{ManagementApiBasePath}/registrations/{Uri.EscapeDataString(id)}/response-state", adminCode, JsonContent.Create(new AdminResponseStateRequest(version, isResponded)));
        using var response = await _httpClient.SendAsync(message, cancellationToken);
        return await ReadResponseAsync(response, new AdminRegistrationMutationResponse("error", "Die Antwortmarkierung konnte nicht gespeichert werden."), cancellationToken);
    }
    public async Task<AdminOperationResponse> AnonymizeRegistrationsAsync(string adminCode, CancellationToken cancellationToken)
    {
        using var message = CreateAdminRequest(HttpMethod.Post, $"{ManagementApiBasePath}/registrations/anonymize", adminCode);
        using var response = await _httpClient.SendAsync(message, cancellationToken);
        return await ReadResponseAsync(response, new AdminOperationResponse("error", "Die Anonymisierung konnte nicht ausgeführt werden."), cancellationToken);
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
        using var message = CreateAdminRequest(HttpMethod.Put, $"api/planning-status/{Uri.EscapeDataString(key)}", adminCode, JsonContent.Create(request));
        using var response = await _httpClient.SendAsync(message, cancellationToken);
        return await ReadResponseAsync(response, new UpdatePlanningStatusResponse("error", "Die Antwort der Statusänderung konnte nicht gelesen werden."), cancellationToken);
    }

    public async Task<AdminVerificationResponse> VerifyAdminCodeAsync(string adminCode, CancellationToken cancellationToken)
    {
        using var message = CreateAdminRequest(HttpMethod.Post, $"{ManagementApiBasePath}/verify", adminCode);
        using var response = await _httpClient.SendAsync(message, cancellationToken);
        return await ReadResponseAsync(response, new AdminVerificationResponse("error", "Die Autorisierungsantwort konnte nicht gelesen werden."), cancellationToken);
    }

    public async Task<AdminRegistrationsResponse> GetAdminRegistrationsAsync(string adminCode, CancellationToken cancellationToken)
    {
        using var message = CreateAdminRequest(HttpMethod.Get, $"{ManagementApiBasePath}/registrations", adminCode);
        using var response = await _httpClient.SendAsync(message, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException("Die Anmeldungen konnten nicht geladen werden.", null, response.StatusCode);

        return await response.Content.ReadFromJsonAsync<AdminRegistrationsResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Die Anmeldungen konnten nicht gelesen werden.");
    }

    public async Task<AdminRegistrationMutationResponse> UpdateAdminRegistrationAsync(
        string id,
        AdminRegistrationUpdateRequest request,
        string adminCode,
        CancellationToken cancellationToken)
    {
        using var message = CreateAdminRequest(HttpMethod.Put, $"{ManagementApiBasePath}/registrations/{Uri.EscapeDataString(id)}", adminCode, JsonContent.Create(request));
        using var response = await _httpClient.SendAsync(message, cancellationToken);
        return await ReadResponseAsync(response, new AdminRegistrationMutationResponse("error", "Die Antwort konnte nicht gelesen werden."), cancellationToken);
    }

    public async Task<AdminRegistrationMutationResponse> DeleteAdminRegistrationAsync(
        string id,
        string version,
        string adminCode,
        CancellationToken cancellationToken)
    {
        var uri = $"{ManagementApiBasePath}/registrations/{Uri.EscapeDataString(id)}?version={Uri.EscapeDataString(version)}";
        using var message = CreateAdminRequest(HttpMethod.Delete, uri, adminCode);
        using var response = await _httpClient.SendAsync(message, cancellationToken);
        return await ReadResponseAsync(response, new AdminRegistrationMutationResponse("error", "Die Antwort konnte nicht gelesen werden."), cancellationToken);
    }

    private static HttpRequestMessage CreateAdminRequest(HttpMethod method, string uri, string adminCode, HttpContent? content = null)
    {
        var message = new HttpRequestMessage(method, uri) { Content = content };
        message.Headers.Add(AdminCodeHeader, adminCode);
        return message;
    }

    private static async Task<T> ReadResponseAsync<T>(HttpResponseMessage response, T fallback, CancellationToken cancellationToken)
    {
        var result = await response.Content.ReadFromJsonAsync<T>(cancellationToken) ?? fallback;

        if (response.IsSuccessStatusCode || response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized or HttpStatusCode.NotFound or HttpStatusCode.Conflict)
            return result;

        throw new HttpRequestException("Die API-Anfrage ist fehlgeschlagen.", null, response.StatusCode);
    }
}
