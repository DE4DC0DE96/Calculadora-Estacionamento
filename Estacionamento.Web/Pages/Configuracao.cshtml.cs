using Estacionamento.Core.Models;
using Estacionamento.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;

namespace Estacionamento.Web.Pages;

public class ConfiguracaoModel : PageModel
{
    private readonly IWebHostEnvironment environment;

    public ConfiguracaoModel(IWebHostEnvironment environment)
    {
        this.environment = environment;
    }

    [BindProperty]
    public ConfiguracaoInput Input { get; set; } = new();

    public string? SuccessMessage { get; private set; }

    public string? ErrorMessage { get; private set; }

    public void OnGet()
    {
        LoadInput();
    }

    public void OnPost()
    {
        try
        {
            ParkingSettings settings = new()
            {
                ToleranciaSaidaGratuitaMinutos = Input.ToleranciaSaidaGratuitaMinutos,
                ToleranciaDemaisHorasMinutos = Input.ToleranciaDemaisHorasMinutos,
                Tarifas = Input.Tarifas.ToDictionary(
                    item => item.TipoVeiculo,
                    item => new ParkingRate
                    {
                        PrimeiraHora = ParseMoney(item.PrimeiraHora),
                        DemaisHoras = ParseMoney(item.DemaisHoras)
                    },
                    StringComparer.OrdinalIgnoreCase)
            };

            ParkingSettingsProvider.Save(settings);
            string projectSettingsPath = Path.Combine(environment.ContentRootPath, "tarifas-estacionamento.json");
            ParkingSettingsProvider.Save(settings, projectSettingsPath);
            LoadInput();
            SuccessMessage = "Configura\u00e7\u00e3o salva com sucesso.";
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or IOException)
        {
            ErrorMessage = ex.Message;
        }
    }

    private void LoadInput()
    {
        ParkingSettings settings = ParkingSettingsProvider.Load();
        Input = new ConfiguracaoInput
        {
            ToleranciaSaidaGratuitaMinutos = settings.ToleranciaSaidaGratuitaMinutos,
            ToleranciaDemaisHorasMinutos = settings.ToleranciaDemaisHorasMinutos,
            Tarifas = settings.Tarifas
                .OrderBy(item => item.Key, StringComparer.CurrentCultureIgnoreCase)
                .Select(item => new TarifaInput
                {
                    TipoVeiculo = item.Key,
                    PrimeiraHora = FormatMoneyInput(item.Value.PrimeiraHora),
                    DemaisHoras = FormatMoneyInput(item.Value.DemaisHoras)
                })
                .ToList()
        };
    }

    private static string FormatMoneyInput(decimal value)
    {
        return value.ToString("0.00", CultureInfo.InvariantCulture);
    }

    private static decimal ParseMoney(string value)
    {
        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal invariantValue))
        {
            return invariantValue;
        }

        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.GetCultureInfo("pt-BR"), out decimal brazilianValue))
        {
            return brazilianValue;
        }

        throw new InvalidOperationException("Informe valores monet\u00e1rios v\u00e1lidos.");
    }

    public sealed class ConfiguracaoInput
    {
        public int ToleranciaSaidaGratuitaMinutos { get; set; }

        public int ToleranciaDemaisHorasMinutos { get; set; }

        public List<TarifaInput> Tarifas { get; set; } = [];
    }

    public sealed class TarifaInput
    {
        public string TipoVeiculo { get; set; } = string.Empty;

        public string PrimeiraHora { get; set; } = "0.00";

        public string DemaisHoras { get; set; } = "0.00";
    }
}
