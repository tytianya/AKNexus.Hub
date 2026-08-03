namespace MiniMESApp.Services;

public static class BarcodeGenerator
{
    public static List<string> Generate(string productModel, int quantity)
    {
        var timePart = DateTime.Now.ToString("yyMMddHHmm");
        var results = new List<string>(quantity);
        for (int i = 1; i <= quantity; i++)
        {
            results.Add($"{productModel}{timePart}{i:D4}");
        }
        return results;
    }
}
