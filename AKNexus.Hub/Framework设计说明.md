# AKNexus Framework 框架设计说明

## 概述

AKNexus Framework 是一个基于 Furion 框架的模块化后端架构，采用 DDD（领域驱动设计）思想，严格遵循分层架构原则，提供 PostgreSQL 动态数据库切换能力。

## 架构原则

### 1. 分层架构

```
┌─────────────────────────────────────────────────────────────┐
│                    AKNexus.Hub.Web                           │
│                   (主应用入口层)                               │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                  AKNexus.Framework.Web                       │
│                   (Web 基础层)                                │
│  • API 控制器基类  • Swagger 配置  • JWT 认证  • 中间件      │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│              AKNexus.Framework.Application                   │
│                   (应用服务层)                                │
│  • 应用服务基类  • DTO 规范  • 服务契约  • 业务编排           │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                AKNexus.Framework.Domain                      │
│                   (领域模型层)                                │
│  • 实体基类  • 值对象  • 领域事件  • 业务规则                 │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│            AKNexus.Framework.Infrastructure                  │
│                   (基础设施层)                                │
│  • 仓储实现  • 工作单元  • 数据库上下文  • 动态数据库切换      │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│            AKNexus.Framework.Abstractions                    │
│                   (抽象定义层)                                │
│  • 接口规范  • 契约模型  • 基础标记  • 开发标准               │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│              AKNexus.Framework.Shared                        │
│                   (公共资源层)                                │
│  • 常量定义  • 枚举类型  • 扩展方法  • 公共工具               │
└─────────────────────────────────────────────────────────────┘
```

### 2. 依赖关系

```
Web → Application → Domain → Abstractions
  │                                         ↑
  └─────────── Infrastructure ──────────────┘
                      │
                      ▼
                  Shared (被所有层引用)
```

**依赖规则**：
- 上层依赖下层，下层不依赖上层
- Abstractions 层不依赖任何框架层
- Domain 层仅依赖 Abstractions
- Infrastructure 层实现 Abstractions 定义的接口

---

## 项目详细说明

### 1. AKNexus.Framework.Abstractions

**定位**：Framework 最底层抽象定义层

**职责**：
- 定义平台统一的接口规范
- 定义契约模型（DTO 基类）
- 定义基础标记接口
- 定义开发标准

**核心内容**：

#### 实体接口
```csharp
// 实体基础接口
public interface IEntity { Guid Id { get; } }

// 软删除接口
public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    DateTimeOffset? DeletedTime { get; set; }
}

// 审计接口
public interface IAuditable
{
    Guid? CreatedBy { get; set; }
    DateTimeOffset CreatedTime { get; set; }
    Guid? UpdatedBy { get; set; }
    DateTimeOffset? UpdatedTime { get; set; }
}
```

#### 仓储接口
```csharp
public interface IRepository<TEntity, TKey> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(TKey id, CancellationToken cancellationToken = default);
}
```

#### 工作单元接口
```csharp
public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
```

#### 分页模型
```csharp
public class PagedRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class PagedResult<T>
{
    public List<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; }
    public bool HasPreviousPage { get; }
    public bool HasNextPage { get; }
}
```

**设计原则**：
- ✅ 仅定义抽象，不提供实现
- ✅ 不依赖任何基础设施
- ✅ 不包含业务逻辑
- ✅ 保持最小依赖原则

---

### 2. AKNexus.Framework.Domain

**定位**：Framework 领域模型基础层

**职责**：
- 定义领域对象基础规范
- 提供领域模型公共能力
- 建立业务对象模型

**核心内容**：

#### 实体基类
```csharp
// 基础实体
public abstract class EntityBase : IEntity
{
    public Guid Id { get; protected set; }
    // 重写 Equals、GetHashCode、==、!= 运算符
}

// 软删除实体
public abstract class SoftDeleteEntity : EntityBase, ISoftDelete
{
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedTime { get; set; }
}

// 完整审计实体（含软删除）
public abstract class FullAuditEntity : SoftDeleteEntity, IAuditable
{
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTimeOffset? UpdatedTime { get; set; }
}

// 审计实体（不含软删除）
public abstract class AuditEntity : EntityBase, IAuditable
{
    // 审计字段...
}
```

#### 值对象基类
```csharp
public abstract class ValueObject
{
    protected abstract IEnumerable<object?> GetAtomicValues();
    // 重写 Equals、GetHashCode、==、!= 运算符
}
```

#### 领域事件
```csharp
public interface IDomainEvent
{
    DateTimeOffset OccurredOn { get; }
}

public abstract class DomainEventBase : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}

public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}
```

**设计原则**：
- ✅ 面向业务模型设计
- ✅ 不依赖具体数据库实现
- ✅ 不依赖 Web 框架
- ✅ 保持领域对象纯净性

---

### 3. AKNexus.Framework.Application

**定位**：Framework 应用服务基础层

**职责**：
- 定义应用服务开发规范
- 提供统一的应用服务基类
- 定义数据传输规范
- 提供业务编排支持

**核心内容**：

#### 应用服务基类
```csharp
public abstract class AppServiceBase<TEntity, TKey, TDto, TCreateInput, TUpdateInput, TRepository>
{
    protected readonly TRepository Repository;
    protected readonly IUnitOfWork UnitOfWork;
    
    public virtual async Task<TDto?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
    public virtual async Task<List<TDto>> GetAllAsync(CancellationToken cancellationToken = default);
    public virtual async Task<PagedResult<TDto>> GetPagedListAsync(PagedRequest request, CancellationToken cancellationToken = default);
    public virtual async Task<TDto> CreateAsync(TCreateInput input, CancellationToken cancellationToken = default);
    public virtual async Task UpdateAsync(TKey id, TUpdateInput input, CancellationToken cancellationToken = default);
    public virtual async Task DeleteAsync(TKey id, CancellationToken cancellationToken = default);
}
```

#### 统一响应模型
```csharp
public class Result<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T? Data { get; set; }
    public string? ErrorCode { get; set; }
    
    public static Result<T> Ok(T data, string message = "操作成功");
    public static Result<T> Fail(string message, string? errorCode = null);
}

public class Result
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string? ErrorCode { get; set; }
    
    public static Result Ok(string message = "操作成功");
    public static Result Fail(string message, string? errorCode = null);
}
```

#### 服务契约
```csharp
public interface IServiceContract<TDto, in TCreateInput, in TUpdateInput>
{
    Task<TDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<TDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TDto> CreateAsync(TCreateInput input, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, TUpdateInput input, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
```

**设计原则**：
- ✅ 面向业务用例开发
- ✅ 不负责数据存储实现
- ✅ 不直接处理 HTTP 请求
- ✅ 不承担基础设施职责

---

### 4. AKNexus.Framework.Infrastructure

**定位**：Framework 基础设施抽象层

**职责**：
- 定义平台基础设施标准
- 提供数据库访问实现
- 统一管理外部资源能力
- 实现 PostgreSQL 动态切换

**核心内容**：

#### 数据库类型定义
```csharp
public enum DatabaseType
{
    PostgreSQL,
    SqlServer,
    MySql,
    Sqlite
}
```

#### 动态数据库切换
```csharp
public class DatabaseConnectionConfig
{
    public DatabaseType DatabaseType { get; set; } = DatabaseType.PostgreSQL;
    public string ConnectionString { get; set; }
    public string ConnectionStringName { get; set; } = "Default";
}

public static class DbContextFactory
{
    public static IServiceCollection AddDynamicDbContext<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder, DatabaseType, string>? optionsAction = null);
}

public interface IDbContextSwitcher
{
    Task SwitchDatabaseAsync(DatabaseType databaseType, string connectionString);
    DatabaseType GetCurrentDatabaseType();
}
```

#### 数据库上下文基类
```csharp
public abstract class BaseDbContext : DbContext
{
    // 自动应用配置
    protected override void OnModelCreating(ModelBuilder modelBuilder);
    
    // 全局软删除过滤器
    modelBuilder.Entity(entityType).HasQueryFilter(e => !e.IsDeleted);
    
    // 自动更新审计字段
    private void UpdateAuditEntities();
}
```

#### 仓储实现
```csharp
public abstract class EfRepositoryBase<TEntity, TKey, TDbContext> : IRepository<TEntity, TKey>
{
    protected readonly TDbContext DbContext;
    protected readonly DbSet<TEntity> DbSet;
    
    public virtual async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    // ... 其他方法实现
}
```

#### 工作单元实现
```csharp
public class UnitOfWork<TDbContext> : IUnitOfWork where TDbContext : DbContext
{
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

**设计原则**：
- ✅ 面向基础设施设计
- ✅ 不承载业务逻辑
- ✅ 不直接依赖业务模块
- ✅ 所有外部资源统一抽象

---

### 5. AKNexus.Framework.Web

**定位**：Framework Web 基础层

**职责**：
- 提供 Web 开发规范
- 统一 HTTP 服务公共能力
- 管理接口开发标准
- 提供中间件支持

**核心内容**：

#### API 控制器基类
```csharp
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult Success<T>(T data, string message = "操作成功");
    protected IActionResult Success(string message = "操作成功");
    protected IActionResult Fail(string message, string? errorCode = null, HttpStatusCode statusCode = HttpStatusCode.BadRequest);
    protected IActionResult NotFoundResult(string message = "资源不存在");
    protected IActionResult Paged<T>(PagedResult<T> result);
}
```

#### Web 应用扩展
```csharp
public static class WebApplicationExtensions
{
    // 配置服务
    public static IServiceCollection AddFrameworkWeb(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IServiceCollection>? configure = null)
    {
        services.AddFurion();
        services.AddSwaggerGen();
        services.AddControllers();
        services.AddApiVersioning();
        services.AddCors();
        services.AddHealthChecks();
        configure?.Invoke(services);
        return services;
    }
    
    // 配置中间件
    public static WebApplication UseFrameworkWeb(this WebApplication app)
    {
        app.UseSwagger();
        app.UseCors();
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapHealthChecks("/health");
        app.MapControllers();
        return app;
    }
}
```

#### 全局异常中间件
```csharp
public class GlobalExceptionMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }
}
```

**设计原则**：
- ✅ 面向 HTTP 服务
- ✅ 不实现业务逻辑
- ✅ 不直接访问数据库
- ✅ 保持接口层轻量化

---

### 6. AKNexus.Framework.Shared

**定位**：Framework 公共资源层

**职责**：
- 存放平台公共对象
- 提供基础资源支持
- 定义通用工具类

**核心内容**：

#### 系统常量
```csharp
public static class SystemConstants
{
    public const string SystemName = "AKNexus Hub";
    public const string Version = "1.0.0";
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
}

public static class ErrorCodes
{
    public const string NotFound = "NOT_FOUND";
    public const string BadRequest = "BAD_REQUEST";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string InternalError = "INTERNAL_ERROR";
}
```

#### 枚举定义
```csharp
public enum Status { Disabled = 0, Enabled = 1 }
public enum DeleteMark { NotDeleted = 0, Deleted = 1 }
public enum Boolean { No = 0, Yes = 1 }
```

#### 扩展方法
```csharp
// 集合扩展
public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source);
public static void ForEach<T>(this IEnumerable<T> source, Action<T> action);

// 字符串扩展
public static bool IsNullOrWhiteSpace(this string? value);
public static Guid ToGuid(this string? value);

// 日期时间扩展
public static long ToTimestamp(this DateTimeOffset dateTime);
public static bool IsToday(this DateTimeOffset dateTime);
```

**设计原则**：
- ✅ 保持纯公共属性
- ✅ 不包含业务模型
- ✅ 不包含框架实现
- ✅ 不依赖基础设施

---

## 主应用层 (AKNexus.Hub.Web)

**定位**：应用入口和业务实现层

**职责**：
- 应用启动配置
- 业务模块实现
- 数据库上下文定义
- 控制器实现

**核心内容**：

#### Program.cs 启动配置
```csharp
var builder = WebApplication.CreateBuilder(args);

// 配置服务
services.AddFrameworkWeb(configuration, frameworkServices =>
{
    frameworkServices.AddDynamicDbContext<HubDbContext>(configuration);
    frameworkServices.AddScoped(typeof(IRepository<>), typeof(Repository<>));
    frameworkServices.AddScoped<IUnitOfWork, UnitOfWork<HubDbContext>>();
});

// 配置 JWT 认证
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { /* JWT 配置 */ });

// 配置健康检查
services.AddHealthChecks()
    .AddNpgSql(connectionString)
    .AddRedis(redisConnection);

var app = builder.Build();
app.UseFrameworkWeb();
app.Run();
```

#### 数据库上下文
```csharp
public class HubDbContext : BaseDbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 实体配置
        modelBuilder.Entity<User>(entity => { /* 配置 */ });
    }
}
```

#### 业务实体
```csharp
public class User : FullAuditEntity
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public int Status { get; set; }
}
```

---

## 使用示例

### 1. 创建新实体

```csharp
// 继承合适的基类
public class Product : FullAuditEntity
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
}
```

### 2. 创建 DTO

```csharp
public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

public class ProductCreateInput
{
    [Required] public string Name { get; set; }
    [Range(0, double.MaxValue)] public decimal Price { get; set; }
}
```

### 3. 创建服务

```csharp
public interface IProductService : IServiceContract<ProductDto, ProductCreateInput, ProductUpdateInput>
{
}

public class ProductService : AppServiceBase<Product, Guid, ProductDto, ProductCreateInput, ProductUpdateInput, IRepository<Product>>, IProductService
{
    public ProductService(IRepository<Product> repository, IUnitOfWork unitOfWork, ILogger<ProductService> logger) 
        : base(repository, unitOfWork, logger) { }
}
```

### 4. 创建控制器

```csharp
[ApiVersion("1.0")]
public class ProductsController : ApiControllerBase
{
    private readonly IProductService _productService;
    
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var products = await _productService.GetAllAsync(ct);
        return Success(products);
    }
}
```

---

## 技术栈

| 组件 | 版本 | 说明 |
|------|------|------|
| .NET | 8.0 | 运行时框架 |
| Furion | 4.9.8.87 | 应用框架 |
| EF Core | 8.0.27 | ORM 框架 |
| PostgreSQL | 8.0.4 | 主数据库 |
| Mapster | 1.13.0 | 对象映射 |
| Swagger | 6.9.0 | API 文档 |
| JWT Bearer | 8.0.14 | 认证授权 |
| Redis | 2.8.24 | 缓存服务 |

---

## 总结

AKNexus Framework 通过清晰的分层架构，实现了：
1. **职责分离**：每层只负责自己的核心职责
2. **依赖解耦**：上层依赖下层，下层不依赖上层
3. **易于测试**：抽象层定义接口，实现层可替换
4. **可扩展性**：Modules 目录预留业务模块扩展
5. **开箱即用**：提供完整的基类和工具支持

这套架构适用于中大型项目的快速开发，能够有效控制项目复杂度，保证代码质量和可维护性。