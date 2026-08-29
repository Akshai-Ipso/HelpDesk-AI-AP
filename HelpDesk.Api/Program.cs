using HelpDesk.Api.Data;
using HelpDesk.Api.Services;
using Microsoft.EntityFrameworkCore;
using HelpDesk.Api.Services;

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

            builder.Services.AddSingleton<
                IKiAntwortGenerator,
                SimulierterKiAntwortGenerator>();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

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
