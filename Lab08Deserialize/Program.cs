using AnimalLibrary;
using System.Xml.Serialization;
namespace Lab08Deserialize
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            string path = args.Length > 0 ? args[0] : "animal.xml";
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Animal));
                Animal animal;
                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    animal = (Animal)serializer.Deserialize(stream);
                }
                Console.WriteLine("Тип после загрузки: " + animal.GetType().Name);
                Console.WriteLine("После десериализации: " + animal);
            }
            catch (Exception e)
            {
                Console.WriteLine("Не удалось прочитать XML: " + e.Message);
            }
        }
    }
}
