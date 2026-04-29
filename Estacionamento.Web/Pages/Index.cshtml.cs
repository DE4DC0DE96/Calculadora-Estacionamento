using System.Globalization;
using Estacionamento.Core.Models;
using Estacionamento.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Estacionamento.Web.Pages;

public class IndexModel : PageModel
{
    private readonly CultureInfo culture = CultureInfo.GetCultureInfo("pt-BR");
    private readonly ParkingCalculator calculator;

    public IndexModel()
    {
        Settings = ParkingSettingsProvider.Load();
        calculator = new ParkingCalculator(Settings);
    }

    public ParkingSettings Settings { get; }

    public IReadOnlyList<string> VehicleTypes => Settings.Tarifas.Keys
        .Order(StringComparer.CurrentCultureIgnoreCase)
        .ToList();

    [BindProperty]
    public ParkingInput Input { get; set; } = new();

    public ParkingCalculationResult? Result { get; private set; }

    public string? ErrorMessage { get; private set; }

    public DateTime? SaidaConsiderada { get; private set; }

    public void OnGet()
    {
        DateTime agora = DateTime.Now;
        Input.TipoVeiculo = VehicleTypes.FirstOrDefault() ?? string.Empty;
        Input.Entrada = DateTime.Today;
        Input.Saida = new DateTime(agora.Year, agora.Month, agora.Day, agora.Hour, agora.Minute, 0);
        CalculateIfPossible();
    }

    public void OnPost()
    {
        CalculateIfPossible();
    }

    public JsonResult OnPostCalculate()
    {
        CalculateIfPossible();
        return new JsonResult(CreateResponse());
    }

    public string FormatCurrency(decimal value)
    {
        return value.ToString("C", culture);
    }

    public string FormatChargedHours(int? hours)
    {
        return hours switch
        {
            null => "-",
            0 => "Gr\u00e1tis",
            1 => "1 hora",
            _ => $"{hours} horas"
        };
    }

    public string FormatDuration(TimeSpan? duration)
    {
        if (duration is null)
        {
            return "-";
        }

        int totalHours = (int)Math.Floor(duration.Value.TotalHours);
        int minutes = duration.Value.Minutes;

        if (totalHours == 0)
        {
            return $"{minutes} min";
        }

        return $"{totalHours}h {minutes:D2}min";
    }

    public string FormatDateTime(DateTime? dateTime)
    {
        return dateTime?.ToString("dd/MM/yyyy HH:mm", culture) ?? "-";
    }

    private void CalculateIfPossible()
    {
        try
        {
            SaidaConsiderada = Input.Saida;
            Result = calculator.Calculate(Input.TipoVeiculo, Input.Entrada, Input.Saida);
            ErrorMessage = null;
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or FileNotFoundException)
        {
            Result = null;
            SaidaConsiderada = Input.Saida;
            ErrorMessage = ex.Message;
        }
    }

    private object CreateResponse()
    {
        return new
        {
            success = string.IsNullOrWhiteSpace(ErrorMessage),
            errorMessage = ErrorMessage,
            result = new
            {
                amount = FormatCurrency(Result?.ValorTotal ?? 0m),
                duration = FormatDuration(Result?.Permanencia),
                minutes = Result?.MinutosConsiderados.ToString() ?? "-",
                hours = FormatChargedHours(Result?.HorasCobradas),
                exit = FormatDateTime(SaidaConsiderada)
            }
        };
    }

    public sealed class ParkingInput
    {
        public string TipoVeiculo { get; set; } = string.Empty;

        public DateTime Entrada { get; set; }

        public DateTime Saida { get; set; }
    }
}
