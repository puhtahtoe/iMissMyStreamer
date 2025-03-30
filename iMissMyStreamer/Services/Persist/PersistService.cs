using System.IO;
using System.Text.Json;

namespace iMissMyStreamer.Services.Persist;

public class PersistService : IPersistService
{

#if DEBUG
    string PersistFilePath = @"D:\imlMnt\persist.json";
#else
    string PersistFilePath = "/data/persist.json";
#endif

    private readonly IConfiguration config;
    private readonly ILogger<PersistService> logger;

    private Dictionary<string, JsonElement>? data;

    public PersistService(IConfiguration _config, ILogger<PersistService> _logger)
    {
        config = _config;
        logger = _logger;

        if (config["debug"] is not null && config.GetValue<bool>("debug"))
        {
            PersistFilePath = PersistFilePath.Replace("persist.json", "debug_persist.json");
        }

        LoadPersistedData();

        if (data is null) { data = new Dictionary<string, JsonElement>(); }


    }

    private void LoadPersistedData()
    {
        if (File.Exists(PersistFilePath))
        {
            try
            {
                var json = File.ReadAllText(PersistFilePath);
                data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json) ?? new Dictionary<string, JsonElement>();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to load persisted data");
            }
        }
        else
        {
            data = new Dictionary<string, JsonElement>();
        }
    }

    private void SaveToFile()
    {
        try
        {
            var ops = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(data, ops);
            File.WriteAllText(PersistFilePath, json);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to save persisted data");
        }
    }
    public void SetBoolean(string key, bool value)
    {
        data![key] = JsonDocument.Parse(value ? "true" : "false").RootElement;
        SaveToFile();
    }

    public void SetDateTime(string key, DateTime value)
    {
        data![key] = JsonDocument.Parse($"\"{value:O}\"").RootElement;
        SaveToFile();
    }
    public void SetNumber(string key, double value)
    {
        data![key] = JsonDocument.Parse(value.ToString(System.Globalization.CultureInfo.InvariantCulture)).RootElement;
        SaveToFile();
    }

    public void SetString(string key, string value)
    {
        data![key] = JsonDocument.Parse($"\"{value}\"").RootElement;
        SaveToFile();
    }


    public bool? GetBoolean(string key)
    {
        return data!.TryGetValue(key, out var value) && value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False
            ? value.GetBoolean()
            : null;
    }

    public DateTime? GetDateTime(string key)
    {
        return data!.TryGetValue(key, out var value) && value.ValueKind == JsonValueKind.String
            && DateTime.TryParse(value.GetString(), null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt)
            ? dt
            : null;
    }
    public double? GetNumber(string key)
    {
        return data!.TryGetValue(key, out var value) && value.ValueKind == JsonValueKind.Number
            ? value.GetDouble()
            : null;
    }

    public string? GetString(string key)
    {
        return data!.TryGetValue(key, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }


}
