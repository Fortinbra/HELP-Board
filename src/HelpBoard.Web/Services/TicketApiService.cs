using HelpBoard.Contracts;
using System.Net.Http.Json;

namespace HelpBoard.Web.Services;

/// <summary>HTTP client service for communicating with the HELP-Board API tickets endpoint.</summary>
public sealed class TicketApiService(HttpClient httpClient)
{
    private const string TicketsRoute = "api/tickets";
    private const string DashboardSummaryRoute = "api/dashboard/summary";

    private static readonly DashboardSummaryCardModel[] EmptyCards =
    [
        new(
            Key: "total-work-items",
            Title: "Total work items",
            ValueText: "0",
            SupportingText: "No work items are available yet.",
            IconName: "ticket",
            IconToneClass: "hb-stat-card__icon--primary",
            StatusText: "No data",
            StatusIconName: "inbox",
            StatusBadgeClass: "hb-badge--neutral",
            DrillDownRoute: "tickets",
            DrillDownText: "Open tickets"),
        new(
            Key: "active-work",
            Title: "Active work",
            ValueText: "0",
            SupportingText: "No active work is in progress.",
            IconName: "activity",
            IconToneClass: "hb-stat-card__icon--warning",
            StatusText: "On track",
            StatusIconName: "check-circle-2",
            StatusBadgeClass: "hb-badge--success",
            DrillDownRoute: "tickets/active",
            DrillDownText: "View active work"),
        new(
            Key: "strategic-plan",
            Title: "Strategic plan",
            ValueText: "0%",
            SupportingText: "No strategic milestones are tracked yet.",
            IconName: "panel-left",
            IconToneClass: "hb-stat-card__icon--primary",
            StatusText: "No milestones",
            StatusIconName: "clock-3",
            StatusBadgeClass: "hb-badge--neutral",
            DrillDownRoute: "strategic-plan",
            DrillDownText: "View strategic plan"),
        new(
            Key: "project-boards",
            Title: "Project boards",
            ValueText: "0",
            SupportingText: "No board summary data is available yet.",
            IconName: "menu",
            IconToneClass: "hb-stat-card__icon--danger",
            StatusText: "Awaiting data",
            StatusIconName: "clock-3",
            StatusBadgeClass: "hb-badge--warning",
            DrillDownRoute: "boards",
            DrillDownText: "View project boards")
    ];

    /// <summary>Retrieves all tickets from the API.</summary>
    public async Task<IReadOnlyList<TicketResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tickets = await httpClient
            .GetFromJsonAsync<List<TicketResponse>>(TicketsRoute, cancellationToken)
            .ConfigureAwait(false);

        return tickets ?? [];
    }

    /// <summary>Retrieves dashboard summary cards from the API when available, with a ticket-based fallback.</summary>
    public async Task<DashboardSummaryModel> GetDashboardSummaryAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var summary = await httpClient
                .GetFromJsonAsync<DashboardSummaryApiContract>(DashboardSummaryRoute, cancellationToken)
                .ConfigureAwait(false);

            if (summary is not null && summary.Cards is not null && summary.Cards.Count > 0)
            {
                return new DashboardSummaryModel(
                    summary.Cards.Select(MapApiCard).ToArray(),
                    summary.LastUpdatedAt ?? DateTimeOffset.UtcNow);
            }
        }
        catch (HttpRequestException)
        {
            // Fallback is expected until the dashboard summary endpoint is delivered.
        }
        catch (NotSupportedException)
        {
            // Fallback is expected when the endpoint shape is not available yet.
        }

        return await BuildFallbackSummaryAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Retrieves a single ticket by identifier.</summary>
    public async Task<TicketResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await httpClient
            .GetFromJsonAsync<TicketResponse>($"{TicketsRoute}/{id}", cancellationToken)
            .ConfigureAwait(false);

    /// <summary>Creates a new ticket.</summary>
    public async Task<TicketResponse?> CreateAsync(CreateTicketRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient
            .PostAsJsonAsync(TicketsRoute, request, cancellationToken)
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<TicketResponse>(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>Updates an existing ticket.</summary>
    public async Task<TicketResponse?> UpdateAsync(Guid id, UpdateTicketRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient
            .PutAsJsonAsync($"{TicketsRoute}/{id}", request, cancellationToken)
            .ConfigureAwait(false);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<TicketResponse>(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>Deletes a ticket by identifier.</summary>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient
            .DeleteAsync($"{TicketsRoute}/{id}", cancellationToken)
            .ConfigureAwait(false);

        return response.IsSuccessStatusCode;
    }

    private async Task<DashboardSummaryModel> BuildFallbackSummaryAsync(CancellationToken cancellationToken)
    {
        var tickets = await GetAllAsync(cancellationToken).ConfigureAwait(false);

        if (tickets.Count == 0)
        {
            return new DashboardSummaryModel(EmptyCards, DateTimeOffset.UtcNow);
        }

        var totalTickets = tickets.Count;
        var activeTickets = tickets.Count(ticket => ticket.Status is TicketStatus.Open or TicketStatus.InProgress);
        var resolvedTickets = tickets.Count(ticket => ticket.Status is TicketStatus.Resolved or TicketStatus.Closed);
        var criticalTickets = tickets.Count(ticket => ticket.Priority == TicketPriority.Critical);
        var completion = totalTickets == 0
            ? 0
            : (int)Math.Round((double)resolvedTickets / totalTickets * 100, MidpointRounding.AwayFromZero);

        var cards = new[]
        {
            new DashboardSummaryCardModel(
                Key: "total-work-items",
                Title: "Total work items",
                ValueText: totalTickets.ToString(),
                SupportingText: $"{resolvedTickets} resolved or closed",
                IconName: "ticket",
                IconToneClass: "hb-stat-card__icon--primary",
                StatusText: "Live",
                StatusIconName: "activity",
                StatusBadgeClass: "hb-badge--info",
                DrillDownRoute: "tickets",
                DrillDownText: "Open tickets"),
            new DashboardSummaryCardModel(
                Key: "active-work",
                Title: "Active work",
                ValueText: activeTickets.ToString(),
                SupportingText: $"{criticalTickets} critical priority items",
                IconName: "activity",
                IconToneClass: "hb-stat-card__icon--warning",
                StatusText: activeTickets == 0 ? "On track" : "Needs review",
                StatusIconName: activeTickets == 0 ? "check-circle-2" : "alert-circle",
                StatusBadgeClass: activeTickets == 0 ? "hb-badge--success" : "hb-badge--warning",
                DrillDownRoute: "tickets/active",
                DrillDownText: "View active work"),
            new DashboardSummaryCardModel(
                Key: "strategic-plan",
                Title: "Strategic plan",
                ValueText: $"{completion}%",
                SupportingText: "Derived from current ticket completion",
                IconName: "panel-left",
                IconToneClass: "hb-stat-card__icon--primary",
                StatusText: completion >= 70 ? "Healthy" : "Watch",
                StatusIconName: completion >= 70 ? "check-circle-2" : "clock-3",
                StatusBadgeClass: completion >= 70 ? "hb-badge--success" : "hb-badge--warning",
                DrillDownRoute: "strategic-plan",
                DrillDownText: "View strategic plan"),
            new DashboardSummaryCardModel(
                Key: "project-boards",
                Title: "Project boards",
                ValueText: criticalTickets.ToString(),
                SupportingText: "Critical items requiring board attention",
                IconName: "menu",
                IconToneClass: "hb-stat-card__icon--danger",
                StatusText: criticalTickets == 0 ? "Stable" : "Escalation",
                StatusIconName: criticalTickets == 0 ? "check-circle-2" : "alert-circle",
                StatusBadgeClass: criticalTickets == 0 ? "hb-badge--success" : "hb-badge--danger",
                DrillDownRoute: "boards",
                DrillDownText: "View project boards")
        };

        return new DashboardSummaryModel(cards, DateTimeOffset.UtcNow);
    }

    private static DashboardSummaryCardModel MapApiCard(DashboardSummaryApiCardContract source)
        => new(
            Key: source.Key,
            Title: source.Title,
            ValueText: source.ValueText,
            SupportingText: source.SupportingText,
            IconName: source.IconName,
            IconToneClass: source.IconToneClass,
            StatusText: source.StatusText,
            StatusIconName: source.StatusIconName,
            StatusBadgeClass: source.StatusBadgeClass,
            DrillDownRoute: source.DrillDownRoute,
            DrillDownText: source.DrillDownText);

    private sealed record DashboardSummaryApiContract(
        IReadOnlyList<DashboardSummaryApiCardContract>? Cards,
        DateTimeOffset? LastUpdatedAt);

    private sealed record DashboardSummaryApiCardContract(
        string Key,
        string Title,
        string ValueText,
        string SupportingText,
        string IconName,
        string IconToneClass,
        string StatusText,
        string StatusIconName,
        string StatusBadgeClass,
        string DrillDownRoute,
        string DrillDownText);
}
