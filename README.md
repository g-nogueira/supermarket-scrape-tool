Pack a project
```bash
dotnet paket pack nugets --version 1.0.0
```

How to add paket credentials to GitHub Nuget Feed
```bash
dotnet paket config add-credentials https://nuget.pkg.github.com/g-nogueira/index.json --verify
```
Publish a package to GitHub Nuget Feed
```bash
dotnet paket push <Package-Path-Name-Extension> --url https://nuget.pkg.github.com/g-nogueira --api-key <GitHub-Access-Token>
```