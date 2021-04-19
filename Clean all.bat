rem @echo off
attrib -a -h ".vs\YoutubeDownloader\DesignTimeBuild\.dtbcache.v2"
attrib -a -h ".vs\YoutubeDownloader\v16\.suo"

del /s/q .vs\*
del /s/q YoutubeDownloader\bin\*
del /s/q YoutubeDownloader\obj\*

call "Clean empty directories.bat"
