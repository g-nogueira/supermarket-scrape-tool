### Pack a project

For all projects that have a paket.template file

```bash
dotnet paket pack --version 1.0.0 nugets
```

or
For a specific project

```bash
dotnet paket pack --template GNogueira.SupermarketScrapeTool.Models\paket.template --version 2.0.0-beta3 --symbols nugets
```

### How to add paket credentials to GitHub Nuget Feed

```bash
dotnet paket config add-credentials https://nuget.pkg.github.com/g-nogueira/index.json --verify
```

It will then be asked for your GitHub username and password. The password should be a classic GitHub Access Token.

### Publish a package to GitHub Nuget Feed

```bash
dotnet paket push nugets\GNogueira.SupermarketScrapeTool.Models.2.0.0-beta3.nupkg --url https://nuget.pkg.github.com/g-nogueira --api-key <GitHub-Access-Token>
```

### Login to Azure
We use the DefaultAzureCredential to authenticate with Azure. This will use the following methods to authenticate:
https://learn.microsoft.com/en-us/dotnet/api/azure.identity.defaultazurecredential?view=azure-dotnet

## Todo

- [ ] Add scrapper settings to appsettings.json