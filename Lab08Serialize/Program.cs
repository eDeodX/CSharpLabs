using AnimalLibrary;
using System.Xml.Serialization;
namespace Lab08Serialize
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Animal animal = new Cow("Бурёнка", "Россия", false);
            string path = args.Length > 0 ? args[0] : "animal.xml";
            XmlSerializer serializer = new XmlSerializer(typeof(Animal));
            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                serializer.Serialize(stream, animal);
            }
            Console.WriteLine("До сериализации: " + animal);
            Console.WriteLine("Объект сохранён: " + Path.GetFullPath(path));
        }
    }
}
