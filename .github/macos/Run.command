#!/bin/bash
# Removes the app from quarantine and launches it
cd "$(dirname "$0")" || exit 1
xattr -rd com.apple.quarantine YoutubeDownloader.app
open YoutubeDownloader.app
