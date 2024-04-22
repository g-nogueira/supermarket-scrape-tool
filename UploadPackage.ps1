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

# Check if the build was successful
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed. Stopping script execution."
    exit $LASTEXITCODE
}

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

Write-Host "Package uploaded successfully."

Write-Host "Updating the .fsproj file with the new version number..."

# Define the path to the .fsproj file
$fsprojPath = "$projectName\$projectName.fsproj"

# Load the .csproj file as an Xml object
$fsproj = New-Object Xml
$fsproj.Load($fsprojPath)

# Navigate to the Version element and update its value
$versionNode = $fsproj.SelectSingleNode('//Version')
if ($versionNode -eq $null) {
    $versionNode = $fsproj.CreateElement("Version")
    $fsproj.DocumentElement.AppendChild($versionNode)
}
$versionNode.InnerText = $newVersion

$fileVersionNode = $fsproj.SelectSingleNode('//FileVersion')
if ($fileVersionNode -eq $null) {
    $fileVersionNode = $fsproj.CreateElement("FileVersion")
    $fsproj.DocumentElement.AppendChild($fileVersionNode)
}
$fileVersionNode.InnerText = $newVersion

# Save the changes back to the .csproj file
$fsproj.Save($fsprojPath)

Write-Host "Done."