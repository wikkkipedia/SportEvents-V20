# Виконуйте з кореня SportEvents (Windows PowerShell 5.1+).
$ErrorActionPreference = 'Stop'
$commands = @(
    @('restore', 'SportEvents.sln'),
    @('format', 'SportEvents.sln', '--verify-no-changes', '--no-restore'),
    @('build', 'SportEvents.sln', '-c', 'Release', '--no-restore', '-warnaserror'),
    @('test', 'SportEvents.Tests/SportEvents.Tests.csproj', '-c', 'Release', '--no-restore', '-p:CollectCoverage=true', '-p:CoverletOutputFormat=cobertura', '-p:Threshold=70', '-p:ThresholdType=line', '-p:ThresholdStat=total')
)
foreach ($commandArgs in $commands) {
    & dotnet @commandArgs
    if ($LASTEXITCODE -ne 0) {
        Write-Error ('Невдала команда: dotnet ' + ($commandArgs -join ' '))
        exit $LASTEXITCODE
    }
}
Write-Host 'Усі локальні перевірки пройшли.'
