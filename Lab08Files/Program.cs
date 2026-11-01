using System.IO.Compression;
namespace Lab08Files
{
    internal class Program
    {
        static void Find(string folder, string fileName, List<string> result)
        {
            try
            {
                foreach (string file in Directory.GetFiles(folder))
                {
                    if (string.Equals(Path.GetFileName(file), fileName,
                        StringComparison.OrdinalIgnoreCase)) result.Add(file);
                }
                foreach (string dir in Directory.GetDirectories(folder))
                {
                    if ((File.GetAttributes(dir) & FileAttributes.ReparsePoint) != 0)
                        continue;
                    Find(dir, fileName, result);
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Нет доступа: " + folder);
            }
            catch (IOException e)
            {
                Console.WriteLine("Ошибка чтения папки: " + e.Message);
            }
        }
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Write("Начальная папка: ");
            string folder = Console.ReadLine();
            Console.Write("Имя файла с расширением: ");
            string name = Console.ReadLine();
            if (!Directory.Exists(folder) || string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Неверная папка или имя.");
                return;
            }
            List<string> files = new List<string>();
            Find(folder, name, files);
            Console.WriteLine("Найдено файлов: " + files.Count);
            foreach (string file in files)
            {
                try
                {
                    Console.WriteLine("Файл: " + file);
                    using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read))
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        Console.WriteLine(reader.ReadToEnd());
                    }
                    Console.Write("Сжать этот файл в GZip? (да/нет): ");
                    if (Console.ReadLine() != "да") continue;
                    string archive = file + ".gz";
                    if (File.Exists(archive))
                    {
                        Console.WriteLine("Архив уже существует: " + archive);
                        continue;
                    }
                    using (FileStream input = new FileStream(file, FileMode.Open, FileAccess.Read))
                    using (FileStream output = new FileStream(archive, FileMode.CreateNew))
                    using (GZipStream zip = new GZipStream(output, CompressionMode.Compress))
                    {
                        input.CopyTo(zip);
                    }
                    Console.WriteLine("Создан архив: " + archive);
                }
                catch (IOException e)
                {
                    Console.WriteLine("Ошибка файла: " + e.Message);
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine("Нет доступа к файлу.");
                }
            }
        }
    }
}
