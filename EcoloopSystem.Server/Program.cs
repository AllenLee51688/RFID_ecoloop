
using EcoloopSystem.Server.Data;
using EcoloopSystem.Server.Services;
using Microsoft.EntityFrameworkCore;

namespace EcoloopSystem.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            
            builder.Services.AddDbContext<EcoloopContext>(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 註冊 RFID 服務 (Singleton 確保硬體連線狀態一致)
            builder.Services.AddSingleton<IRfidReaderService, RfidReaderService>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp",
                    builder => builder.WithOrigins(
                            "http://localhost:5173", 
                            "https://localhost:5173",
                            "http://localhost:5000",
                            "http://localhost:3000",
                            "http://127.0.0.1:5173",
                            "https://127.0.0.1:5173")
                                      .AllowAnyMethod()
                                      .AllowAnyHeader());
            });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Seed Data
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<EcoloopContext>();
                    DbInitializer.Initialize(context);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred creating the DB.");
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // 開發環境下停用 HTTPS 重定向，避免 CORS 問題
            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }
            
            app.UseCors("AllowReactApp");

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
