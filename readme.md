# Mtd.Extensions.AspNetCore

[![.NET Build and Test](https://github.com/CUMTD/Mtd.Extensions.AspNetCore/actions/workflows/build-test.yml/badge.svg)](https://github.com/CUMTD/Mtd.Extensions.AspNetCore/actions/workflows/build-test.yml)
![GitHub Release](https://img.shields.io/github/v/release/cumtd/Mtd.Extensions.AspNetCore?sort=semver&style=flat&logo=nuget&color=34D058&cacheSeconds=300)

## GitHub NuGet Feed

See instructions in [Mtd.Core][core] for information about using the GitHub NuGet feed.

## Creating Releases

To update the NugetPackage for this repository, follow these steps:

1. Create a PR and update the `main` branch with your changes.
2. When the branch is updated and all checks have passed,
   go to the [Releases][releases] and click "[Create a new release][new-release]".
3. Under "Choose a tag", click "Create new tag".
   Name your tag in the format `vX.Y.Z` following semantic versioning.
   Ideally, the major version should match the .NET version that the package is targeting.
4. Click "Generate releasee notes" to auto-populate the release notes. Add any additional notes if needed.
5. Click "Publish release".
6. This should trigger the "Publish Release" action which will build and publish the package to the GitHub NuGet feed.

## Using the Package

This package is designed to be used by MTD for ASP.NET Core projects.
It provides a set of extensions and utilities to simplify common configurations and standardize project structure.

### Sample Usage

Below is a sample application that uses the features of this package.
Each will be explained in more detail below.
In the example everything is in the `Program.cs` file,
but in a real project you would likely split out at least the service registration and database setup
into their own files or extension methods.

```csharp
// Program.cs
using Mtd.Extensions.AspNetCore.Config;
using Mtd.Extensions.AspNetCore.Extensions;
using Mtd.Extensions.AspNetCore.Filter;

using Serilog;

using Test1.Config;

var builder = WebApplication
	.CreateBuilder(args);

var assembly = "Mtd.SampleApi";

var swaggerConfig = new SwaggerConfig
{
	Title = "Mtd Sample Api",
	Description = "Sample API for a super cool MTD project.",
	MajorVersion = 1,
	MinorVersion = 0,
	IncludeApiKeySecurity = true,
	IncludeXmlComments = true,
	RunSwaggerAtRoot = true,
	CustomCssPath = "/css/swagger-ui.css"
};

try
{
	// set assembly name
	assembly = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

	// get key vault url from configuration
	var keyVaultUrl = builder.Configuration["keyVaultUrl"];
	if (string.IsNullOrEmpty(keyVaultUrl))
	{
		throw new Exception("keyVaultUrl is not set in the configuration.");
	}

	// configure the app with common MTD defaults.
	builder = builder
		.Mtd_ConfigureForKeyVault<Program>(new KeyVaultConfig { KeyVaultUrl = keyVaultUrl, EnvironmentVariablePrefix ="MyApp_" })
		.Mtd_AddSerilogLogging()
		.Mtd_BindOptionToConfigSection<SampleConfig>("sampleConfig")
		.Mtd_BindOptionToConfigSection<ApiKeyConfig>("apiKeyConfig")
		.Mtd_BindOptionToConfigSection<ConnectionStrings>("connectionStrings")
		.Mtd_ConfigureSwagger(swaggerConfig);

	// Add Simple API Key Filter
	// Should be used with .Mtd_BindOptionToConfigSection<ApiKeyConfig>("apiKeyConfig")
	builder.Services.AddControllers(options => options.Filters.Add<SimpleApiKeyFilter>());

	// Register EF Here
	builder.Services.AddDbContextPool<MyContext>((sp, options) =>
	{
		var connectionString = sp.GetRequiredService<IOptions<ConnectionStrings>>().Value.MyConnectionString;
		options.UseSqlServer(connectionString, o => o.EnableRetryOnFailure(2));
	});

	// Register Services Here
	builder.Services.AddScoped<IMyService, MyService>();

	// build the app
	var app = builder
		.Build();

	// always use HSTS and HTTPS in production
	if (app.Environment.IsProduction())
	{
		app.UseHsts();
		app.UseHttpsRedirection();
	}

	// Useful if you want to use custom CSS for swagger.
	app.MapStaticAssets();

	// Use the standard routing middleware
	app.UseRouting();

	// Standard Swagger setup
	app.Mtd_UseSwagger(swaggerConfig);

	// Use the Simple API Key Filter
	app.MapControllers();

	// Run the app
	Log.Information("The {assembly} is starting.", assembly);
	await app.RunAsync();
}
catch (Exception ex)
{
	Log.Fatal(ex, "The {assembly} failed to start correctly.", assembly);
}
finally
{
	Log.Information("The {assembly} has stopped.", assembly);
	Log.CloseAndFlush();
}
```

Note that the above configuration would require the following NuGet packages:

* `Azure.Extensions.AspNetCore.Configuration.Secrets`
* `Azure.Identity`
* `Microsoft.Extensions.DependencyInjection`
* `Microsoft.EntityFrameworkCore.Relational`
* `Microsoft.EntityFrameworkCore.SqlServer`
* `Serilog`
* `Serilog.AspNetCore`
* `Swashbuckle.AspNetCore`
* `Swashbuckle.AspNetCore.Annotations`
* `Swashbuckle.AspNetCore.Filters`

## Extension Methods

### `Mtd_ConfigureForKeyVault`

This extension method configures the application to use Azure Key Vault for configuration.
It requires the following NuGet packages:

* `Azure.Extensions.AspNetCore.Configuration.Secrets`
* `Azure.Identity`

To use it, you must provide a `KeyVaultConfig` object with the URL of the key vault.

It registres the following configuration sources in this order:

1. Azure Key Vault
2. Environmental Variables (Only if , `EnvironmentVariablePrefix` is provided)
3. User Secrets (Only in Development Mode)

This means that if the same value is defined in both Azure Key Vault and an environmental variable,
the value from Environmental Variable will be used. Likewise, if the same value is also defined in user secrets,
the value from user secrets will be used.

[core]: https://github.com/CUMTD/Mtd.Core
[releases]: https://github.com/CUMTD/Mtd.Extensions.AspNetCore/releases
[new-release]: https://github.com/CUMTD/Mtd.Extensions.AspNetCore/releases/new

### `Mtd_AddSerilogLogging`

This extension method configures the application to use Serilog for logging.

It requires the following NuGet packages:

* `Serilog`
* `Serilog.AspNetCore`

Additonally, the following NuGet packages are recommended:

* `Serilog.Enrichers.AssemblyName`
* `Serilog.Enrichers.Environment`
* `Serilog.Enrichers.Process`
* `Serilog.Enrichers.Thread`
* `Serilog.Sinks.Console`
* `Serilog.Sinks.Seq`

Using these packages, your `appSettings.json` configuraiton might look like this:

```json
{
    // Other Settings
    "Serilog": {
        "Using": [
            "Serilog.Sinks.Seq",
            "Serilog.Enrichers.Environment",
            "Serilog.Enrichers.Process",
            "Serilog.Enrichers.Thread"
        ],
        "MinimumLevel": {
            "Default": "Information",
            "Override": {
                "Microsoft": "Warning",
                "System": "Warning",
                "Microsoft.Hosting.Lifetime": "Information"
            }
        },
        "Enrich":[
            "FromLogContext",
            "WithMachineName",
            "WithEnvironmentUserName",
            "WithProcessId",
            "WithThreadId",
            "WithAssemblyName",
            "WithAssemblyVersion"
        ],
        "WriteTo":[
            {
                "Name": "Seq",
                "Args": {
                    "serverUrl": "<SEQ Server Url>",
                    "apiKey": "<Uses Azure key Vault>"
                }
            },
            {
                "Name": "Console",
                "Args": {
                    "theme": "Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme::Code, Serilog.Sinks.Console",
                    "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} &lt;s:{SourceContext}&gt;{NewLine}{Exception}",
                    "restrictedToMinimumLevel": "Information"
                }
            }
        ]
    }
}
```

### `Mtd_BindOptionToConfigSection`

This extension method binds a configuration section to an object and allow the object to be injected into services using `IOptions`.
It will run validation on any `System.ComponentModel.DataAnnotations` attributes on the option object.

Here is example usage for binding the ApiKeyConfig class to the `apiKeyConfig` section in `appSettings.json`:

```json
// appSettings.json
{
	"apiKeyConfig": {
		"keys": [
			"key1",
			"key2"
		]
	}
}
```

```csharp
// Program.cs

using Mtd.Extensions.AspNetCore.Config;
...
builder.Mtd_BindOptionToConfigSection<ApiKeyConfig>("apiKeyConfig");
```

### `Mtd_ConfigureSwagger` and `Mtd_UseSwagger`

These extension methods configure the application to use Swagger for API documentation.
Both should be used in conjunction.

The `SwaggerConfig` object has several properties for configuring the Swagger UI:

| Property                | Required | Description                                                                                                                                                         | Default           |
|-------------------------|----------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------|-------------------|
| `Title`                 | Yes      | The title of the API.                                                                                                                                               |                   |
| `Description`           | Yes      | A description of the API.                                                                                                                                           |                   |
| `ContactName`           | No       | The name of the contact for the API.                                                                                                                                | MTD               |
| `ContactEmail`          | No       | The email of the contact for the API.                                                                                                                               | developer@mtd.org |
| `MajorVersion`          | No       | The major version of the API.                                                                                                                                       | 1                 |
| `MinorVersion`          | No       | The minor version of the API.                                                                                                                                       | 0                 |
| `IncludeApiKeySecurity` | No       | Whether to include an API key security scheme. If true, will configure to use `SimpleApiKeyFilter`                                                                  | true              |
| `IncludeXmlComments`    | No       | Whether to include XML comments in the Swagger documentation. If true, project config should include `<GenerateDocumentationFile>true</GenerateDocumentationFile>` | true              |
| `RunSwaggerAtRoot`      | No       | Whether to run the Swagger UI at the root of the application instead of `/swagger`.                                                                                 | true              |
| `CustomCssPath`         | No       | Path to a custom CSS file to use for the Swagger UI.                                                                                                                | null              |

## Simple API Key Filter

The `SimpleApiKeyFilter` is a simple filter that can be used to require an API key for all requests.

It takes keys in an `X-ApiKey` header. It compares them to a list of keys in the `ApiKeyConfig` object.

To use the filter, you must first bind the `ApiKeyConfig` object to a configuration section as [described above](#mtd_bindoptiontoconfigsection).
Then, you can add the filter to the MVC options in the `ConfigureServices` method of your `Startup` class:

```json
// appSettings.json
{
	...
	"apiKeyConfig": {
		"keys": [
			"key1",
			"key2"
		]
	}
}
```

```csharp
// Program.cs
using Mtd.Extensions.AspNetCore.Config;
using Mtd.Extensions.AspNetCore.Extensions;
using Mtd.Extensions.AspNetCore.Filter;

...

// Bind the ApiKeyConfig to the apiKeyConfig section in appSettings.json
builder.Mtd_BindOptionToConfigSection<ApiKeyConfig>("apiKeyConfig")

...

// Add Simple API Key Filter to the MVC options.
builder.Services.AddControllers(options => options.Filters.Add<SimpleApiKeyFilter>());
	
...

// Add controllers configured with the SimpleApiKeyFilter to the App.
app.MapControllers();


```
