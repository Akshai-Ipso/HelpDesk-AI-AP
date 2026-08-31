using HelpDesk.Api.Data;
using HelpDesk.Api.Services;
using Microsoft.EntityFrameworkCore;
using HelpDesk.Api.Services;
using HelpDesk.Api.Middleware;
using System.Reflection;

namespace HelpDesk.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            var connectionString = builder.Configuration
            .GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
            "Die Connection-String-Konfiguration 'DefaultConnection' fehlt.");

            builder.Services.AddDbContext<HelpDeskDbContext>(options =>
                options.UseSqlite(connectionString));

            builder.Services.AddScoped<ITicketService, TicketService>();

            builder.Services.AddScoped<
                IKiAntwortGenerator,
                SimulierterKiAntwortGenerator>();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                var xmlDateiname =
                    $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

                var xmlPfad =
                    Path.Combine(AppContext.BaseDirectory, xmlDateiname);

                options.IncludeXmlComments(xmlPfad);
            });

            var app = builder.Build();

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
