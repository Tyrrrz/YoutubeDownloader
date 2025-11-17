param (
    [Parameter(Mandatory=$false)]
    [string]$Platform,

    [Parameter(Mandatory=$false)]
    [string]$OutputPath = $PSScriptRoot
)

$ErrorActionPreference = "Stop"

# If the platform is not specified, use the current OS/arch
if (-not $Platform) {
    $arch = [Runtime.InteropServices.RuntimeInformation]::OSArchitecture

    if ($isWindows) {
        $Platform = "windows-$arch"
    } elseif ($isLinux) {
        $Platform = "linux-$arch"
    } elseif ($isMacOS) {
        $Platform = "osx-$arch"
    } else {
        throw "Unsupported platform"
    }
}

# Normalize platform identifier
$Platform = $Platform.ToLower().Replace("win-", "windows-")

# Identify the FFmpeg filename based on the platform
$fileName = if ($Platform.Contains("windows-")) { "ffmpeg.exe" } else { "ffmpeg" }

# If the output path is an existing directory, append the default file name for the platform
if (Test-Path $OutputPath -PathType Container) {
    $OutputPath = Join-Path $OutputPath $fileName
}

# Delete the existing file if it exists
if (Test-Path $OutputPath) {
    Remove-Item $OutputPath
}

# Download the archive
Write-Host "Downloading FFmpeg for $Platform..."
$http = New-Object System.Net.WebClient
try {
    $http.DownloadFile("https://github.com/Tyrrrz/FFmpegBin/releases/download/7.1.2/ffmpeg-$Platform.zip", "$OutputPath.zip")
} finally {
    $http.Dispose()
}

try {
    # Extract FFmpeg
    Add-Type -Assembly System.IO.Compression.FileSystem
    $zip = [IO.Compression.ZipFile]::OpenRead("$OutputPath.zip")
    try {
        [IO.Compression.ZipFileExtensions]::ExtractToFile($zip.GetEntry($fileName), $OutputPath)
    } finally {
        $zip.Dispose()
    }

    Write-Host "Done downloading FFmpeg."
} finally {
    # Clean up
    Remove-Item "$OutputPath.zip" -Force
}