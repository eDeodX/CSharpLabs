namespace AnimalLibrary
{
    [Comment("Свинья")]
    public class Pig : Animal
    {
        public Pig() { }
        public Pig(string name, string country, bool hide)
            : base(name, country, hide, "Свинья",
                eClassificationAnimal.Omnivores) { }
        public override eFavoriteFood GetFavoriteFood()
        {
            return eFavoriteFood.Everything;
        }
        public override string SayHello()
        {
            return "Хрю!";
        }
    }
}
