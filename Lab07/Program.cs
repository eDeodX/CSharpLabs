using AnimalLibrary;
using System.Reflection;
using System.Xml.Linq;
namespace Lab07
{
    internal class Program
    {
        static XElement Parameters(ParameterInfo[] parameters)
        {
            XElement result = new XElement("Parameters");
            foreach (ParameterInfo p in parameters)
            {
                result.Add(new XElement("Parameter",
                    new XAttribute("name", p.Name),
                    new XAttribute("type", p.ParameterType.FullName)));
            }
            return result;
        }
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Assembly assembly = typeof(Animal).Assembly;
            XElement root = new XElement("Assembly",
                new XAttribute("name", assembly.GetName().Name));
            Type[] types = assembly.GetExportedTypes();
            Array.Sort(types, (a, b) => string.CompareOrdinal(a.Name, b.Name));
            foreach (Type type in types)
            {
                XElement node = new XElement("Type",
                    new XAttribute("name", type.FullName),
                    new XAttribute("kind", type.IsEnum ? "enum" : "class"),
                    new XAttribute("abstract", type.IsAbstract));
                if (type.BaseType != null)
                    node.Add(new XAttribute("base", type.BaseType.FullName));
                foreach (CustomAttributeData attribute in type.GetCustomAttributesData())
                {
                    XElement attr = new XElement("Attribute",
                        new XAttribute("type", attribute.AttributeType.FullName));
                    foreach (CustomAttributeTypedArgument arg in attribute.ConstructorArguments)
                        attr.Add(new XElement("Argument", Convert.ToString(arg.Value)));
                    node.Add(attr);
                }
                CommentAttribute comment = type.GetCustomAttribute<CommentAttribute>();
                if (comment != null)
                    node.Add(new XElement("Comment", comment.Comment));
                if (type.IsEnum)
                {
                    foreach (string value in Enum.GetNames(type))
                        node.Add(new XElement("Value", value));
                }
                BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic
                    | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
                foreach (ConstructorInfo c in type.GetConstructors(flags))
                    node.Add(new XElement("Constructor",
                        new XAttribute("access", c.IsPublic ? "public" : "protected/private"),
                        Parameters(c.GetParameters())));
                foreach (PropertyInfo p in type.GetProperties(flags))
                    node.Add(new XElement("Property", new XAttribute("name", p.Name),
                        new XAttribute("type", p.PropertyType.FullName),
                        new XAttribute("read", p.CanRead), new XAttribute("write", p.CanWrite)));
                foreach (FieldInfo f in type.GetFields(flags))
                {
                    if (f.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute)))
                        continue;
                    node.Add(new XElement("Field", new XAttribute("name", f.Name),
                        new XAttribute("type", f.FieldType.FullName)));
                }
                foreach (MethodInfo m in type.GetMethods(flags))
                {
                    if (m.IsSpecialName) continue;
                    node.Add(new XElement("Method", new XAttribute("name", m.Name),
                        new XAttribute("return", m.ReturnType.FullName),
                        new XAttribute("abstract", m.IsAbstract),
                        new XAttribute("virtual", m.IsVirtual), Parameters(m.GetParameters())));
                }
                root.Add(node);
                Console.WriteLine(type.Name + (comment == null ? "" : ": " + comment.Comment));
            }
            string path = args.Length > 0 ? args[0] : "diagram.xml";
            new XDocument(root).Save(path);
            Console.WriteLine("XML сохранён: " + Path.GetFullPath(path));
        }
    }
}
