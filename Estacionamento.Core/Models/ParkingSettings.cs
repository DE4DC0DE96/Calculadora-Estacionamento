namespace Estacionamento.Core.Models;

public sealed class ParkingSettings
{
    public int ToleranciaSaidaGratuitaMinutos { get; set; }

    public int ToleranciaDemaisHorasMinutos { get; set; }

    public Dictionary<string, ParkingRate> Tarifas { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
