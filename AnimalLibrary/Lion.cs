namespace AnimalLibrary
{
    [Comment("Лев")]
    public class Lion : Animal
    {
        public Lion() { }
        public Lion(string name, string country, bool hide)
            : base(name, country, hide, "Лев",
                eClassificationAnimal.Carnivores) { }
        public override eFavoriteFood GetFavoriteFood()
        {
            return eFavoriteFood.Meat;
        }
        public override string SayHello()
        {
            return "Р-р-р!";
        }
    }
}
