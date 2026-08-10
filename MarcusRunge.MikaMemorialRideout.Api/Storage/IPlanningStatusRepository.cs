using MarcusRunge.MikaMemorialRideout.Api.Contracts;

namespace MarcusRunge.MikaMemorialRideout.Api.Storage;

internal interface IPlanningStatusRepository
{
    Task<PlanningStatusResponse> GetAllAsync(CancellationToken cancellationToken);

    Task<PlanningStatusItemResponse> UpdateAsync(
        PlanningStatusDefinition definition,
        UpdatePlanningStatusRequest request,
        CancellationToken cancellationToken);
}
