using System.Text.Json;
using Estacionamento.Core.Models;

namespace Estacionamento.Core.Services;

public static class ParkingSettingsProvider
{
    private const string FileName = "tarifas-estacionamento.json";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public static ParkingSettings Load()
    {
        string path = ResolvePath();
        return Load(path);
    }

    public static ParkingSettings Load(string path)
    {
        string json = File.ReadAllText(path);
        ParkingSettings? settings = JsonSerializer.Deserialize<ParkingSettings>(json, JsonOptions);

        Validate(settings, path);
        settings!.Tarifas = new Dictionary<string, ParkingRate>(settings.Tarifas, StringComparer.OrdinalIgnoreCase);
        return settings;
    }

    public static void Save(ParkingSettings settings)
    {
        string path = ResolvePath();
        Save(settings, path);
    }

    public static void Save(ParkingSettings settings, string path)
    {
        Validate(settings, path);
        settings.Tarifas = new Dictionary<string, ParkingRate>(settings.Tarifas, StringComparer.OrdinalIgnoreCase);
        string json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(path, json);
    }

    private static string ResolvePath()
    {
        string[] candidates =
        [
            Path.Combine(AppContext.BaseDirectory, FileName),
            Path.Combine(Directory.GetCurrentDirectory(), FileName)
        ];

        string? path = candidates.FirstOrDefault(File.Exists);
        if (path is null)
        {
            throw new FileNotFoundException($"Arquivo de configura\u00e7\u00e3o n\u00e3o encontrado: {FileName}");
        }

        return path;
    }

    private static void Validate(ParkingSettings? settings, string path)
    {
        if (settings is null)
        {
            throw new InvalidOperationException($"O arquivo {path} n\u00e3o possui uma configura\u00e7\u00e3o valida.");
        }

        if (settings.ToleranciaSaidaGratuitaMinutos < 0)
        {
            throw new InvalidOperationException("A toler\u00e2ncia de sa\u00edda gratuita n\u00e3o pode ser negativa.");
        }

        if (settings.ToleranciaDemaisHorasMinutos < 0)
        {
            throw new InvalidOperationException("A toler\u00e2ncia das demais horas n\u00e3o pode ser negativa.");
        }

        if (settings.Tarifas.Count == 0)
        {
            throw new InvalidOperationException("Configure ao menos uma tarifa.");
        }

        foreach ((string tipo, ParkingRate tarifa) in settings.Tarifas)
        {
            if (tarifa.PrimeiraHora < 0 || tarifa.DemaisHoras < 0)
            {
                throw new InvalidOperationException($"A tarifa de {tipo} n\u00e3o pode ter valores negativos.");
            }
        }
    }
}
