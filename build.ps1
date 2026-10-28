$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$compilerCandidates = @(
    "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe",
    "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\csc.exe"
)
$compiler = $compilerCandidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
if (-not $compiler) { throw 'The .NET Framework C# compiler (csc.exe) was not found.' }

$source = Join-Path $projectRoot 'src\GaussianInputManager.cs'
$font = Join-Path $projectRoot 'assets\fonts\GoogleSans-Regular.ttf'
$icon = Join-Path $projectRoot 'assets\GaussianInputManager.ico'
$logo = Join-Path $projectRoot 'assets\GaussianInputManager.png'
$template = Join-Path $projectRoot 'resources\linda.sh.template'
$outputDirectory = Join-Path $projectRoot 'release'
$output = Join-Path $outputDirectory 'GaussianInputManager.exe'
New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null

$compilerArgs = @(
    '/nologo',
    '/target:winexe',
    "/out:$output",
    "/win32icon:$icon",
    "/resource:$font,GoogleSansRegular",
    "/resource:$icon,AppIcon",
    "/resource:$logo,AppLogo",
    "/resource:$template,LindaTemplate",
    '/reference:System.Windows.Forms.dll',
    '/reference:System.Drawing.dll',
    $source
)
& $compiler @compilerArgs
if ($LASTEXITCODE -ne 0) { throw "Compilation failed with exit code $LASTEXITCODE." }
Write-Host "Built $output"
