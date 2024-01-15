using Microsoft.AspNetCore.Identity;

namespace Library.Models
{
    public class Comment
    {
        public int ID { get; set; }
        public int BookID { get; set; }
        public string Text { get; set; }
        public int Score { get; set; }

        // Poprawione właściwości
        public string UserId { get; set; }  // Upewnij się, że to jest typu string
        public IdentityUser User { get; set; }
    }
}
