
namespace iMissMyStreamer.Services.Persist
{
    public interface IPersistService
    {
        bool? GetBoolean(string key);
        DateTime? GetDateTime(string key);
        double? GetNumber(string key);
        string? GetString(string key);
        void SetBoolean(string key, bool value);
        void SetDateTime(string key, DateTime value);
        void SetNumber(string key, double value);
        void SetString(string key, string value);
    }
}