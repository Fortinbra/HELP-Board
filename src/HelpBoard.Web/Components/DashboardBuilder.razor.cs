using System.Net;
using System.Text.Json;
using HelpBoard.Contracts;
using HelpBoard.Web.Services;
using Microsoft.AspNetCore.Components;

namespace HelpBoard.Web.Components;

public partial class DashboardBuilder : IDisposable
{
    private readonly CancellationTokenSource _cancellation = new();
    private List<DashboardResponse> _dashboards = [];
    private DashboardResponse? _dashboard;
    private string _newName = string.Empty;
    private string _widgetType = "Line";
    private string _status = string.Empty;
    private string? _loadError;
    private string? _saveError;
    private bool _loading = true;
    private bool _saving;
    private bool _conflict;
    private bool ControlsDisabled => _saving || _saveError is not null;

    [Inject] private DashboardApiClient Api { get; set; } = null!;
    [Inject] private ILogger<DashboardBuilder> Logger { get; set; } = null!;

    protected override Task OnInitializedAsync() => LoadAsync();

    private async Task LoadAsync()
    {
        _loading = true;
        _loadError = null;
        try
        {
            _dashboards = (await Api.GetAllAsync(_cancellation.Token)).ToList();
            _dashboard = _dashboards.FirstOrDefault();
        }
        catch (Exception exception) when (IsRequestFailure(exception))
        {
            Logger.LogError(exception, "Unable to load shared dashboards.");
            _loadError = "Dashboards could not be loaded. Please try again.";
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task CreateAsync()
    {
        if (ControlsDisabled || string.IsNullOrWhiteSpace(_newName))
        {
            return;
        }

        _saving = true;
        try
        {
            var created = await Api.CreateAsync(_newName.Trim(), _cancellation.Token);
            _dashboards.Add(created);
            _dashboard = created;
            _newName = string.Empty;
            _status = "Dashboard created.";
        }
        catch (Exception exception) when (IsRequestFailure(exception))
        {
            Logger.LogError(exception, "Unable to create a dashboard.");
            _status = "Dashboard could not be created. Please try again.";
        }
        finally
        {
            _saving = false;
        }
    }

    private Task SelectAsync(ChangeEventArgs args)
    {
        if (!ControlsDisabled && Guid.TryParse(args.Value?.ToString(), out var id))
        {
            _dashboard = _dashboards.FirstOrDefault(item => item.Id == id);
            _status = string.Empty;
        }

        return Task.CompletedTask;
    }

    private Task RenameAsync(ChangeEventArgs args)
    {
        var name = args.Value?.ToString()?.Trim();
        if (_dashboard is null || ControlsDisabled || string.IsNullOrWhiteSpace(name))
        {
            _status = "A dashboard name is required.";
            return Task.CompletedTask;
        }

        _dashboard = _dashboard with { Name = name };
        return SaveAsync();
    }

    private Task AddWidgetAsync()
    {
        if (_dashboard is null || ControlsDisabled)
        {
            return Task.CompletedTask;
        }

        var widget = new DashboardWidget(Guid.NewGuid(), _widgetType, WidgetTypeLabel(_widgetType), "", 0, 0, 2, 2);
        for (var row = 0; row <= DashboardLayout.MaximumRow - widget.Height; row++)
        {
            for (var column = 0; column <= DashboardLayout.Columns - widget.Width; column++)
            {
                var candidate = widget with { Column = column, Row = row };
                if (_dashboard.Widgets.All(existing => !DashboardLayout.Overlaps(existing, candidate)))
                {
                    return ApplyWidgetsAsync([.. _dashboard.Widgets, candidate]);
                }
            }
        }

        _status = "There is no room for another widget.";
        return Task.CompletedTask;
    }

    private Task MoveAsync(DashboardWidget widget, int columns, int rows)
        => PlaceAsync(widget with { Column = widget.Column + columns, Row = widget.Row + rows });

    private Task ResizeAsync(DashboardWidget widget, int width, int height)
        => PlaceAsync(widget with { Width = widget.Width + width, Height = widget.Height + height });

    private Task SetTitleAsync(DashboardWidget widget, ChangeEventArgs args)
    {
        var title = args.Value?.ToString()?.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            _status = "A widget title is required.";
            return Task.CompletedTask;
        }

        return PlaceAsync(widget with { Title = title });
    }

    private Task SetContentAsync(DashboardWidget widget, ChangeEventArgs args)
        => PlaceAsync(widget with { Content = args.Value?.ToString() ?? string.Empty });

    private Task RemoveAsync(DashboardWidget widget)
        => _dashboard is null ? Task.CompletedTask
            : ApplyWidgetsAsync(_dashboard.Widgets.Where(item => item.Id != widget.Id).ToList());

    private Task PlaceAsync(DashboardWidget widget)
    {
        if (_dashboard is null || ControlsDisabled)
        {
            return Task.CompletedTask;
        }

        var widgets = DashboardGrid.Place(_dashboard.Widgets, widget);
        if (widgets is null)
        {
            _status = "This change does not fit within the dashboard grid.";
            return Task.CompletedTask;
        }

        return ApplyWidgetsAsync(widgets);
    }

    private Task ApplyWidgetsAsync(IReadOnlyList<DashboardWidget> widgets)
    {
        if (_dashboard is null || ControlsDisabled)
        {
            return Task.CompletedTask;
        }

        if (!DashboardLayout.IsValid(widgets))
        {
            _status = $"A dashboard can have at most {DashboardLayout.MaximumWidgets} widgets.";
            return Task.CompletedTask;
        }

        _dashboard = _dashboard with { Widgets = widgets };
        return SaveAsync();
    }

    private Task RetrySaveAsync()
    {
        _saveError = null;
        return SaveAsync();
    }

    private async Task SaveAsync()
    {
        if (_dashboard is null || _saving)
        {
            return;
        }

        _saving = true;
        _status = "Saving...";
        try
        {
            _dashboard = await Api.SaveAsync(_dashboard, _cancellation.Token);
            ReplaceDashboard(_dashboard);
            _status = "Changes saved.";
        }
        catch (HttpRequestException exception) when (exception.StatusCode == HttpStatusCode.Conflict)
        {
            Logger.LogWarning(exception, "Dashboard {DashboardId} has a concurrent edit.", _dashboard.Id);
            _conflict = true;
            _saveError = "Someone else changed this dashboard. Your local changes have not been saved; copy any notes before reloading.";
            _status = "Save conflict.";
        }
        catch (Exception exception) when (IsRequestFailure(exception))
        {
            Logger.LogError(exception, "Unable to save dashboard {DashboardId}.", _dashboard.Id);
            _saveError = "Changes could not be saved. Your local changes are still shown.";
            _status = "Save failed.";
        }
        finally
        {
            _saving = false;
        }
    }

    private async Task ReloadDashboardAsync()
    {
        if (_dashboard is null || _saving)
        {
            return;
        }

        _saving = true;
        try
        {
            _dashboard = await Api.GetAsync(_dashboard.Id, _cancellation.Token);
            ReplaceDashboard(_dashboard);
            _saveError = null;
            _conflict = false;
            _status = "Dashboard reloaded.";
        }
        catch (Exception exception) when (IsRequestFailure(exception))
        {
            Logger.LogError(exception, "Unable to reload dashboard {DashboardId}.", _dashboard.Id);
            _saveError = "Dashboard could not be reloaded. Your local changes are still shown.";
        }
        finally
        {
            _saving = false;
        }
    }

    private void ReplaceDashboard(DashboardResponse dashboard)
    {
        var index = _dashboards.FindIndex(item => item.Id == dashboard.Id);
        if (index >= 0)
        {
            _dashboards[index] = dashboard;
        }
    }

    private bool IsRequestFailure(Exception exception)
        => exception is HttpRequestException or JsonException or InvalidOperationException
           || exception is TaskCanceledException && !_cancellation.IsCancellationRequested;

    private static string GridStyle(DashboardWidget widget)
        => $"grid-column: {widget.Column + 1} / span {widget.Width}; grid-row: {widget.Row + 1} / span {widget.Height};";

    private static string WidgetTypeLabel(string type) => type switch
    {
        "Line" => "Line chart",
        "Bar" => "Bar chart",
        "Table" => "Table",
        "Text" => "Text / notes",
        _ => "Widget"
    };

    public void Dispose()
    {
        _cancellation.Cancel();
        _cancellation.Dispose();
    }
}
