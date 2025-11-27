using Microsoft.FluentUI.AspNetCore.Components;

namespace PgmqAdminUI.Components.UI;

public class NotificationService
{
    public event Action<NotificationMessage>? OnNotification;

    public void ShowSuccess(string message) =>
        OnNotification?.Invoke(new NotificationMessage(MessageIntent.Success, message));

    public void ShowError(string message) =>
        OnNotification?.Invoke(new NotificationMessage(MessageIntent.Error, message));

    public void ShowWarning(string message) =>
        OnNotification?.Invoke(new NotificationMessage(MessageIntent.Warning, message));

    public void ShowInfo(string message) =>
        OnNotification?.Invoke(new NotificationMessage(MessageIntent.Info, message));
}

public record NotificationMessage(MessageIntent Intent, string Message, int DurationMs = 5000);
