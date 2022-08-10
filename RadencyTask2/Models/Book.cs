using System.ComponentModel.DataAnnotations;

namespace RadencyTask2.Models
{
    public class Book
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Cover { get; set; }
        public string Content { get; set; }
        public string Author { get; set; }
        public string Genre { get; set; }
        public List<Review> Reviews { get; set; } = new List<Review>();
        public List<Rating> Ratings { get; set; } = new List<Rating>();
    }
}
