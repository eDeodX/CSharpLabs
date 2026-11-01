namespace AnimalLibrary
{
    [Comment("Корова")]
    public class Cow : Animal
    {
        public Cow() { }
        public Cow(string name, string country, bool hide)
            : base(name, country, hide, "Корова",
                eClassificationAnimal.Herbivores) { }
        public override eFavoriteFood GetFavoriteFood()
        {
            return eFavoriteFood.Plants;
        }
        public override string SayHello()
        {
            return "Му!";
        }
    }
}
