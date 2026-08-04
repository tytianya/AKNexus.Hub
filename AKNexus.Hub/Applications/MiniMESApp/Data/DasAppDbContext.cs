using AKNexus.Framework.Infrastructure.Data;
using MiniMESApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace MiniMESApp.Data;

/// <summary>
/// DASApp 数据库上下文
/// 继承 BaseDbContext 获得审计字段自动填充和软删除拦截
/// MES 实体不实现 ISoftDelete/IAuditable，因此全局过滤器不会作用于它们
/// </summary>
public class DasAppDbContext : BaseDbContext
{
    public DasAppDbContext(DbContextOptions<DasAppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 先调用基类（ISoftDelete 全局过滤器 + Infrastructure 程序集配置）
        base.OnModelCreating(modelBuilder);

        // 应用 DASApp 程序集内的 IEntityTypeConfiguration<T>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DasAppDbContext).Assembly);
    }

    // MES 实体 DbSet
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Station> Stations => Set<Station>();
    public DbSet<StationFlow> StationFlows => Set<StationFlow>();
    public DbSet<ProductFlowBinding> ProductFlowBindings => Set<ProductFlowBinding>();
    public DbSet<ProductStationLog> ProductStationLogs => Set<ProductStationLog>();
    public DbSet<GateRule> GateRules => Set<GateRule>();
    public DbSet<MacRange> MacRanges => Set<MacRange>();
    public DbSet<MacAllocation> MacAllocations => Set<MacAllocation>();
    public DbSet<TestSoftware> TestSoftwares => Set<TestSoftware>();
    public DbSet<StationSoftwareBinding> StationSoftwareBindings => Set<StationSoftwareBinding>();
    public DbSet<IncomingMaterial> IncomingMaterials => Set<IncomingMaterial>();
    public DbSet<MaterialBarcode> MaterialBarcodes => Set<MaterialBarcode>();
    public DbSet<BusinessLog> BusinessLogs => Set<BusinessLog>();
}
