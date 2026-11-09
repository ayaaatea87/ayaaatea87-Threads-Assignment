public sealed class FridayNightBooking
{
    public int TablesLeft { get; private set; } = 5;
    public int Booked { get; private set; }
    private readonly object _sync = new();

    public void Book(string customerName)
    {
        bool lockTaken = false;

        try 
        {
            Monitor.Enter(_sync, ref lockTaken);
            if (TablesLeft <= 0)
            {
                Console.WriteLine($"{customerName}: No tables left.");
                return;
            }
            Thread.Sleep(20);
            TablesLeft--;
            Booked++;
        }
        finally
        {
            if (lockTaken)
            {
                Monitor.Exit(_sync);
            }
        }
        Console.WriteLine($"{customerName} booked a table. Left={TablesLeft}");
    }
}
