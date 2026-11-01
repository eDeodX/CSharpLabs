using System.Xml.Serialization;
namespace AnimalLibrary
{
    public enum eClassificationAnimal { Herbivores, Carnivores, Omnivores }
    public enum eFavoriteFood { Meat, Plants, Everything }

    [Comment("Общий абстрактный класс животного")]
    [XmlInclude(typeof(Cow))]
    [XmlInclude(typeof(Lion))]
    [XmlInclude(typeof(Pig))]
    public abstract class Animal
    {
        public string Country { get; set; }
        public bool HideFromOtherAnimals { get; set; }
        public string Name { get; set; }
        public string WhatAnimal { get; set; }
        public eClassificationAnimal Classification { get; set; }

        protected Animal() { }
        protected Animal(string name, string country, bool hide,
            string whatAnimal, eClassificationAnimal classification)
        {
            Name = name;
            Country = country;
            HideFromOtherAnimals = hide;
            WhatAnimal = whatAnimal;
            Classification = classification;
        }
        public void Deconstruct(out string name, out string country)
        {
            name = Name;
            country = Country;
        }
        public eClassificationAnimal GetClassificationAnimal()
        {
            return Classification;
        }
        public abstract eFavoriteFood GetFavoriteFood();
        public abstract string SayHello();
        public override string ToString()
        {
            return Name + ", " + WhatAnimal + ", " + Country
                + ", прячется: " + HideFromOtherAnimals
                + ", " + Classification + ", еда: " + GetFavoriteFood()
                + ", голос: " + SayHello();
        }
    }
}
