using System.Net.Sockets;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
while (true)
{
    Console.Write("Тикер (Enter для выхода): ");
    string? ticker = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(ticker)) break;
    try
    {
        using var client = new TcpClient();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        await client.ConnectAsync("127.0.0.1", 5111, timeout.Token);
        using var stream = client.GetStream();
        using var writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true };
        using var reader = new StreamReader(stream, Encoding.UTF8);
        await writer.WriteLineAsync(ticker.Trim());
        string? answer = await reader.ReadLineAsync(timeout.Token);
        if (answer == "NOT_FOUND") Console.WriteLine("Тикер или цена не найдены.");
        else if (answer?.StartsWith("OK ") == true)
        {
            string[] parts = answer.Split(' ');
            Console.WriteLine($"Последняя цена: {parts[1]}, дата: {parts[2]}");
        }
        else Console.WriteLine(answer ?? "Сервер не прислал ответ.");
    }
    catch (Exception e) when (e is SocketException or IOException or OperationCanceledException)
    {
        Console.WriteLine("Ошибка связи: " + e.Message);
    }
}
