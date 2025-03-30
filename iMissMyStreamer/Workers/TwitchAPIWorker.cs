
using System.Runtime.CompilerServices;
using System.Text.Json;
using iMissMyStreamer.Components;
using iMissMyStreamer.Services.Data;

namespace iMissMyStreamer.Workers;

public class TwitchAPIWorker : BackgroundService
{
    private readonly ILogger<TwitchAPIWorker> logger;
    private readonly IConfiguration config;
    private readonly IDataService dataService;
    private readonly IHttpClientFactory clientFactory;

    private string ClientID { get; set; } = "";
    private string ClientSecret { get; set; } = "";
    private string AccessToken { get; set; } = "";
    private DateTime? AccessTokenExpiration { get; set; }

    private string StreamerName { get; set; } = "";
    private string StreamerID { get; set; } = "";

    public TwitchAPIWorker(ILogger<TwitchAPIWorker> logger, IConfiguration config, IDataService dataService, IHttpClientFactory clientFactory)
    {
        this.logger = logger;
        this.config = config;
        this.dataService = dataService;
        this.clientFactory = clientFactory;


        ClientID = config.GetValue<string>("TwitchClientID") ?? "";
        ClientSecret = config.GetValue<string>("TwitchClientSecret") ?? "";
        StreamerName = config.GetValue<string>("TwitchStreamerName") ?? "";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        if (string.IsNullOrEmpty(ClientID) ||
            string.IsNullOrEmpty(ClientSecret) ||
            string.IsNullOrEmpty(StreamerName))
        {
            logger?.LogCritical("You must provide at minimum a Twitch ClientID, Twitch ClientSecret, and a streamer name.");
            return;
        }


        bool tokenRefreshFailed = false;
        try
        {
            bool firstTime = true;
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!firstTime)
                {
                    TimeSpan delay = new TimeSpan(0, 1, 0);

                    if (tokenRefreshFailed)
                    {
                        delay = new TimeSpan(0, 30, 0);
                    }

                    await Task.Delay(delay, stoppingToken);
                }


                if (!await CheckAccessToken(stoppingToken))
                {
                    logger?.Log(LogLevel.Error, "Failed to refresh access token");
                    tokenRefreshFailed = true;
                    continue;
                }
                tokenRefreshFailed = false;

                if (firstTime)
                {
                    if (!await LoadStreamerID(stoppingToken))
                    {
                        logger?.Log(LogLevel.Error, "Error getting streamer ID from name");
                        return;
                    }
                }
                firstTime = false;

                await UpdateStreamStatus(stoppingToken);
            }
        }
        catch (Exception ex)
        {
            logger?.Log(LogLevel.Error, ex, "Exception in worker");
        }

        await CleanUp();
    }

    private async Task UpdateStreamStatus(CancellationToken stoppingToken)
    {
        var streamStatus = await GetStreamStatus(stoppingToken);

        if (!streamStatus.success)
        {
            logger?.Log(LogLevel.Warning, "Failed to get stream status");
            return;
        }

        var currentStatus = dataService.GetStreamerStatus();
        if (currentStatus.gotData)
        {
            if (currentStatus.isLive != streamStatus.isLive)
            {
                if (streamStatus.isLive)
                {
                    dataService.SetStreamerStatus(true);
                }
                else
                {
                    dataService.SetLastStreamTime(DateTime.UtcNow);
                    dataService.SetStreamerStatus(false);
                }
            }
        }
        else
        {
            dataService.SetStreamerStatus(streamStatus.isLive);
        }

        if (!streamStatus.isLive) { return; }

        var currentTitle = dataService.GetStreamTitle();
        if (currentTitle.gotData)
        {
            if (!currentTitle.streamTitle.Equals(streamStatus.streamTitle))
            {
                dataService.SetStreamTitle(streamStatus.streamTitle);
            }
        }
        else
        {
            dataService.SetStreamTitle(streamStatus.streamTitle);
        }


        var currentGame = dataService.GetStreamGame();
        if (currentGame.gotData)
        {
            if (!currentGame.streamGame.Equals(streamStatus.streamGame))
            {
                dataService.SetStreamGame(streamStatus.streamGame);
            }
        }
        else
        {
            dataService.SetStreamGame(streamStatus.streamGame);
        }
    }

    private async Task<(bool success, bool isLive, string streamTitle, string streamGame)> GetStreamStatus(CancellationToken stoppingToken)
    {
        int retryCount = -1;

        while (retryCount < 5)
        {
            retryCount++;

            if (retryCount > 0)
            {
                logger?.Log(LogLevel.Error, "Error getting live status");

                TimeSpan delay = new TimeSpan(0, 0, 2 * (retryCount * retryCount));

                await Task.Delay(delay, stoppingToken);
            }

            try
            {
                using HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, $"https://api.twitch.tv/helix/streams?user_id={StreamerID}&type=live&first=1");
                //using HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, $"https://api.twitch.tv/helix/streams?first=20");
                req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);
                req.Headers.Add("Client-Id", ClientID);

                using var client = clientFactory.CreateClient();

                using var response = await client.SendAsync(req, stoppingToken);

                string respText = await response.Content.ReadAsStringAsync(stoppingToken);

                if (!response.IsSuccessStatusCode) { continue; }

                var jResp = JsonDocument.Parse(respText);
                var root = jResp.RootElement;

                var jStreamArray = root.GetProperty("data").EnumerateArray();

                if (jStreamArray.Count() > 0)
                {
                    var jStream = root.GetProperty("data").EnumerateArray().First();

                    string title = jStream.GetProperty("title").GetString() ?? "";
                    string game = jStream.GetProperty("game_name").GetString() ?? "";

                    return (true, true, title, game);
                }
                else
                {
                    return (true, false, "", "");
                }

                //    var limit = response.Headers.GetValues("Ratelimit-Limit");
                //var remaining = response.Headers.GetValues("Ratelimit-Remaining");
                //var reset = response.Headers.GetValues("Ratelimit-Reset");
            }
            catch (Exception ex)
            {
                logger?.Log(LogLevel.Error, ex, "Error getting stream status");
            }
        }

        return (false, false, "", "");
    }

    private async Task<bool> LoadStreamerID(CancellationToken stoppingToken)
    {
        int retryCount = -1;

        while (retryCount < 5)
        {
            retryCount++;

            if (retryCount > 0)
            {
                logger?.Log(LogLevel.Error, "Error getting live status");

                TimeSpan delay = new TimeSpan(0, 0, 2 * (retryCount * retryCount));

                await Task.Delay(delay, stoppingToken);
            }

            try
            {
                using HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, $"https://api.twitch.tv/helix/users?login={StreamerName}");
                //using HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, $"https://api.twitch.tv/helix/streams?first=20");
                req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);
                req.Headers.Add("Client-Id", ClientID);

                using var client = clientFactory.CreateClient();

                using var response = await client.SendAsync(req, stoppingToken);

                if (!response.IsSuccessStatusCode)
                {
                    logger?.Log(LogLevel.Error, "Error getting streamer ID");
                }

                string respText = await response.Content.ReadAsStringAsync(stoppingToken);

                var jResp = JsonDocument.Parse(respText);

                var userArray = jResp.RootElement.GetProperty("data").EnumerateArray();

                if (userArray.Count() > 0)
                {
                    StreamerID = userArray.First().GetProperty("id").GetString() ?? "";
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                logger?.Log(LogLevel.Error, ex, "Error getting streamer ID");
            }
        }

        return false;
    }

    private async Task CleanUp()
    {
        logger?.LogInformation("Cleaning up");
        await RevokeToken();
    }

    private async Task RevokeToken()
    {
        logger?.LogInformation("Revoking Twitch token");
        try
        {
            Dictionary<string, string> formVals = [];

            formVals["client_id"] = ClientID;
            formVals["token"] = AccessToken;

            using HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, "https://id.twitch.tv/oauth2/revoke");
            req.Content = new FormUrlEncodedContent(formVals);

            using var client = clientFactory.CreateClient();

            using var response = await client.SendAsync(req);

            if (!response.IsSuccessStatusCode)
            {
                logger?.Log(LogLevel.Error, "Failed to revoke token - " + await response.Content.ReadAsStringAsync());
            }
        }
        catch (Exception ex)
        {
            logger?.Log(LogLevel.Error, ex, "Failed to revoke token.");
        }
    }

    private async Task<bool> CheckAccessToken(CancellationToken stoppingToken)
    {
        if (DateTime.Now < AccessTokenExpiration) { return true; }

        return await GetAccessToken(stoppingToken);
    }

    private async Task<bool> GetAccessToken(CancellationToken stoppingToken)
    {
        int retryCount = -1;

        while (retryCount < 25)
        {
            retryCount++;

            if (retryCount > 0)
            {
                logger?.Log(LogLevel.Error, "Error getting access token");

                TimeSpan delay = new TimeSpan(0, 0, (2 * (retryCount * retryCount)));

                if (delay.TotalHours > 1)
                {
                    delay = new TimeSpan(1, 0, 0);
                }

                await Task.Delay(delay, stoppingToken);
            }

            try
            {
                Dictionary<string, string> reqVals = [];
                reqVals["client_id"] = ClientID;
                reqVals["client_secret"] = ClientSecret;
                reqVals["grant_type"] = "client_credentials";

                using HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, "https://id.twitch.tv/oauth2/token");
                req.Content = new FormUrlEncodedContent(reqVals);

                using var client = clientFactory.CreateClient();

                using var response = await client.SendAsync(req, stoppingToken);

                if (!response.IsSuccessStatusCode)
                {
                    logger?.LogError(await response.Content.ReadAsStringAsync(stoppingToken));
                    continue;
                }

                string respText = await response.Content.ReadAsStringAsync(stoppingToken);

                using var jDoc = JsonDocument.Parse(respText);

                AccessToken = jDoc.RootElement.GetProperty("access_token").GetString() ?? "";

                int tempInt = jDoc.RootElement.GetProperty("expires_in").GetInt32();

                // make sure we don't till the very last possible moment to refresh the token
                AccessTokenExpiration = DateTime.Now.AddSeconds(tempInt - 900);

                return true;
            }
            catch (Exception ex)
            {
                logger?.Log(LogLevel.Error, ex, "Error getting access token");
            }
        }

        return false;
    }
}
