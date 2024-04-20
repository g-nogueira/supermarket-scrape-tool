param(
    [Parameter(Mandatory=$true)]
    [string]$projectName
)

# Define constants
$apiKey = [System.Environment]::GetEnvironmentVariable("GITHUB_API_KEY")
$githubUser = "g-nogueira"
$githubRepo = "supermarket-scrape-tool"
$githubPackagesUrl = "https://nuget.pkg.github.com/$githubUser"

# Build the project in Release mode
Write-Host "Building $projectName in Release mode..."
dotnet build $projectName -c Release

# Check the last release version from the GitHub Packages page
Write-Host "Checking the last release version from the GitHub Packages page..."
$lastReleaseVersion = (Invoke-RestMethod -Uri "https://api.github.com/users/$githubUser/packages/nuget/$projectName/versions" -Headers @{"Accept"="application/vnd.github+json"; "Authorization"="Bearer $apiKey"; "X-GitHub-Api-Version"="2022-11-28"}) | Select-Object -First 1 -ExpandProperty name
Write-Host "Last release version: $lastReleaseVersion"

# Determine the new version number
$versionParts = $lastReleaseVersion -split '-'
$baseVersion = $versionParts[0]
$betaVersion = $versionParts[1]

if ($betaVersion) {
    $betaNumber = [int]($betaVersion -replace 'beta', '') + 1
    $newVersion = "$baseVersion-beta$betaNumber"
} else {
    $newVersion = "$baseVersion-beta1"
}

# Package the project with the new version number
Write-Host "Packaging the project with the new version number..."
$packagePath = "nugets\$projectName.$newVersion.nupkg"
dotnet paket pack --template "$projectName\paket.template" --version $newVersion --symbols nugets

# Push the new package to GitHub Packages
Write-Host "Pushing the new package to GitHub Packages..."
dotnet paket push $packagePath --url $githubPackagesUrl --api-key $apiKey

Write-Host "Done."