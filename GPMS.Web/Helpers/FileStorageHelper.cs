using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace GPMS.Web.Helpers;

public static class FileStorageHelper
{
    private static readonly string BasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");

    static FileStorageHelper()
    {
        if (!Directory.Exists(BasePath))
        {
            Directory.CreateDirectory(BasePath);
        }
    }

    public static async Task SaveAsync<T>(string fileName, T data)
    {
        var filePath = Path.Combine(BasePath, fileName);
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(filePath, json);
    }

    public static async Task<T?> LoadAsync<T>(string fileName)
    {
        var filePath = Path.Combine(BasePath, fileName);
        if (!File.Exists(filePath)) return default;

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
            return default;
        }
    }
}
