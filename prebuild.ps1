function DownloadWithRedirect([string]$url, [string]$outputFile) {
    $request = Invoke-WebRequest -Uri $url -MaximumRedirection 1 -SkipHttpErrorCheck -Method "OPTIONS"
    if($request.StatusCode -ge 300 -and $request.StatusCode -lt 400)
    {
        DownloadWithRedirect -url $request.Headers.Location -outputFile $outputFile
    }
    elseif($request.StatusCode -eq 200)
    {
        Invoke-WebRequest -Uri $url -OutFile $outputFile
    }
}

New-Item -ItemType Directory -Force -Path tools
cd tools
$progressPreference = 'silentlyContinue'
Write-Output "Downloading newest ffmpeg from https://www.gyan.dev/ffmpeg/builds/"
DownloadWithRedirect -url "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip" -outputFile "ffmpeg.zip"
Write-Output "Expanding ffmpeg archive"
Expand-Archive ffmpeg.zip -DestinationPath .
$location = Resolve-Path -Path "*\bin\ffmpeg.exe"
[System.Environment]::SetEnvironmentVariable('FFMPEG', $location, [System.EnvironmentVariableTarget]::Process)
