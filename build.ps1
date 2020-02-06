<#
.SYNOPSIS
Compiles, tests, packs, and publishes the Blueshift.Jobs solution.

.DESCRIPTION
Executes the steps to compile, test, and pack the solution. With the "Publish" stage, this
script can also be used to publish build artifacts to the configured package manager.
 
.PARAMETER Stages
A list of build stages to execute. Valid stages are:

- Restore
    Restores the dependency tree for the solution.
- Compile
    Compiles the solution.
- Test
    Runs tests found within the solution. Test results are placed in the .\artifacts\testResults directory.
- Pack
    Ensures that all packaged build artifacts are created and placed in the .\artifacts\packages directory.
- Publish
    Pushes package artifacts to their respective package managers.

The default stage list is "Restore, Compile, Test, Pack". By default, the "Publish" stage is not included in order
to reduce load on the package management system.

.PARAMETER NugetPublishSource
The NuGet package source URI to use when publishing nupkg artifacts.

.PARAMETER NugetApiKey
The API key to use when publishing nupkg files.
#>
[CmdletBinding()]
Param (
    [Parameter(Position = 0)]
    [ValidateSet("Restore", "Compile", "Test", "Pack", "Publish")]
    [string[]] $Stages = @("Restore", "Compile", "Test", "Pack"),

    [string] $NugetPublishSource = $env:NUGET_PUBLISH_SOURCE,

    [string] $NugetApiKey = $env:NUGET_API_KEY
)

Push-Location $PSScriptRoot

$separator = $([System.IO.Path]::DirectorySeparatorChar)

try {
    $artifactsDir = (Join-Path (PWD) "artifacts")
    $srcDir = (Join-Path (PWD) "src${separator}")
    $testDir = (Join-Path (PWD) "test${separator}")
    $packagesDir = (Join-Path $artifactsDir "packages${separator}")
    $testResultsDir = (Join-Path $artifactsDir "testResults${separator}")

    if ($Stages -contains "Restore") {
        dotnet restore --force
    }

    if ($Stages -contains "Compile") {
        dotnet build --no-restore --force
    }

    if ($Stages -contains "Test") {
        del $testResultsDir -ErrorAction SilentlyContinue -Recurse -Force

        $coverageFile = (Join-Path $testResultsDir "result.json")

        $excludeFromCoverageByAttribute = @(
            "Obsolete"
            "GeneratedCodeAttribute"
            "CompilerGeneratedAttribute"
        )

        $coverageThresholdTypes = @(
            "line",
            "branch",
            "method"
        )

        $testParams = @(
            "/p:CollectCoverage=true"
            "/p:MergeWith=$coverageFile"
            "/p:ExcludeByAttribute=`"$($excludeFromCoverageByAttribute -join '%2c')`""
        )

        if ([System.Convert]::ToBoolean($env:ENABLE_CODE_COVERAGE) -eq $true) {
            $testParams += "/p:Threshold=80"
            $testParams += "/p:ThresholdType=`"$($coverageThresholdTypes -join '%2c')`""
            $testParams += "/p:ThresholdStat=minimum"
        }

        dotnet test `
            $testParams `
            --results-directory $testResultsDir `
            --logger "trx" `
            --no-build `
            --no-restore
    }

    if ($Stages -contains "Pack") {
        del $packagesDir -ErrorAction SilentlyContinue -Recurse -Force

        dotnet pack -o $packagesDir --no-build --no-restore
    }

    if ($Stages -contains "Publish") {
        dotnet nuget push $packagesDir -s $NugetPublishSource -k $NugetApiKey
    }
} finally {
    Pop-Location
}