namespace Estacionamento.Core.Services;

public sealed record ParkingCalculationResult(
    string TipoVeiculo,
    TimeSpan Permanencia,
    int MinutosConsiderados,
    int HorasCobradas,
    decimal ValorTotal);
