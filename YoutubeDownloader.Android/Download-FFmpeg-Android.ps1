<#
.SYNOPSIS
    Downloads the FFmpeg binaries the Android app bundles.

.DESCRIPTION
    Fetches prebuilt FFmpeg executables from this repository's own release, produced by
    .github/workflows/build-ffmpeg-android.yml.

    They are built here rather than taken from a third party because no maintained
    prebuilt carries the encoders this app needs: the only current one is a pure-LGPL
    build with no external encoders at all, which cannot produce MP3, and the builds that
    do carry them were last published in 2018.

    Each file is an FFmpeg *executable* despite the .so name. Android only grants execute
    permission inside the native library directory, and only lib*.so files are placed
    there, so that is how a CLI binary has to be shipped.

.EXAMPLE
    ./Download-FFmpeg-Android.ps1 -DownloadAll -OutputPath .
#>

param (
    # Single ABI to fetch; ignored when -DownloadAll is given.
    [Parameter(Mandatory = $false)]
    [ValidateSet('arm64-v8a', 'x86_64')]
    [string]$Architecture,

    # Directory to write <abi>/libffmpeg.so into. Defaults to this script's directory.
    [Parameter(Mandatory = $false)]
    [string]$OutputPath,

    [Parameter(Mandatory = $false)]
    [switch]$DownloadAll
)

$ErrorActionPreference = 'Stop'

# Keep in sync with the release the build workflow publishes.
$Repository = 'leobischof/YoutubeDownloader'
$ReleaseTag = 'ffmpeg-8.1.2-android.2'

$Architectures = @('arm64-v8a', 'x86_64')

if (-not $OutputPath) { $OutputPath = $PSScriptRoot }

function Get-FFmpegBinary {
    param(
        [Parameter(Mandatory)][string]$Abi,
        [Parameter(Mandatory)][string]$TargetPath
    )

    $url = "https://github.com/$Repository/releases/download/$ReleaseTag/libffmpeg-$Abi.so"
    $destinationDir = Join-Path $TargetPath $Abi
    $destination = Join-Path $destinationDir 'libffmpeg.so'

    if (-not (Test-Path $destinationDir)) {
        New-Item -ItemType Directory -Path $destinationDir -Force | Out-Null
    }

    Write-Host "Downloading FFmpeg for $Abi..."

    # Download beside the target and move into place, so an interrupted download cannot
    # leave a truncated binary that later looks like a valid cached one.
    $temporary = "$destination.download"
    try {
        # Invoke-WebRequest's progress bar makes large downloads dramatically slower in
        # Windows PowerShell.
        $previousProgress = $ProgressPreference
        $ProgressPreference = 'SilentlyContinue'
        try {
            Invoke-WebRequest -Uri $url -OutFile $temporary -UseBasicParsing
        } finally {
            $ProgressPreference = $previousProgress
        }

        # A stray HTML error page would otherwise be packaged as if it were FFmpeg.
        $header = [System.IO.File]::ReadAllBytes($temporary)[0..3]
        if ($header[0] -ne 0x7F -or $header[1] -ne 0x45 -or $header[2] -ne 0x4C -or $header[3] -ne 0x46) {
            throw "Downloaded file for $Abi is not an ELF binary. Has release '$ReleaseTag' been published?"
        }

        Move-Item -Path $temporary -Destination $destination -Force
        $sizeMb = [math]::Round((Get-Item $destination).Length / 1MB, 1)
        Write-Host "FFmpeg for $Abi saved to $destination ($sizeMb MB)"
    } finally {
        if (Test-Path $temporary) { Remove-Item $temporary -Force -ErrorAction SilentlyContinue }
    }
}

if ($DownloadAll) {
    Write-Host "Downloading FFmpeg $ReleaseTag for: $($Architectures -join ', ')"
    foreach ($abi in $Architectures) {
        Get-FFmpegBinary -Abi $abi -TargetPath $OutputPath
    }
} elseif ($Architecture) {
    Get-FFmpegBinary -Abi $Architecture -TargetPath $OutputPath
} else {
    Write-Host 'Available architectures:'
    foreach ($abi in $Architectures) { Write-Host "  - $abi" }
    Write-Host ''
    Write-Host 'Pass -DownloadAll, or -Architecture <abi>.'
}
