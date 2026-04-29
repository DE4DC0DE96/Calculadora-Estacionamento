using Estacionamento.Core.Models;

namespace Estacionamento.Core.Services;

public sealed class ParkingCalculator
{
    private readonly ParkingSettings settings;

    public ParkingCalculator(ParkingSettings settings)
    {
        this.settings = settings;
    }

    public ParkingCalculationResult Calculate(string tipoVeiculo, DateTime entrada, DateTime saida)
    {
        if (string.IsNullOrWhiteSpace(tipoVeiculo))
        {
            throw new ArgumentException("Selecione o tipo de ve\u00edculo.", nameof(tipoVeiculo));
        }

        if (saida <= entrada)
        {
            throw new ArgumentException("A sa\u00edda deve ser posterior a entrada.");
        }

        if (!settings.Tarifas.TryGetValue(tipoVeiculo, out ParkingRate? tarifa))
        {
            throw new ArgumentException($"N\u00e3o ha tarifa configurada para {tipoVeiculo}.");
        }

        TimeSpan permanencia = saida - entrada;
        int minutos = Math.Max(1, (int)Math.Ceiling(permanencia.TotalMinutes));
        int horasCobradas = CalculateChargedHours(minutos);
        decimal valor = horasCobradas == 0
            ? 0m
            : tarifa.PrimeiraHora + Math.Max(0, horasCobradas - 1) * tarifa.DemaisHoras;

        return new ParkingCalculationResult(tipoVeiculo, permanencia, minutos, horasCobradas, valor);
    }

    private int CalculateChargedHours(int minutos)
    {
        if (minutos <= settings.ToleranciaSaidaGratuitaMinutos)
        {
            return 0;
        }

        int minutosComTolerancia = Math.Max(1, minutos - settings.ToleranciaDemaisHorasMinutos);
        return Math.Max(1, (int)Math.Ceiling(minutosComTolerancia / 60m));
    }
}
