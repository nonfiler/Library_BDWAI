namespace Library.Models
{
    public class Author
    {
        public int ID { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? BirthYear { get; set; }
        public string? Description { get; set; }

        public string NameSurname => $"{Name} {Surname}";
    }
}
