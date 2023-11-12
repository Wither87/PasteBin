using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Minio;
using NLog;
using NLog.Web;
using PasteBinASP.Cache;
using PasteBinASP.Cache.Impl;
using PasteBinASP.DataProviders;
using PasteBinASP.DataProviders.Impl;
using PasteBinASP.DataSeeders;
using PasteBinASP.DbContexts;
using PasteBinASP.Middlewares;
using PasteBinASP.ObjectStorages;
using PasteBinASP.ObjectStorages.Impl;
using PasteBinASP.Repositories;
using PasteBinASP.Repositories.Impl;
using PasteBinASP.Services;
using PasteBinASP.Services.Impl;
using StackExchange.Redis;
using Quartz;
using Quartz.AspNetCore;
using PasteBinASP.Quartz.Jobs;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("init Main");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddControllersWithViews();

    #region NLog
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();
    #endregion

    #region Services
    builder.Services.AddScoped<IPasteService, PasteService>();
    builder.Services.AddScoped<IPasteObjectStorage, PasteObjectStorage>();
    builder.Services.AddScoped<IDateTimeProvider, DateTimeProvider>();
    builder.Services.AddScoped<IPasteRepository, PasteRepository>();
    builder.Services.AddScoped<IHashService, HashService>();
    builder.Services.AddScoped<IHashCache, HashCache>();
    builder.Services.AddScoped<IPasteCache, PasteCache>();
    #endregion

    #region PostgreSQL
    var connectionString = builder.Configuration.GetConnectionString("Pastebin");
    builder.Services.AddDbContext<PastebinDbContext>(options =>
        options
               .UseNpgsql(connectionString)
               .UseSnakeCaseNamingConvention()); 
    #endregion

    #region Redis
    //* добавление редиса в качестве IDistributedCache */
    var redisConnectionString = builder.Configuration.GetSection("Redis:ConnectionString").Value;
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
        options.InstanceName = "PastebinRedis";
    });

    //* добавление IConnectionMultiplexer дл€ полной работы с редисом */
    var connectionMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
    // ef core не дружит с докером( приходитс€ указывать host localhost чтобы подключитьс€
    // к Redis и PostgreSQL(
    //var connectionMultiplexer = ConnectionMultiplexer.Connect($"localhost:6379");
    builder.Services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);
    #endregion

    #region Logging
    builder.Services.AddLogging(options =>
    {
        options
            .AddConsole()
            .AddFilter(DbLoggerCategory.Database.Command.Name, Microsoft.Extensions.Logging.LogLevel.Information);

        options.AddDebug();
    });
    #endregion

    #region Mioio

    var minioConfigSection = builder.Configuration.GetSection("Minio");
    var minioEndpoint = minioConfigSection["Endpoint"];
    var minioAccessKey = minioConfigSection["AccessKey"];
    var minioSecretKey = minioConfigSection["SecretKey"];
    builder.Services.AddMinio(options => options
        .WithEndpoint(minioEndpoint)
        .WithCredentials(minioAccessKey, minioSecretKey));

    #endregion

    #region Quartz
    builder.Services.AddQuartz(options =>
    {
        var autoDeleteTextsJobKey = new JobKey(nameof(AutoDeleteTextsJob));
        options.AddJob<AutoDeleteTextsJob>(jobOptions 
            => jobOptions.WithIdentity(autoDeleteTextsJobKey));

        var autoDeleteTextsTriggerName = $"{nameof(AutoDeleteTextsJob)}-trigger";
        options.AddTrigger(triggerOptions => triggerOptions
            .ForJob(autoDeleteTextsJobKey)
            .WithIdentity(autoDeleteTextsTriggerName)
            .WithCronSchedule("0,30 * * ? * *"));
    });
    builder.Services.AddQuartzServer(options => options.WaitForJobsToComplete = true);
    #endregion

    var app = builder.Build();

    using(var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetService<PastebinDbContext>();
        var seeder = new ShortLinkDataSeeder();
        seeder.Seed(dbContext!);
    }

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
        app.UseExceptionHandler("/Home/Error");

    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });

    app.UseHttpsRedirection();
    app.UseHsts();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Paste}/{action=Index}/{id?}");

    //* ѕолучить id сервера дл€ проверки load balancer-а */
    if (app.Environment.IsDevelopment())
        app.UseMiddleware<ServerIdMiddleware>();    

    app.Run();
}
catch (Exception exception)
{
    logger.Error(exception, "ќстановка программы из-за ошибки");
    throw;
}
finally
{
    NLog.LogManager.Shutdown();
}