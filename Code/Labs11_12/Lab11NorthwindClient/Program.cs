using System.Net.Http.Json;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
using var client = new HttpClient { BaseAddress = new Uri("http://127.0.0.1:5112/") };
const string url = "api/categories";
while (true)
{
    Console.WriteLine("1 — список, 2 — по ID, 3 — добавить, 4 — изменить, 5 — удалить, 0 — выход");
    string? choice = Console.ReadLine();
    if (choice is null or "0") break;
    int id = 0;
    if (choice is "2" or "4" or "5")
    {
        Console.Write("ID: ");
        if (!int.TryParse(Console.ReadLine(), out id) || id < 1)
        {
            Console.WriteLine("Нужен положительный ID.");
            continue;
        }
    }

    try
    {
        string path = url + "/" + id;
        using HttpResponseMessage? response = choice switch
        {
            "1" => await client.GetAsync(url),
            "2" => await client.GetAsync(path),
            "3" => await client.PostAsJsonAsync(url, ReadInput()),
            "4" => await client.PutAsJsonAsync(path, ReadInput()),
            "5" => await client.DeleteAsync(path),
            _ => null
        };
        if (response == null)
        {
            Console.WriteLine("Неизвестный пункт.");
            continue;
        }
        Console.WriteLine("HTTP " + (int)response.StatusCode);
        string body = await response.Content.ReadAsStringAsync();
        if (body.Length != 0) Console.WriteLine(body);
    }
    catch (HttpRequestException e)
    {
        Console.WriteLine("Не удалось обратиться к API: " + e.Message);
    }
    catch (TaskCanceledException)
    {
        Console.WriteLine("Сервер не ответил вовремя.");
    }
}

static object ReadInput()
{
    Console.Write("Название (до 15 символов): ");
    string? name = Console.ReadLine();
    Console.Write("Описание: ");
    string? description = Console.ReadLine();
    return new { categoryName = name?.Trim(), description };
}
