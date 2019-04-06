param([string] $newVersion)

function Replace-TextInFile {
    param([string] $filePath, [string] $pattern, [string] $replacement)

    $content = [IO.File]::ReadAllText($filePath)
    $content = [Text.RegularExpressions.Regex]::Replace($content, $pattern, $replacement)
    [IO.File]::WriteAllText($filePath, $content, [Text.Encoding]::UTF8)
}

Replace-TextInFile "$PSScriptRoot\YoutubeDownloader\Properties\AssemblyInfo.cs" '(?<=Assembly.*?Version\(")(.*?)(?="\)\])' $newVersion
Replace-TextInFile "$PSScriptRoot\Deploy\Choco\youtube-downloader.nuspec" '(?<=<version>)(.*?)(?=</version>)' $newVersion
Replace-TextInFile "$PSScriptRoot\Deploy\Choco\tools\chocolateyinstall.ps1" '(?<=download/)(.*?)(?=/)' $newVersion