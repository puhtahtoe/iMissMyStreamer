# iMissMyStreamer

**iMissMyStreamer** is a simple self-hostable site to celebrate your favorite streamer.

Heavily inspired by [imissfauna.com](https://github.com/saplinganon/imissfauna.com) but developed with Twitch in mind.

This project was largely an excuse to dip my toes into Blazor and containerized development a little bit, two things in which I am lacking experience.

## Running your own

You will need to register an application with Twitch through their [developer portal](https://dev.twitch.tv/docs/authentication/register-app/).

The easiest way to run your own instance is with [Docker](https://docker.com) using Docker-Compose.

```
services:
  iMissMyStreamer:
    image: ghcr.io/puhtahtoe/imissmystreamer:latest
    container_name: iMissMyStreamer
    restart: unless-stopped
    volumes:
      - /path/to/data:/data
    ports:
      - 8080:8080
    Environment:
      - TwitchClientID=xxxxxxxxxx
      - TwitchClientSecret=xxxxxxxxxx
      - TwitchStreamerName=xxxxxxxxxx
```

### Configuration - Data and images

To facilitate persistence and enable you to add images, you will need to mount storage to the /data directory.

The application will store a small json file at /data/persist.json to keep track of stream status and last stream times in between restarts.

You may place an icon at /data/favicon.ico for the site to use.

Images for online and offline status are stored in /data/images/live/ and /data/images/offline/. 

Each folder must also have a manifest.json file in the following format:

```
[
    {
        "FileName": "file1.png",
        "ArtistName": "Artist1",
        "ArtistLink": "Cool.Art",
        "AltText": "The most beautiful image you have never seen."
    },
    {
        "FileName": "anotherImageButThisOneMoves.gif",
        "ArtistName": "Second Artist",
        "ArtistLink": "www.Cooler.Art",
        "AltText": "an ok picture."
    }
]
```

### Configuration - Customization

There are several environment variables you can take advantage of to customize the site

| Variable | Description | Required | Default |
| -------- | ----------- | -------- | ------- |
| TwitchClientID | The client ID for an application registered with Twitch | yes | n/a | 
| TwitchClientSecret | The client secret for an application registered with Twitch | yes | n/a |
| TwitchStreamerName | The streamer's name on Twitch to use for status lookups | yes | n/a |
| StreamerDisplayName | The streamer's name to use for display purposes within the application | no | value of TwitchStreamerName |
| StreamerShortName | A nickname / short name / informal name for the streamer. Used for display in some places within the application | no | value of StreamerDisplayName or TwitchStreamerName |
| StreamerStreamURL | The URL to use for the link to the streamer's stream when live | no | the stream title will be displayed without a link |
| StreamerVODsURL | A link to a location where viewers can watch VODs of the streamer's past streams | no | If no URL is given, no link will be displayed |
| StreamerHighlightsURL | A link to a location where the streamer uploads edited or clipped highlights | no | If no URL is given, no link will be displayed |
| BackgroundColor | The color to use for the CSS background-color of the site. Can be any valid CSS value. | no | #62ba62 |
| FontColor | The color to use for the text. Can be any valid CSS value. | no | #293F14 |
| FontFamily | The font family to use for the text. Can be any valid CSS value. | no | Arial, sans-serif |
| LinkColor | The color to use for links. Can be any valid CSS value. | no | #293F14|
| LinkHoverColor | The color to use for hovering over links. Can be any valid CSS value. | no | #293F14 |
| LinkVistedColor | The color to use for visited links. Can be any valid CSS value. | no | #293F14 |
| LinkActiveColor | The color to use for activated links. Can be any valid CSS value. | no | #293F14 |
| debug | Enables the /debug page for toggling live status and setting game/stream title for testing purposes. Set to true to enable. Creates a debug persist.json file at /data/debug_persist.json so as to not interfere with real persisted data. | no | false |
