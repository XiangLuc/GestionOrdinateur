using GestionOrdinateurs.Shared;

namespace GestionOrdinateurs.API;

public interface IStockService
{
    int StockSSD { get; }
    void ConsommerSSD();
}