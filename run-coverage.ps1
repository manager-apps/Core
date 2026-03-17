param([switch]$Open)

$root        = $PSScriptRoot
$coverageDir = Join-Path $root 'coverage-results'
$reportDir   = Join-Path $root 'coverage-report'

if (Test-Path $coverageDir) { Remove-Item -Recurse -Force $coverageDir }
if (Test-Path $reportDir)   { Remove-Item -Recurse -Force $reportDir }

Write-Host 'Building solution...' -ForegroundColor Cyan
dotnet build (Join-Path $root 'Manager.sln') --configuration Debug --nologo
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$testProjects = @(
    'tests\Server.Api.Tests\Server.Api.Tests.csproj',
    'tests\Server.Ingest.Tests\Server.Ingest.Tests.csproj',
    'tests\Server.InstructionWorker.Tests\Server.InstructionWorker.Tests.csproj',
    'tests\Server.MetricWorker.Tests\Server.MetricWorker.Tests.csproj'
)

Write-Host 'Running tests with coverage...' -ForegroundColor Cyan
foreach ($project in $testProjects) {
    $projectPath = Join-Path $root $project
    Write-Host "  >> $project" -ForegroundColor Gray
    dotnet test $projectPath --collect:'XPlat Code Coverage' --results-directory $coverageDir --no-build --nologo
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

$coverageFiles = Get-ChildItem -Path $coverageDir -Recurse -Filter 'coverage.cobertura.xml' |
    Select-Object -ExpandProperty FullName

if ($coverageFiles.Count -eq 0) {
    Write-Error 'No coverage files found.'
    exit 1
}

Write-Host "Found $($coverageFiles.Count) coverage file(s)" -ForegroundColor Gray

$reports = $coverageFiles -join ';'
reportgenerator "-reports:$reports" "-targetdir:$reportDir" '-reporttypes:Html;Badges;TextSummary' '-assemblyfilters:+Server.*;+Common;-*.Tests' '-title:Manager – Test Coverage'
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$summary = Join-Path $reportDir 'Summary.txt'
if (Test-Path $summary) {
    Write-Host ''
    Get-Content $summary | Write-Host
}

Write-Host "Report: $reportDir\index.html" -ForegroundColor Green

if ($Open) {
    Invoke-Item (Join-Path $reportDir 'index.html')
}
