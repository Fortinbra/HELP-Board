using HelpBoard.Contracts;

namespace HelpBoard.Abstractions.Services;

/// <summary>The result of attempting to update a dashboard.</summary>
public sealed record DashboardUpdateResult(DashboardResponse? Dashboard, bool Conflict);
