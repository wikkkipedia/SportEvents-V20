# Запустіть цей скрипт у PowerShell з кореня SportEvents_V20 на ПК з .NET 8
# Показує ТІЛЬКИ реальний результат команд; не створює удаваних скріншотів.
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
Set-Location $root
$logPath = Join-Path $root 'verification'
New-Item -ItemType Directory -Force -Path $logPath | Out-Null
$stamp = Get-Date -Format 'yyyyMMdd_HHmmss'
$log = Join-Path $logPath "verify_$stamp.txt"
Start-Transcript -Path $log | Out-Null
try {
  Write-Host '[1/6] dotnet --version'
  dotnet --version
  if ($LASTEXITCODE -ne 0) { throw 'Для перевірки потрібен .NET SDK 8 або новіший.' }

  Write-Host '[2/6] dotnet restore'
  dotnet restore SportEvents.sln
  if ($LASTEXITCODE -ne 0) { throw 'Restore failed' }

  Write-Host '[3/6] dotnet format --verify-no-changes'
  dotnet format SportEvents.sln --verify-no-changes --no-restore
  if ($LASTEXITCODE -ne 0) { throw 'Formatting failed' }

  Write-Host '[4/6] dotnet build Release -warnaserror'
  dotnet build SportEvents.sln -c Release --no-restore -warnaserror
  if ($LASTEXITCODE -ne 0) { throw 'Build failed' }

  Write-Host '[5/6] dotnet test with line coverage'
  dotnet test SportEvents.Tests/SportEvents.Tests.csproj -c Release --no-restore -p:CollectCoverage=true -p:CoverletOutputFormat=cobertura -p:Threshold=70 -p:ThresholdType=line -p:ThresholdStat=total
  if ($LASTEXITCODE -ne 0) { throw 'Tests or coverage threshold failed' }

  Write-Host '[6/6] git log'
  git log --oneline -n 25
  Write-Host 'Перевірки успішні. Лог:' $log
}
finally {
  Stop-Transcript | Out-Null
}
