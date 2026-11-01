namespace AnimalLibrary
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    [Comment("Пользовательский атрибут с комментарием")]
    public class CommentAttribute : Attribute
    {
        public string Comment { get; set; }
        public CommentAttribute(string comment)
        {
            Comment = comment;
        }
    }
}
