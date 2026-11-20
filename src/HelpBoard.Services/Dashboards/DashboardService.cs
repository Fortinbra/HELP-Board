using System.Text.Json;
using HelpBoard.Abstractions.Domain;
using HelpBoard.Abstractions.Repositories;
using HelpBoard.Abstractions.Services;
using HelpBoard.Contracts;

namespace HelpBoard.Services.Dashboards;

internal sealed class DashboardService(IDashboardReader reader, IDashboardWriter writer) : IDashboardService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<DashboardResponse>> GetAllAsync(CancellationToken cancellationToken)
        => (await reader.GetAllAsync(cancellationToken).ConfigureAwait(false)).Select(ToResponse).ToList();

    public async Task<DashboardResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var dashboard = await reader.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        return dashboard is null ? null : ToResponse(dashboard);
    }

    public async Task<DashboardResponse> CreateAsync(CreateDashboardRequest request, CancellationToken cancellationToken)
    {
        var dashboard = new DashboardDocument
        {
            Id = Guid.NewGuid(), Name = request.Name.Trim(), Revision = Guid.NewGuid(), WidgetsJson = "[]"
        };
        await writer.AddAsync(dashboard, cancellationToken).ConfigureAwait(false);
        return ToResponse(dashboard);
    }

    public async Task<DashboardUpdateResult> UpdateAsync(
        Guid id, UpdateDashboardRequest request, CancellationToken cancellationToken)
    {
        var dashboard = await reader.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (dashboard is null)
        {
            return new(null, false);
        }

        if (dashboard.Revision != request.Revision)
        {
            return new(null, true);
        }

        dashboard.Name = request.Name.Trim();
        dashboard.Revision = Guid.NewGuid();
        dashboard.WidgetsJson = JsonSerializer.Serialize(request.Widgets, JsonOptions);
        var updated = await writer.UpdateAsync(dashboard, request.Revision, cancellationToken).ConfigureAwait(false);
        return updated ? new(ToResponse(dashboard), false) : new(null, true);
    }

    private static DashboardResponse ToResponse(DashboardDocument dashboard)
        => new(dashboard.Id, dashboard.Name, dashboard.Revision,
            JsonSerializer.Deserialize<List<DashboardWidget>>(dashboard.WidgetsJson, JsonOptions)
            ?? throw new JsonException("Dashboard widget data was empty."));
}
