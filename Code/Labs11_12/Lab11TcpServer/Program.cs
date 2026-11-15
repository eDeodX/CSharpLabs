using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

Console.OutputEncoding = Encoding.UTF8;
bool demo = args.Length == 1 && args[0] == "--demo";
if (!demo && args.Length != 1)
{
    Console.WriteLine("Укажи путь к базе ЛР10 или аргумент --demo.");
    return;
}

string path = Path.GetFullPath(demo ? "lab11-tcp-demo.db" : args[0]);
var connection = new SqliteConnectionStringBuilder { DataSource = path };
if (demo)
{
    await using var db = new StocksContext(connection.ToString());
    await db.Database.EnsureCreatedAsync();
    if (!await db.Stocks.AnyAsync())
    {
        var stock = new Stock { Ticker = "TEST_UP" };
        db.Stocks.Add(stock);
        await db.SaveChangesAsync();
        db.StockPrices.AddRange(
            new StockPrice { StockId = stock.Id, Date = new DateTime(2026, 1, 1), Close = 10 },
            new StockPrice { StockId = stock.Id, Date = new DateTime(2026, 1, 2), Close = 12 });
        await db.SaveChangesAsync();
    }
}
connection.Mode = SqliteOpenMode.ReadOnly;
string readOnly = connection.ToString();

// Проверяем схему до запуска TCP-сервера.
try
{
    await using var db = new StocksContext(readOnly);
    await db.Stocks.Select(s => s.Ticker).Take(1).ToListAsync();
    await db.StockPrices.Select(p => p.Close).Take(1).ToListAsync();
}
catch (SqliteException e)
{
    Console.WriteLine("Не удалось открыть базу ЛР10: " + e.Message);
    return;
}

var listener = new TcpListener(IPAddress.Loopback, 5111);
listener.Start();
Console.WriteLine(demo ? "Учебная база: TEST_UP." : "База ЛР10: " + path);
Console.WriteLine("TCP-сервер: 127.0.0.1:5111");
while (true)
{
    using TcpClient client = await listener.AcceptTcpClientAsync();
    try
    {
        using var stream = client.GetStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        using var writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true };
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        string? request = await reader.ReadLineAsync(timeout.Token);
        if (request is null) continue;
        string ticker = request.Trim().ToUpperInvariant();
        if (ticker.Length == 0)
        {
            await writer.WriteLineAsync("ERROR: пустой тикер");
            continue;
        }

        await using var db = new StocksContext(readOnly);
        StockPrice? last = await db.StockPrices
            .Where(p => db.Stocks.Any(s => s.Id == p.StockId && s.Ticker == ticker))
            .OrderByDescending(p => p.Date)
            .ThenByDescending(p => p.Id)
            .FirstOrDefaultAsync();
        string response = last is null ? "NOT_FOUND"
            : $"OK {last.Close.ToString(CultureInfo.InvariantCulture)} {last.Date:yyyy-MM-dd}";
        await writer.WriteLineAsync(response);
        Console.WriteLine(ticker + " -> " + response);
    }
    catch (Exception e) when (e is IOException or OperationCanceledException or SqliteException)
    {
        Console.WriteLine("Запрос не обработан: " + e.Message);
    }
}
