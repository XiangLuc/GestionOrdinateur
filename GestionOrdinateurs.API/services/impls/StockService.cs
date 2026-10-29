namespace GestionOrdinateurs.API;

public class StockService : IStockService
{
    
    public int StockSSD { get; private set; } = 10;
    private readonly object _stockLock = new object();

    public void ConsommerSSD()
    {
        Task.Delay(10).Wait();

        lock (_stockLock)
        {
            StockSSD--;
        }
    }
}