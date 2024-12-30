# YoutubeDownloader

[![Status](https://img.shields.io/badge/status-maintenance-ffd700.svg)](https://github.com/Tyrrrz/.github/blob/master/docs/project-status.md)
[![Made in Ukraine](https://img.shields.io/badge/made_in-ukraine-ffd700.svg?labelColor=0057b7)](https://tyrrrz.me/ukraine)
[![Build](https://img.shields.io/github/actions/workflow/status/Tyrrrz/YoutubeDownloader/main.yml?branch=master)](https://github.com/Tyrrrz/YoutubeDownloader/actions)
[![Release](https://img.shields.io/github/release/Tyrrrz/YoutubeDownloader.svg)](https://github.com/Tyrrrz/YoutubeDownloader/releases)
[![Downloads](https://img.shields.io/github/downloads/Tyrrrz/YoutubeDownloader/total.svg)](https://github.com/Tyrrrz/YoutubeDownloader/releases)
[![Discord](https://img.shields.io/discord/869237470565392384?label=discord)](https://discord.gg/2SUWKFnHSm)
[![Fuck Russia](https://img.shields.io/badge/fuck-russia-e4181c.svg?labelColor=000000)](https://twitter.com/tyrrrz/status/1495972128977571848)

<table>
    <tr>
        <td width="99999" align="center">Development of this project is entirely funded by the community. <b><a href="https://tyrrrz.me/donate">Consider donating to support!</a></b></td>
    </tr>
</table>

<p align="center">
    <img src="favicon.png" alt="Icon" />
</p>

## Table of Contents

- [Getting Started](#getting-started)
- [Features](#features)
- [Terms of use](#terms-of-use)
- [Screenshots](#screenshots)


**YoutubeDownloader** is an application that lets you download videos from YouTube.
You can copy-paste URL of any video, playlist or channel and download it directly to a format of your choice.
It also supports searching by keywords, which is helpful if you want to quickly look up and download videos.

> **Note**:
> This application uses [**YoutubeExplode**](https://github.com/Tyrrrz/YoutubeExplode) under the hood to interact with YouTube.
> You can [read this article](https://tyrrrz.me/blog/reverse-engineering-youtube-revisited) to learn more about how it works.


## Getting Started

### Download release appropriate to your environment 

- 🟢 **[Stable release](https://github.com/Tyrrrz/YoutubeDownloader/releases/latest)**
- 🟠 [CI build](https://github.com/Tyrrrz/YoutubeDownloader/actions/workflows/main.yml)


### Running on Linux

### Making folder for application
```bash
mkdir ~/YoutubeDownloader
```

### Moving application file to the folder
```bash
mv YoutubeDownloader.bare.*.zip ~/YoutubeDownloader
```

### Unziping application
```bash
unzip YoutubeDownloader.Bare.linux-x64.zip
```

### Granting execution execution
```bash
### Granting execution permission
```

### Launching application
```bash
./YoutubeDownloader 
```

### Assigning symbolic link (optional) 
assigning syslink so you can launch application via command line (replace ytd with anything you like)
```bash
ln -s ~/YoutubeDownloader/YoutubeDownloader /usr/local/bin/ytd
```
now you can launch YoutubeDownloader directly via terminal 
```bash
ytd
```

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


## Terms of use<sup>[[?]](https://github.com/Tyrrrz/.github/blob/master/docs/why-so-political.md)</sup>

By using this project or its source code, for any purpose and in any shape or form, you grant your **implicit agreement** to all the following statements:

- You **condemn Russia and its military aggression against Ukraine**
- You **recognize that Russia is an occupant that unlawfully invaded a sovereign state**
- You **support Ukraine's territorial integrity, including its claims over temporarily occupied territories of Crimea and Donbas**
- You **reject false narratives perpetuated by Russian state propaganda**

To learn more about the war and how you can help, [click here](https://tyrrrz.me/ukraine). Glory to Ukraine! 🇺🇦

## Contributing

We welcome contributions to improve **YouTubeDownloader**! Whether you're fixing bugs, adding new features, enhancing documentation, or improving tests, your help is appreciated.

### How to Contribute

1. **Fork the Repository**  
   Click the "Fork" button at the top-right corner of this page to create a copy of the repository under your account.

2. **Clone the Repository**  
   Clone your fork to your local machine:
   ```bash
   git clone https://github.com/Tyrrrz/YoutubeDownloader.git
   ```
3. **Create a Branch**
   Create a new branch for you changes
    ```bash
    git checkout -b feature/your-feature-name
    ```
4. **Commit you changes**
    ```bash
    git commit -m "Add a brief description of your changes"
    ```
5. **Push your changes**
    ```bash
    git push origin feature/your-feature-name
    ```
6. **Open a pull request**

    Go to the original repository and open a pull request. Provide a clear description of your changes and the problem they solve.

## Screenshots

![list](.assets/list.png)
![single](.assets/single.png)
![multiple](.assets/multiple.png)

