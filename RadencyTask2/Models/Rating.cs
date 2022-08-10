using System.ComponentModel.DataAnnotations;

namespace RadencyTask2.Models
{
    public class Rating
    {
        [Key]
        public int Id { get; set; }
        public Book Book { get; set; }
        public decimal Score { get; set; }
    }
}
