namespace MiniMESApp.Dtos;

public class MacAllocationResult
{
    public bool Success { get; set; }
    public string MacAddress { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public static MacAllocationResult Ok(string mac) => new() { Success = true, MacAddress = mac };
    public static MacAllocationResult Fail(string msg) => new() { Success = false, Message = msg };
}
