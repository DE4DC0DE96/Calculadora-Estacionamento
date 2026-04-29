namespace Estacionamento.Core.Models;

public sealed class ParkingRate
{
    public decimal PrimeiraHora { get; set; }

    public decimal DemaisHoras { get; set; }
}
