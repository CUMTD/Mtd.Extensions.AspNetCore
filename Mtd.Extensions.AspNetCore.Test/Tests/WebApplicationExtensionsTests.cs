using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Builder;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mtd.Extensions.AspNetCore.Config;
using Mtd.Extensions.AspNetCore.Extensions;

namespace Mtd.Extensions.AspNetCore.Test.Tests;
[TestClass]
internal class WebApplicationExtensionsTests
{
	[TestMethod]
	[ExpectedException(typeof(ArgumentNullException))]
	public void Mtd_UseSwagger_NullApp_Throws()
	{
		WebApplication? app = null;
		var swaggerConfig = new SwaggerConfig
		{
			Title = "Test",
			Description = "Test"
		};
		app!.Mtd_UseSwagger(swaggerConfig);
	}

	[TestMethod]
	[ExpectedException(typeof(ArgumentNullException))]
	public void Mtd_UseSwagger_NullConfig_Throws()
	{
		var builder = WebApplication.CreateBuilder();
		using var app = builder.Build();
		app.Mtd_UseSwagger(null!);
	}

	[TestMethod]
	[ExpectedException(typeof(ValidationException))]
	public void Mtd_UseSwagger_InvalidConfig_Throws()
	{
		var builder = WebApplication.CreateBuilder();
		using var app = builder.Build();
		var config = new SwaggerConfig
		{
			Title = "", // invalid
			Description = "Test"
		};
		app.Mtd_UseSwagger(config);
	}

	[TestMethod]
	public void Mtd_UseSwagger_ValidConfig_EnablesSwaggerUI()
	{
		var builder = WebApplication.CreateBuilder();
		using var app = builder.Build();
		var config = new SwaggerConfig
		{
			Title = "My API",
			Description = "My description"
		};
		app.Mtd_UseSwagger(config);

		// No exception should be thrown.
		Assert.IsNotNull(app);
	}
}
