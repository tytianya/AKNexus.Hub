using AKNexus.Framework.Application.Dtos;
using MiniMESApp.Dtos;
using MiniMESApp.Entities;
using MiniMESApp.Repositories;
using Furion.DynamicApiController;
using Microsoft.AspNetCore.Mvc;

namespace MiniMESApp.Services;

/// <summary>
/// 产品服务接口
/// </summary>
public interface IProductService
{
    /// <summary>
    /// 获取所有产品
    /// </summary>
    Task<Result<List<ProductDto>>> GetAllAsync();

    /// <summary>
    /// 根据ID获取产品
    /// </summary>
    Task<Result<ProductDto>> GetByIdAsync(int id);

    /// <summary>
    /// 创建产品
    /// </summary>
    Task<Result<ProductDto>> CreateAsync(ProductDto dto);

    /// <summary>
    /// 更新产品
    /// </summary>
    Task<Result> UpdateAsync(int id, ProductUpdateDto dto);

    /// <summary>
    /// 删除产品
    /// </summary>
    Task<Result> DeleteAsync(int id);

    /// <summary>
    /// 绑定流程
    /// </summary>
    Task<Result> BindFlowAsync(int productId, int flowId);
}

/// <summary>
/// 产品服务 - 动态 API
/// </summary>
/// <remarks>
/// 使用 IDynamicApiController，无需创建 Controller
/// 自动生成路由：/api/product/get-all-async, /api/product/get-by-id-async 等
/// </remarks>
[ApiDescriptionSettings("Product", Name = "Product")]
public class ProductService : IProductService, IDynamicApiController
{
    private readonly IProductRepository _productRepo;
    private readonly IProductFlowBindingRepository _bindingRepo;

    public ProductService(IProductRepository productRepo, IProductFlowBindingRepository bindingRepo)
    {
        _productRepo = productRepo;
        _bindingRepo = bindingRepo;
    }

    /// <summary>
    /// 获取所有产品
    /// </summary>
    /// <remarks>GET /api/product/get-all-async</remarks>
    [HttpGet("")]
    public async Task<Result<List<ProductDto>>> GetAllAsync()
    {
        var products = await _productRepo.GetAllAsync();
        var bindings = await _bindingRepo.GetAllBindingsAsync();

        var dtos = products.Select(p =>
        {
            var hasBinding = bindings.TryGetValue(p.Id, out var b);
            return new ProductDto
            {
                Id = p.Id, ProductCode = p.ProductCode, ProductName = p.ProductName,
                Spec = p.Spec, BatchNo = p.BatchNo, Status = p.Status, CreatedAt = p.CreatedAt,
                BoundFlowId = hasBinding ? b.FlowId : null,
                BoundFlowCode = hasBinding ? b.FlowCode : null
            };
        }).ToList();
        return Result<List<ProductDto>>.Ok(dtos);
    }

    /// <summary>
    /// 根据ID获取产品
    /// </summary>
    /// <remarks>GET /api/product/get-by-id-async?id={id}</remarks>
    [HttpGet("{id}")]
    public async Task<Result<ProductDto>> GetByIdAsync(int id)
    {
        var p = await _productRepo.GetById(id);
        if (p == null) return Result<ProductDto>.Fail("资源不存在");
        return Result<ProductDto>.Ok(new ProductDto
        {
            Id = p.Id, ProductCode = p.ProductCode, ProductName = p.ProductName,
            Spec = p.Spec, BatchNo = p.BatchNo, Status = p.Status, CreatedAt = p.CreatedAt
        });
    }

    /// <summary>
    /// 创建产品
    /// </summary>
    /// <remarks>POST /api/product/create-async</remarks>
    [HttpPost("")]
    public async Task<Result<ProductDto>> CreateAsync(ProductDto dto)
    {
        var product = new Product
        {
            ProductCode = dto.ProductCode,
            ProductName = dto.ProductName,
            Spec = dto.Spec,
            BatchNo = dto.BatchNo,
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        };
        product.Id = await _productRepo.InsertAsync(product);
        return Result<ProductDto>.Ok(new ProductDto
        {
            Id = product.Id, ProductCode = product.ProductCode, ProductName = product.ProductName,
            Spec = product.Spec, BatchNo = product.BatchNo, Status = product.Status, CreatedAt = product.CreatedAt
        }, "创建成功");
    }

    /// <summary>
    /// 更新产品
    /// </summary>
    /// <remarks>PUT /api/product/update-async?id={id}</remarks>
    [HttpPut("{id}")]
    public async Task<Result> UpdateAsync(int id, ProductUpdateDto dto)
    {
        var product = await _productRepo.GetById(id);
        if (product == null) return Result.Fail("产品不存在");

        if (!string.IsNullOrWhiteSpace(dto.ProductCode) && dto.ProductCode != product.ProductCode)
        {
            var existing = await _productRepo.GetByCode(dto.ProductCode);
            if (existing != null)
                return Result.Fail($"产品编码 [{dto.ProductCode}] 已被使用");
        }

        if (!string.IsNullOrWhiteSpace(dto.ProductCode)) product.ProductCode = dto.ProductCode;
        if (!string.IsNullOrWhiteSpace(dto.ProductName)) product.ProductName = dto.ProductName;
        product.Spec = dto.Spec;
        product.BatchNo = dto.BatchNo;
        if (!string.IsNullOrWhiteSpace(dto.Status)) product.Status = dto.Status;
        product.UpdatedAt = DateTime.UtcNow;

        await _productRepo.UpdateAsync(product);
        return Result.Ok("更新成功");
    }

    /// <summary>
    /// 删除产品
    /// </summary>
    /// <remarks>DELETE /api/product/delete-async?id={id}</remarks>
    [HttpDelete("{id}")]
    public async Task<Result> DeleteAsync(int id)
    {
        var product = await _productRepo.GetById(id);
        if (product == null) return Result.Fail("产品不存在");
        await _productRepo.DeleteAsync(id);
        return Result.Ok("删除成功");
    }

    /// <summary>
    /// 绑定流程
    /// </summary>
    /// <remarks>POST /api/product/bind-flow-async?productId={productId}&amp;flowId={flowId}</remarks>
    [HttpPost("{productId}/bind-flow/{flowId}")]
    public async Task<Result> BindFlowAsync(int productId, int flowId)
    {
        var existing = await _bindingRepo.GetByProductId(productId);
        if (existing != null)
        {
            existing.FlowId = flowId;
            existing.BoundAt = DateTime.UtcNow;
            await _bindingRepo.UpdateAsync(existing);
        }
        else
        {
            await _bindingRepo.InsertAsync(new ProductFlowBinding
            {
                ProductId = productId,
                FlowId = flowId,
                BoundAt = DateTime.UtcNow
            });
        }
        return Result.Ok("绑定成功");
    }
}
