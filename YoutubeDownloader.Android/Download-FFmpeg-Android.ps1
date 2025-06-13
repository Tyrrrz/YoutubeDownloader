param (
    [Parameter(Mandatory=$false)]
    [string]$Architecture,

    [Parameter(Mandatory=$false)]
    [string]$OutputPath,

    [Parameter(Mandatory=$false)]
    [switch]$DownloadAll
)

$ErrorActionPreference = "Stop"

# Android architecture mappings
$AndroidArchitectures = @{
    "arm" = "arm-full.tar.bz2"
    "arm-v7n" = "arm-v7n-full.tar.bz2"
    "arm64-v8a" = "arm64-v8a-full.tar.bz2"
    "armv7-a" = "armv7-a-full.tar.bz2"
    "i686" = "i686-full.tar.bz2"
    "native" = "native-full.tar.bz2"
    "x86_64" = "x86_64-full.tar.bz2"
}

# GitHub repository information
$GitHubRepo = "Khang-NT/ffmpeg-binary-android"
$ReleaseTag = "2018-07-31"  # You can modify this to use "latest" if needed
$BaseUrl = "https://github.com/$GitHubRepo/releases/download/$ReleaseTag"

# If output path is not specified, use the current directory
if (-not $OutputPath) {
    $OutputPath = $PSScriptRoot
}

# Ensure output directory exists
if (-not (Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
}

function Download-And-Extract-FFmpeg {
    param(
        [string]$ArchName,
        [string]$FileName,
        [string]$TargetPath
    )
    
    $downloadUrl = "$BaseUrl/$FileName"
    $tempArchive = Join-Path $env:TEMP $FileName
    $tempExtractDir = Join-Path $env:TEMP "ffmpeg_extract_$ArchName"
    $finalOutputDir = Join-Path $TargetPath $ArchName
    
    try {
        Write-Host "Downloading FFmpeg for Android ($ArchName)..."
        
        # Download the archive
        $webClient = New-Object System.Net.WebClient
        try {
            $webClient.DownloadFile($downloadUrl, $tempArchive)
        } finally {
            $webClient.Dispose()
        }
        
        # Create extraction directory
        if (Test-Path $tempExtractDir) {
            Remove-Item $tempExtractDir -Recurse -Force
        }
        New-Item -ItemType Directory -Path $tempExtractDir -Force | Out-Null
        
        # Extract the tar.bz2 file
        Write-Host "Extracting $FileName..."
        
        # Use 7-Zip if available, otherwise use PowerShell with tar command
        $sevenZipPath = Get-Command "7z.exe" -ErrorAction SilentlyContinue
        if ($sevenZipPath) {
            # Extract using 7-Zip (handles tar.bz2 directly)
            & $sevenZipPath.Source x $tempArchive "-o$tempExtractDir" -y | Out-Null
            
            # If it's a tar file inside, extract that too
            $tarFile = Get-ChildItem $tempExtractDir -Filter "*.tar" | Select-Object -First 1
            if ($tarFile) {
                & $sevenZipPath.Source x $tarFile.FullName "-o$tempExtractDir" -y | Out-Null
                Remove-Item $tarFile.FullName -Force
            }
        } else {
            # Try using tar command (available in Windows 10+)
            try {
                & tar -xjf $tempArchive -C $tempExtractDir
            } catch {
                Write-Warning "Could not extract $FileName. Please install 7-Zip or use Windows 10+ with built-in tar support."
                return
            }
        }
        
        # Find the ffmpeg binary in the extracted files
        $ffmpegBinary = Get-ChildItem $tempExtractDir -Name "ffmpeg" -Recurse | Select-Object -First 1
        if ($ffmpegBinary) {
            $ffmpegPath = Join-Path $tempExtractDir $ffmpegBinary
            
            # Create architecture-specific output directory
            if (-not (Test-Path $finalOutputDir)) {
                New-Item -ItemType Directory -Path $finalOutputDir -Force | Out-Null
            }
            
            # Copy ffmpeg binary to final location
            $finalFFmpegPath = Join-Path $finalOutputDir "ffmpeg"
            Copy-Item $ffmpegPath $finalFFmpegPath -Force
            
            Write-Host "FFmpeg for $ArchName extracted to: $finalFFmpegPath"
        } else {
            Write-Warning "Could not find ffmpeg binary in extracted archive for $ArchName"
        }
        
    } catch {
        Write-Error "Failed to download or extract FFmpeg for $ArchName`: $($_.Exception.Message)"
    } finally {
        # Clean up temporary files
        if (Test-Path $tempArchive) {
            Remove-Item $tempArchive -Force -ErrorAction SilentlyContinue
        }
        if (Test-Path $tempExtractDir) {
            Remove-Item $tempExtractDir -Recurse -Force -ErrorAction SilentlyContinue
        }
    }
}

if ($DownloadAll) {
    Write-Host "Downloading all Android FFmpeg architectures..."
    foreach ($arch in $AndroidArchitectures.Keys) {
        Download-And-Extract-FFmpeg -ArchName $arch -FileName $AndroidArchitectures[$arch] -TargetPath $OutputPath
    }
} elseif ($Architecture) {
    if ($AndroidArchitectures.ContainsKey($Architecture)) {
        Download-And-Extract-FFmpeg -ArchName $Architecture -FileName $AndroidArchitectures[$Architecture] -TargetPath $OutputPath
    } else {
        Write-Error "Unsupported architecture: $Architecture. Available architectures: $($AndroidArchitectures.Keys -join ', ')"
        exit 1
    }
} else {
    Write-Host "Available Android architectures:"
    foreach ($arch in $AndroidArchitectures.Keys) {
        Write-Host "  - $arch"
    }
    Write-Host ""
    Write-Host "Usage examples:"
    Write-Host "  .\Download-FFmpeg-Android.ps1 -Architecture arm64-v8a"
    Write-Host "  .\Download-FFmpeg-Android.ps1 -DownloadAll"
    Write-Host "  .\Download-FFmpeg-Android.ps1 -Architecture arm64-v8a -OutputPath ./android-assets"
}