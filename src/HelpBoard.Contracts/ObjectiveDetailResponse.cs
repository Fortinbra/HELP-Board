namespace HelpBoard.Contracts;

/// <summary>Represents a work item detail with direct children in the shared board/work-item model.</summary>
public sealed record BoardWorkItemDetailResponse(
    BoardWorkItemSummaryResponse WorkItem,
    IReadOnlyList<BoardWorkItemSummaryResponse> Children);

