using System.ComponentModel.DataAnnotations;

namespace RadencyTask2.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }
        public string Message { get; set; }
        public Book Book { get; set; }
        public string Reviewer { get; set; }
    }
}
