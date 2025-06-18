# TakiWaki
Wireless chat without Internet

## Platform Support Summary

| Platform      | Server (Record) | Client (Playback) |
|--------------|:---------------:|:-----------------:|
| Windows 11   | ✅              | ✅                |
| Android      | ✅              | ✅                |
| iOS          | ❌              | ✅                |

- **Server (Record):** Device can act as the server, capturing audio from the microphone and broadcasting it over WiFi.
- **Client (Playback):** Device can act as the client, receiving and playing audio from the server over WiFi.

This app uses [Plugin.Maui.Audio](https://github.com/jfversluis/Plugin.Maui.Audio) for cross-platform audio features. Microphone recording is currently only supported on Windows and Android. Audio playback is supported on all platforms.
