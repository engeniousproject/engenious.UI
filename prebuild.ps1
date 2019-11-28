New-Item -ItemType Directory -Force -Path tools
cd tools
$progressPreference = 'silentlyContinue'
Invoke-WebRequest https://ffmpeg.zeranoe.com/builds/win64/static/ffmpeg-latest-win64-static.zip -OutFile ffmpeg.zip
Expand-Archive ffmpeg.zip -DestinationPath .
$location = Resolve-Path -Path "ffmpeg-latest-win64-static\bin\ffmpeg.exe"
[System.Environment]::SetEnvironmentVariable('FFMPEG', $location, [System.EnvironmentVariableTarget]::User)