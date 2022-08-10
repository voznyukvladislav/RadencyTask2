using Bogus;
using Microsoft.EntityFrameworkCore;

namespace RadencyTask2.Models
{
    public class LibraryDbContext : DbContext
    {
        public DbSet<Book> Books { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Review> Reviews { get; set; }

        public LibraryDbContext(DbContextOptions options) : base(options)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=LibraryDb;Trusted_Connection=True;");
        }

        public void SeedData()
        {
            Faker faker = new Faker();
            Random random = new Random();

            List<string> covers = new List<string> 
            { 
                "Hardcover",
                "Softcover",
                "Hardcover with ImageWrap",
                "Hardcover with Dust Jacket" 
            };
            List<string> genres = new List<string>
            {
                "Adventure",
                "Horror",
                "Sci-fi",
                "Fantasy",
                "Classic",
                "Novel",
                "Detective",
                "Historical fiction",
                "Romance",
                "Memoir"
            };

            List<Book> books = new List<Book>();
            for (int i = 0; i < 50; i++)
            {
                books.Add(new Book
                {
                    //Id = i + 1,
                    Author = faker.Name.FullName(),
                    Content = faker.Lorem.Text(),
                    Cover = covers[random.Next(0, covers.Count)],
                    Genre = genres[random.Next(0, genres.Count)],
                    Title = ArrayToString(faker.Lorem.Words(random.Next(1, 5))),
                    Ratings = new List<Rating>(),
                    Reviews = new List<Review>()
                });

                for (int j = 0; j < random.Next(5, 50); j++)
                {
                    books[i].Ratings.Add(new Rating
                    {
                        //Id = ratingId++,
                        Score = random.Next(1, 101),
                        Book = books[i]
                    });
                }

                for (int j = 0; j < random.Next(5, 30); j++)
                {
                    books[i].Reviews.Add(new Review
                    {
                        //Id = reviewId++,
                        Reviewer = faker.Name.FullName(),
                        Message = faker.Lorem.Text(),
                        Book = books[i]
                    });
                }
                this.Ratings.AddRange(books[i].Ratings);
                this.Reviews.AddRange(books[i].Reviews);
                this.Books.Add(books[i]);
                this.SaveChanges();
            }
        }

        private string ArrayToString(string[] arr)
        {
            string str = "";
            for(int i = 0; i < arr.Length; i++)
            {
                str += $"{arr[i]}";
                if (i != arr.Length - 1) str += " ";
            }

            return str;
        }
    }
}
