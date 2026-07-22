
namespace iMissMyStreamer.Services.Data
{
    public interface IDataService
    {
        event Action? OnGameChanged;
        event Action? OnLastStreamTimeChanged;
        event Action? OnNextStreamTimeChanged;
        event Action? OnStatusChanged;
        event Action? OnTitleChanged;

        (bool gotData, DateTime lastStreamTime) GetLastStreamTime();
        (bool gotData, DateTime nextStreamTime) GetNextStreamTime();
        (bool gotData, string id) GetOfflineWebhookID();
        (bool gotData, bool isLive) GetStreamerStatus();
        (bool gotData, string streamGame) GetStreamGame();
        (bool gotData, string streamTitle) GetStreamTitle();
        (bool gotData, string featuredClips) GetFeaturedClips();

        (bool gotData, string secret) GetOfflineWebhookVerificationSecret();
        (bool gotData, string id) GetOnlineWebhookID();
        (bool gotData, string secret) GetOnlineWebhookVerificationSecret();

        void SetLastStreamTime(DateTime lastStreamTime, TimeSpan? lifetime = null);
        void SetNextStreamTime(DateTime nextStreamTime, TimeSpan? lifetime = null);
        void SetStreamerStatus(bool isLive, TimeSpan? lifetime = null);
        void SetStreamGame(string game, TimeSpan? lifetime = null);
        void SetStreamTitle(string title, TimeSpan? lifetime = null);
        void SetFeaturedClips(string clips, TimeSpan? lifetime = null);

        void SetOfflineWebhookID(string id);
        void SetOfflineWebhookVerificationSecret(string secret, TimeSpan? lifetime = null);
        void SetOnlineWebhookID(string id);
        void SetOnlineWebhookVerificationSecret(string secret, TimeSpan? lifetime = null);
    }
}