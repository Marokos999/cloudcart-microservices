using System.Collections.Concurrent;
using Notification.API.Models;

namespace Notification.API.Services;

public class NotificationStore
{
    private readonly ConcurrentDictionary<string, List<AppNotification>> _store = new();

    public void Add(string userId, AppNotification notification)
    {
        _store.AddOrUpdate(
            userId,
            _ => [notification],
            (_, list) => { lock (list) { list.Insert(0, notification); } return list; });
    }

    public void AddBroadcast(AppNotification notification) => Add("broadcast", notification);

    public IReadOnlyList<AppNotification> GetForUser(string userId)
    {
        var user = _store.TryGetValue(userId, out var ul) ? ul : [];
        var broadcast = _store.TryGetValue("broadcast", out var bl) ? bl : [];
        return user.Concat(broadcast)
            .OrderByDescending(n => n.CreatedAt)
            .Take(20)
            .ToList();
    }

    public void MarkAllRead(string userId)
    {
        foreach (var key in new[] { userId, "broadcast" })
        {
            if (!_store.TryGetValue(key, out var list)) continue;
            lock (list)
            {
                for (var i = 0; i < list.Count; i++)
                    list[i] = list[i] with { Unread = false };
            }
        }
    }
}
