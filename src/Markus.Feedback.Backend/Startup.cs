using Markus.Feedback.Backend.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;

namespace Markus.Feedback.Backend
{
	public class Startup
	{
		public Startup(IHostingEnvironment env)
		{
			Configuration = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables()
				.Build();
		}

		public IConfigurationRoot Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc();

			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new Info { Title = "Markus.Feedback.Backend", Version = "v1" });
			});

			services.AddOptions();
			services.Configure<MailConfiguration>(Configuration.GetSection("Mail"));
			services.Configure<RegistrationConfiguration>(Configuration.GetSection("Registration"));
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			loggerFactory.AddConsole(Configuration.GetSection("Logging"));
			loggerFactory.AddDebug();

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});

			app.UseSwagger();
			app.UseSwaggerUi(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "Markus.Feedback.Backend V1");
			});
		}
	}
}
