# AKNexus Hub

基于 Furion 框架的模块化后端架构，支持 PostgreSQL 动态切换数据库。

## 项目结构

```
AKNexus.Hub/
├── src/
│   ├── Framework/                    # 框架层
│   │   ├── AKNexus.Framework.Abstractions/     # 抽象定义层
│   │   ├── AKNexus.Framework.Domain/           # 领域模型层
│   │   ├── AKNexus.Framework.Application/      # 应用服务层
│   │   ├── AKNexus.Framework.Infrastructure/   # 基础设施层
│   │   ├── AKNexus.Framework.Web/              # Web基础层
│   │   └── AKNexus.Framework.Shared/           # 公共资源层
│   ├── AKNexus.Hub.Web/              # 主应用
│   └── Modules/                      # 业务模块（预留）
└── README.md
```

## 框架层级说明

### AKNexus.Framework.Abstractions
- **定位**: Framework 最底层抽象定义层
- **职责**: 定义平台统一的接口规范、契约模型、基础标记及开发标准
- **设计原则**: 仅定义抽象，不提供实现；不依赖任何基础设施

### AKNexus.Framework.Domain
- **定位**: Framework 领域模型基础层
- **职责**: 定义领域对象基础规范及领域模型公共能力
- **设计原则**: 面向业务模型设计；不依赖具体数据库实现

### AKNexus.Framework.Application
- **定位**: Framework 应用服务基础层
- **职责**: 定义应用服务开发规范，提供统一的应用服务基类和数据传输规范
- **设计原则**: 面向业务用例开发；不负责数据存储实现

### AKNexus.Framework.Infrastructure
- **定位**: Framework 基础设施抽象层
- **职责**: 定义平台基础设施标准，统一管理外部资源能力
- **核心功能**: 
  - PostgreSQL 动态切换支持
  - EF Core 仓储基类实现
  - 工作单元管理
  - 数据库上下文基类

### AKNexus.Framework.Web
- **定位**: Framework Web 基础层
- **职责**: 提供 Web 开发规范及 HTTP 服务公共能力
- **核心功能**:
  - API 控制器基类
  - Swagger 配置
  - JWT 认证配置
  - 全局异常处理

### AKNexus.Framework.Shared
- **定位**: Framework 公共资源层
- **职责**: 存放整个平台范围内可复用的公共对象及基础资源
- **内容**: 常量、枚举、扩展方法

## 技术栈

- **.NET**: 8.0
- **框架**: Furion 4.9.8.87
- **数据库**: PostgreSQL (Npgsql.EntityFrameworkCore.PostgreSQL 8.0.4)
- **ORM**: Entity Framework Core 8.0.14
- **对象映射**: Mapster 1.13.0
- **缓存**: StackExchange.Redis 2.8.24
- **认证**: JWT Bearer

## 数据库动态切换

框架支持在运行时动态切换数据库类型：

```csharp
// 配置数据库连接
public class DatabaseConnectionConfig
{
    public DatabaseType DatabaseType { get; set; }
    public string ConnectionString { get; set; }
    public string ConnectionStringName { get; set; }
}

// 支持的数据库类型
public enum DatabaseType
{
    PostgreSQL,
    SqlServer,
    MySql,
    Sqlite
}
```

## 快速开始

### 1. 配置数据库连接

修改 `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=AKNexusHub;Username=postgres;Password=your_password"
  }
}
```

### 2. 运行数据库迁移

```bash
cd AKNexus.Hub.Web
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 3. 启动应用

```bash
cd AKNexus.Hub.Web
dotnet run
```

### 4. 访问 Swagger

打开浏览器访问: `http://localhost:5000/swagger`

## 设计原则

1. **分层架构**: 严格遵循职责分层，保证各层边界清晰
2. **依赖倒置**: 高层模块不依赖低层模块，都依赖抽象
3. **开闭原则**: 对扩展开放，对修改关闭
4. **单一职责**: 每个类和模块只负责一个职责
5. **模块化设计**: Framework 提供骨架层，Modules 存放业务模块

## 示例功能

项目包含用户管理示例，展示了：
- 实体定义（支持审计和软删除）
- 仓储模式
- 应用服务
- API 控制器
- DTO 定义
- 对象映射

## 许可证

MIT License