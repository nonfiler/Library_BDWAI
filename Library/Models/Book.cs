namespace Library.Models
{
    public class Book
    {
        public int ID { get; set; }
        public string? Title { get; set; }
        public int AuthorID { get; set; }
        public Author? Author { get; set; }
        public string DisplayAuthor => $"{Author?.Name} {Author?.Surname}";
        public string? Genre { get; set; }
        public string? Description { get; set; }
        public int Rating { get; set; }
        public List<Comment> ?Comments { get; set; }
    }
}