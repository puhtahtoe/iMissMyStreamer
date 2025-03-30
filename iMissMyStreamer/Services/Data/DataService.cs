


using Microsoft.Extensions.Caching.Memory;

using iMissMyStreamer.Services.Persist;

namespace iMissMyStreamer.Services.Data;

public class DataService : IDataService
{
    private IPersistService persistService;
    private IMemoryCache memCache;

    public event Action? OnStatusChanged;
    public event Action? OnTitleChanged;
    public event Action? OnGameChanged;
    public event Action? OnNextStreamTimeChanged;
    public event Action? OnLastStreamTimeChanged;

    public DataService(IPersistService _persistService, IMemoryCache _memCache)
    {
        persistService = _persistService;
        memCache = _memCache;
    }

    public void SetStreamerStatus(bool isLive, TimeSpan? lifetime = null)
    {
        if (lifetime.HasValue)
        {
            memCache.Set("IsLive", isLive, lifetime.Value);
        }
        else
        {
            memCache.Set("IsLive", isLive);
        }

        persistService.SetBoolean("IsLive", isLive);

        OnStatusChanged?.Invoke();
    }

    public (bool gotData, bool isLive) GetStreamerStatus()
    {
        if (memCache.TryGetValue("IsLive", out bool isLive))
        {
            return (true, isLive);
        }

        var persist = persistService.GetBoolean("IsLive");

        if (persist.HasValue)
        {
            memCache.Set("IsLive", persist.Value);
            return (true, persist.Value);
        }

        return (false, false);
    }


    public void SetStreamTitle(string title, TimeSpan? lifetime = null)
    {
        if (lifetime.HasValue)
        {
            memCache.Set("StreamTitle", title, lifetime.Value);
        }
        else
        {
            memCache.Set("StreamTitle", title);
        }

        persistService.SetString("StreamTitle", title);

        OnTitleChanged?.Invoke();
    }

    public (bool gotData, string streamTitle) GetStreamTitle()
    {
        if (memCache.TryGetValue("StreamTitle", out string? streamTitle))
        {
            if (streamTitle is not null)
            {
                return (true, streamTitle);
            }
        }

        string? temp = persistService.GetString("StreamTitle");

        if (temp is not null)
        {
            memCache.Set("StreamTitle", temp);
            return (true, temp);
        }

        return (false, "");
    }

    public void SetStreamGame(string game, TimeSpan? lifetime = null)
    {
        if (lifetime.HasValue)
        {
            memCache.Set("StreamGame", game, lifetime.Value);
        }
        else
        {
            memCache.Set("StreamGame", game);
        }

        persistService.SetString("StreamGame", game);

        OnGameChanged?.Invoke();
    }

    public (bool gotData, string streamGame) GetStreamGame()
    {
        if (memCache.TryGetValue("StreamGame", out string? streamGame))
        {
            if (streamGame is not null)
            {
                return (true, streamGame);
            }
        }

        string? temp = persistService.GetString("StreamGame");

        if (temp is not null)
        {
            memCache.Set("StreamGame", temp);
            return (true, temp);
        }

        return (false, "");
    }


    public void SetNextStreamTime(DateTime nextStreamTime, TimeSpan? lifetime = null)
    {
        if (lifetime.HasValue)
        {
            memCache.Set("NextStreamTime", nextStreamTime, lifetime.Value);
        }
        else
        {
            memCache.Set("NextStreamTime", nextStreamTime);
        }

        persistService.SetDateTime("NextStreamTime", nextStreamTime);

        OnNextStreamTimeChanged?.Invoke();
    }

    public (bool gotData, DateTime nextStreamTime) GetNextStreamTime()
    {
        if (memCache.TryGetValue("NextStreamTime", out DateTime? nextStreamTime))
        {
            if (nextStreamTime.HasValue)
            {
                return (true, nextStreamTime.Value);
            }
        }

        DateTime? temp = persistService.GetDateTime("NextStreamTime");

        if (temp.HasValue)
        {
            memCache.Set("NextStreamTime", temp.Value);
            return (true, temp.Value);
        }

        return (false, DateTime.MaxValue);
    }


    public void SetLastStreamTime(DateTime lastStreamTime, TimeSpan? lifetime = null)
    {
        if (lifetime.HasValue)
        {
            memCache.Set("LastStreamTime", lastStreamTime, lifetime.Value);
        }
        else
        {
            memCache.Set("LastStreamTime", lastStreamTime);
        }

        persistService.SetDateTime("LastStreamTime", lastStreamTime);

        OnLastStreamTimeChanged?.Invoke();
    }

    public (bool gotData, DateTime lastStreamTime) GetLastStreamTime()
    {
        if (memCache.TryGetValue("LastStreamTime", out DateTime? lastStreamTime))
        {
            if (lastStreamTime.HasValue)
            {
                return (true, lastStreamTime.Value);
            }
        }

        DateTime? temp = persistService.GetDateTime("LastStreamTime");

        if (temp.HasValue)
        {
            memCache.Set("LastStreamTime", temp.Value);
            return (true, temp.Value);
        }

        return (false, DateTime.MinValue);
    }

    public void SetOnlineWebhookVerificationSecret(string secret, TimeSpan? lifetime = null)
    {
        if (lifetime.HasValue)
        {
            memCache.Set("OnlineWebhookVerificationSecret", secret, lifetime.Value);
        }
        else
        {
            memCache.Set("OnlineWebhookVerificationSecret", secret);
        }

        persistService.SetString("OnlineWebhookVerificationSecret", secret);
    }

    public (bool gotData, string secret) GetOnlineWebhookVerificationSecret()
    {
        if (memCache.TryGetValue("OnlineWebhookVerificationSecret", out string? secret))
        {
            if (secret is not null)
            {
                return (true, secret);
            }
        }

        string? temp = persistService.GetString("OnlineWebhookVerificationSecret");

        if (temp is not null)
        {
            memCache.Set("OnlineWebhookVerificationSecret", temp);
            return (true, temp);
        }

        return (false, "");
    }

    public void SetOnlineWebhookID(string id)
    {
        memCache.Set("OnlineWebhookId", id);

        persistService.SetString("OnlineWebhookId", id);
    }

    public (bool gotData, string id) GetOnlineWebhookID()
    {
        if (memCache.TryGetValue("OnlineWebhookId", out string? id))
        {
            if (id is not null)
            {
                return (true, id);
            }
        }

        string? temp = persistService.GetString("OnlineWebhookId");

        if (temp is not null)
        {
            memCache.Set("OnlineWebhookId", temp);
            return (true, temp);
        }

        return (false, "");
    }

    public void SetOfflineWebhookVerificationSecret(string secret, TimeSpan? lifetime = null)
    {
        if (lifetime.HasValue)
        {
            memCache.Set("OfflineWebhookVerificationSecret", secret, lifetime.Value);
        }
        else
        {
            memCache.Set("OfflineWebhookVerificationSecret", secret);
        }

        persistService.SetString("OfflineWebhookVerificationSecret", secret);
    }

    public (bool gotData, string secret) GetOfflineWebhookVerificationSecret()
    {
        if (memCache.TryGetValue("OfflineWebhookVerificationSecret", out string? secret))
        {
            if (secret is not null)
            {
                return (true, secret);
            }
        }

        string? temp = persistService.GetString("OfflineWebhookVerificationSecret");

        if (temp is not null)
        {
            memCache.Set("OfflineWebhookVerificationSecret", temp);
            return (true, temp);
        }

        return (false, "");
    }

    public void SetOfflineWebhookID(string id)
    {
        memCache.Set("OfflineWebhookId", id);

        persistService.SetString("OfflineWebhookId", id);
    }

    public (bool gotData, string id) GetOfflineWebhookID()
    {
        if (memCache.TryGetValue("OfflineWebhookId", out string? id))
        {
            if (id is not null)
            {
                return (true, id);
            }
        }

        string? temp = persistService.GetString("OfflineWebhookId");

        if (temp is not null)
        {
            memCache.Set("OfflineWebhookId", temp);
            return (true, temp);
        }

        return (false, "");
    }
}
