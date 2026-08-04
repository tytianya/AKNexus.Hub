using MiniMESApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace MiniMESApp.Services;

public class DbMigrationService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public DbMigrationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DasAppDbContext>();

        // 1. 全新数据库：EnsureCreatedAsync 一次性建好所有表
        await db.Database.EnsureCreatedAsync(cancellationToken);

        // 2. 已有数据库：检查并补建后续新增的表
        await EnsureNewTablesAsync(db, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>
    /// 补建 EnsureCreatedAsync 遗漏的新增表。
    /// 利用 EF Core 的 GenerateCreateScript() 生成全量 DDL，
    /// 按表拆分后逐个检查 information_schema，只创建缺失的表。
    /// </summary>
    private static async Task EnsureNewTablesAsync(DasAppDbContext db, CancellationToken ct)
    {
        // EF Core 生成的完整建库脚本（含所有已注册实体的 CREATE TABLE 语句）
        var fullScript = db.Database.GenerateCreateScript();
        if (string.IsNullOrWhiteSpace(fullScript))
            return;

        // 按表拆分：每段以 CREATE TABLE 开头
        var tableBlocks = Regex.Split(fullScript, @"(?=CREATE\s+TABLE\s+""?)", RegexOptions.IgnoreCase)
            .Where(s => !string.IsNullOrWhiteSpace(s)
                        && Regex.IsMatch(s, @"CREATE\s+TABLE\s+", RegexOptions.IgnoreCase))
            .ToList();

        if (tableBlocks.Count == 0)
            return;

        foreach (var block in tableBlocks)
        {
            // 提取表名（兼容带引号和不带引号）
            var match = Regex.Match(block, @"CREATE\s+TABLE\s+(?:public\.)?""?([^""\s(]+)""?\s*\(",
                RegexOptions.IgnoreCase);
            if (!match.Success)
                continue;

            var tableName = match.Groups[1].Value;

            // 检查该表是否已存在（用 COUNT 返回 int，避免 SqlQueryRaw<bool> 映射问题）
            // 注意：SqlQueryRaw 会将 SQL 包进子查询，末尾不能带分号

            var existSql = $"""
                                SELECT EXISTS (
                    SELECT 1
                    FROM information_schema.tables
                    WHERE table_schema = 'public'
                      AND table_name = '{tableName}'
                ) AS "Value"
                """;
            var exists = await db.Database
    .SqlQueryRaw<bool>(existSql)
    .FirstOrDefaultAsync(ct);
            if (exists)
                continue;
            

            // 取当前块到对应分号为止（CREATE TABLE ... ;），作为独立语句执行
            var endIdx = block.IndexOf(';');
            var createStatement = endIdx >= 0 ? block.Substring(0, endIdx + 1) : block.Trim();

            // 补建单表
            await db.Database.ExecuteSqlRawAsync(createStatement, ct);
            Console.WriteLine($"[DbMigration] 补建表: {tableName}");
        }
    }
}
