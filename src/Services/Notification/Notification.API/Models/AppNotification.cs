namespace Notification.API.Models;

public record AppNotification(
    string Id,
    string UserId,
    string Icon,
    string Text,
    string CreatedAt,
    bool Unread,
    string? Link = null
);
