using AKNexus.Framework.Abstractions.Repositories;
using AKNexus.Framework.Abstractions.UnitOfWork;
using AKNexus.Framework.Application.Contracts;
using AKNexus.Framework.Infrastructure.Data;
using AKNexus.Framework.Infrastructure.Repositories;
using AKNexus.Framework.Infrastructure.UnitOfWork;
using AKNexus.Framework.Web.Extensions;
using AKNexus.Framework.Web.Middlewares;
using AKNexus.Hub.Web.DbContexts;
using AKNexus.Hub.Web.Entities;
using AKNexus.Hub.Web.Repositories;
using AKNexus.Hub.Web.Dtos;
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
    // 添加Framework Web服务（已包含 AddInject）
    services.AddFrameworkWeb(configuration, frameworkServices =>
    {
        // 配置数据库上下文（支持动态切换）
        frameworkServices.AddDynamicDbContext<HubDbContext>(configuration, (options, dbType, connectionString) =>
        {
            options.EnableDetailedErrors(true);
            options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
        });

        // 注册仓储和工作单元
        frameworkServices.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        frameworkServices.AddScoped<IUnitOfWork, UnitOfWork<HubDbContext>>();

        // 注册示例服务
        frameworkServices.AddScoped<IUserService, UserService>();
    });

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

    // 添加健康检查
    var healthChecksBuilder = services.AddHealthChecks();
    
    var connectionString = configuration.GetConnectionString("Default");
    if (!string.IsNullOrEmpty(connectionString))
    {
        healthChecksBuilder.AddNpgSql(connectionString);
    }
    
    var redisConnection = configuration["Redis:ConnectionString"];
    if (!string.IsNullOrEmpty(redisConnection))
    {
        healthChecksBuilder.AddRedis(redisConnection);
    }
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

/// <summary>
/// 示例用户服务接口
/// </summary>
public interface IUserService : IServiceContract<UserDto, UserCreateInput, UserUpdateInput>
{
    Task<UserDto?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> ValidatePasswordAsync(string username, string password, CancellationToken cancellationToken = default);
}

/// <summary>
/// 示例用户服务实现
/// </summary>
public class UserService : IUserService
{
    private readonly IRepository<User> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserService> _logger;

    public UserService(IRepository<User> repository, IUnitOfWork unitOfWork, ILogger<UserService> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetByIdAsync(id, cancellationToken);
        return user?.Adapt<UserDto>();
    }

    public async Task<List<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _repository.GetAllAsync(cancellationToken);
        return users.Select(u => u.Adapt<UserDto>()).ToList();
    }

    public async Task<UserDto> CreateAsync(UserCreateInput input, CancellationToken cancellationToken = default)
    {
        var user = input.Adapt<User>();
        user.PasswordHash = HashPassword(input.Password);
        var result = await _repository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return result.Adapt<UserDto>();
    }

    public async Task UpdateAsync(Guid id, UserUpdateInput input, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetByIdAsync(id, cancellationToken);
        if (user == null)
            throw new InvalidOperationException("用户不存在");

        input.Adapt(user);
        await _repository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _repository.DeleteByIdAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserDto?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var allUsers = await _repository.GetAllAsync(cancellationToken);
        var user = allUsers.FirstOrDefault(u => u.Username == username);
        return user?.Adapt<UserDto>();
    }

    public async Task<bool> ValidatePasswordAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var allUsers = await _repository.GetAllAsync(cancellationToken);
        var user = allUsers.FirstOrDefault(u => u.Username == username);
        if (user == null) return false;
        return user.PasswordHash == HashPassword(password);
    }

    private string HashPassword(string password)
    {
        return Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(password)));
    }
}