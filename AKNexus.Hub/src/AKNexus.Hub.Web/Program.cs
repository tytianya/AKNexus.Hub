using AKNexus.Framework.Abstractions.Repositories;
using AKNexus.Framework.Abstractions.UnitOfWork;
using AKNexus.Framework.Infrastructure.Data;
using AKNexus.Framework.Infrastructure.DbContexts;
using AKNexus.Framework.Infrastructure.Entities;
using AKNexus.Framework.Infrastructure.Repositories;
using AKNexus.Framework.Infrastructure.UnitOfWork;
using AKNexus.Framework.Web.Extensions;
using AKNexus.Framework.Web.Middlewares;
using AKNexus.Hub.Web.Dtos;
using AKNexus.Hub.Web.Extensions;
using AKNexus.Hub.Web.Services;
using Furion;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// 使用 Furion 的 Inject() 初始化
var builder = WebApplication.CreateBuilder(args).Inject();

// 配置服务
ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

// 配置中间件
ConfigureMiddleware(app, builder.Configuration);

// 运行应用
app.Run();

/// <summary>
/// 配置服务
/// </summary>
void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    //核心：激活组件自主注入
    builder.Services.AddAKNexus(configuration,null);


    // 配置JWT认证
    services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var jwtConfig = configuration.GetSection("JWT");
        var key = Encoding.UTF8.GetBytes(jwtConfig["IssuerSigningKey"] ?? throw new InvalidOperationException("JWT IssuerSigningKey is not configured"));

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = jwtConfig.GetValue<bool>("ValidateIssuerSigningKey"),
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = jwtConfig.GetValue<bool>("ValidateIssuer"),
            ValidIssuer = jwtConfig["ValidIssuer"],
            ValidateAudience = jwtConfig.GetValue<bool>("ValidateAudience"),
            ValidAudience = jwtConfig["ValidAudience"],
            ValidateLifetime = jwtConfig.GetValue<bool>("ValidateLifetime"),
            ClockSkew = TimeSpan.FromMinutes(jwtConfig.GetValue<int>("ClockSkew"))
        };
    });

    // 配置Redis（如果需要）
    if (!string.IsNullOrEmpty(configuration["Redis:ConnectionString"]))
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration["Redis:ConnectionString"];
            options.InstanceName = configuration["Redis:InstanceName"];
        });
    }

    // 配置Mapster映射
    ConfigureMapper();
}

/// <summary>
/// 配置中间件
/// </summary>
void ConfigureMiddleware(WebApplication app, IConfiguration configuration)
{
    // 使用Framework Web中间件（已包含 UseInject）
    app.UseFrameworkWeb();

    // 全局异常处理
    app.UseGlobalException();

    // 开发环境配置
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseHsts();
    }
}

/// <summary>
/// 配置对象映射
/// </summary>
void ConfigureMapper()
{
    TypeAdapterConfig<User, UserDto>.NewConfig()
        .Map(dest => dest.FullName, src => $"{src.FirstName} {src.LastName}");

    TypeAdapterConfig<UserCreateInput, User>.NewConfig();
    TypeAdapterConfig<UserUpdateInput, User>.NewConfig();
}