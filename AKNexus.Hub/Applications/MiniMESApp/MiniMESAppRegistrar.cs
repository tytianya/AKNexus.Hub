using AKNexus.Core;
using MiniMESApp.Data;
using MiniMESApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: AssemblyRegistrar(typeof(MiniMESApp.MiniMESAppRegistrar))]
namespace MiniMESApp;

/// <summary>
/// DASApp 组件注册器
/// </summary>
public class MiniMESAppRegistrar : IAssemblyRegistrar
{
    public int Order => 100;

    public void Configure(IKernelBuilder builder)
    {
    var services = builder.Services;
    var configuration = builder.Configuration;

    // 注册 DASApp DbContext（使用独立的 mes_gate 数据库）
    var connectionStringName = configuration["DASApp:ConnectionStringName"] ?? "DASApp";
    var connectionString = configuration.GetConnectionString(connectionStringName)
        ?? "Host=localhost;Port=5432;Database=mes_gate;Username=postgres;Password=postgres";

    services.AddDbContext<DasAppDbContext>(options =>
    {
        options.UseNpgsql(connectionString, npgsql =>
        {
            npgsql.EnableRetryOnFailure(3);
            npgsql.CommandTimeout(30);
        });
        // 开发环境显示详细错误
        #if DEBUG
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
        #endif
    });

    // 注册 DASApp 仓储（Scoped）
    RegisterRepositories(services);

    // 注册 DASApp 服务（Scoped）
    RegisterServices(services);

    // 自动迁移数据库
    services.AddHostedService<DbMigrationService>();
    }

    /// <summary>
    /// 注册所有仓储
    /// </summary>
    private static void RegisterRepositories(IServiceCollection services)
    {
    // 所有仓储使用 Scoped 生命周期
    services.AddScoped<MiniMESApp.Repositories.IProductRepository, MiniMESApp.Repositories.ProductRepository>();
    services.AddScoped<MiniMESApp.Repositories.IStationRepository, MiniMESApp.Repositories.StationRepository>();
    services.AddScoped<MiniMESApp.Repositories.IStationFlowRepository, MiniMESApp.Repositories.StationFlowRepository>();
    services.AddScoped<MiniMESApp.Repositories.IProductFlowBindingRepository, MiniMESApp.Repositories.ProductFlowBindingRepository>();
    services.AddScoped<MiniMESApp.Repositories.IProductStationLogRepository, MiniMESApp.Repositories.ProductStationLogRepository>();
    services.AddScoped<MiniMESApp.Repositories.IGateRuleRepository, MiniMESApp.Repositories.GateRuleRepository>();
    services.AddScoped<MiniMESApp.Repositories.IMacRangeRepository, MiniMESApp.Repositories.MacRangeRepository>();
    services.AddScoped<MiniMESApp.Repositories.IMacAllocationRepository, MiniMESApp.Repositories.MacAllocationRepository>();
    services.AddScoped<MiniMESApp.Repositories.ITestSoftwareRepository, MiniMESApp.Repositories.TestSoftwareRepository>();
    services.AddScoped<MiniMESApp.Repositories.IStationSoftwareBindingRepository, MiniMESApp.Repositories.StationSoftwareBindingRepository>();
    services.AddScoped<MiniMESApp.Repositories.IIncomingMaterialRepository, MiniMESApp.Repositories.IncomingMaterialRepository>();
    services.AddScoped<MiniMESApp.Repositories.IMaterialBarcodeRepository, MiniMESApp.Repositories.MaterialBarcodeRepository>();
    }

    /// <summary>
    /// 注册业务服务
    /// </summary>
    private static void RegisterServices(IServiceCollection services)
    {
    services.AddScoped<MiniMESApp.Services.GateCheckProvider>();
    services.AddScoped<MiniMESApp.Services.MacAllocationService>();
    }
}
