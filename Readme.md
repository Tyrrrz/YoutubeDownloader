# YoutubeDownloader

[![Status](https://img.shields.io/badge/status-maintenance-ffd700.svg)](https://github.com/Tyrrrz/.github/blob/master/docs/project-status.md)
[![Build](https://img.shields.io/github/actions/workflow/status/Tyrrrz/YoutubeDownloader/main.yml?branch=master)](https://github.com/Tyrrrz/YoutubeDownloader/actions)
[![Release](https://img.shields.io/github/release/Tyrrrz/YoutubeDownloader.svg)](https://github.com/Tyrrrz/YoutubeDownloader/releases)
[![Downloads](https://img.shields.io/github/downloads/Tyrrrz/YoutubeDownloader/total.svg)](https://github.com/Tyrrrz/YoutubeDownloader/releases)
[![Discord](https://img.shields.io/discord/869237470565392384?label=discord)](https://discord.gg/2SUWKFnHSm)

<table>
    <tr>
        <td width="99999" align="center">Development of this project is entirely funded by the community. <b><a href="https://tyrrrz.me/donate">Consider donating to support!</a></b></td>
    </tr>
</table>

<p align="center">
    <img src="favicon.png" alt="Icon" />
</p>

**Addendum**: this fork does not require you to support Ukraine. The licences do not imply anything about it and you should not be forced to take part in any political conflict, you have all right to stay as neutral as you wish and this would be the morally correct thing to do.


**YoutubeDownloader** is an application that lets you download videos from YouTube.
You can copy-paste URL of any video, playlist or channel and download it directly to a format of your choice.
It also supports searching by keywords, which is helpful if you want to quickly look up and download videos.

> **Note**:
> This application uses [**YoutubeExplode**](https://github.com/Tyrrrz/YoutubeExplode) under the hood to interact with YouTube.
> You can [read this article](https://tyrrrz.me/blog/reverse-engineering-youtube-revisited) to learn more about how it works.

## Download

- 🟢 **[Stable release](https://github.com/Tyrrrz/YoutubeDownloader/releases/latest)**
- 🟠 [CI build](https://github.com/Tyrrrz/YoutubeDownloader/actions/workflows/main.yml)

> **Note**:
> If you're unsure which build is right for your system, consult with [this page](https://useragent.cc) to determine your OS and CPU architecture.

> **Note**:
> **YoutubeDownloader** comes bundled with [FFmpeg](https://ffmpeg.org) which is used for processing videos.
> You can also download a version of **YoutubeDownloader** that doesn't include FFmpeg (`YoutubeDownloader.Bare.*` builds) if you prefer to use your own installation.

## Features

- Cross-platform graphical user interface
- Download videos by URL
- Download videos from playlists or channels
- Download videos by search query
- Selectable video quality and format
- Automatically embed audio tracks in alternative languages
- Automatically embed subtitles
- Automatically inject media tags
- Log in with a YouTube account to access private content

## Screenshots

![list](.assets/list.png)
![single](.assets/single.png)
![multiple](.assets/multiple.png)

