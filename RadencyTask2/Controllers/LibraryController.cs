using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RadencyTask2.DTO;
using RadencyTask2.Models;
using System.Configuration;

namespace RadencyTask2.Controllers
{
    [Route("Api/")]
    [ApiController]
    public class LibraryController : ControllerBase
    {
        private readonly ILogger<LibraryController> _logger;
        private readonly LibraryDbContext _db;
        public LibraryController(LibraryDbContext db, ILogger<LibraryController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpGet]
        [Route("Books")]
        public async Task<List<BookDTO>> Books(string order)
        {
            List<Book> books = new List<Book>();
            if(order == "title")
            {
                books = await _db.Books
                    .OrderBy(b => b.Title)
                    .Include(b => b.Ratings)
                    .Include(b => b.Reviews)
                    .ToListAsync();
            } else if(order == "author")
            {
                books = await _db.Books
                    .OrderBy(b => b.Author)
                    .Include(b => b.Ratings)
                    .Include(b => b.Reviews)
                    .ToListAsync();
            }

            List<BookDTO> bookDTOs = new List<BookDTO>();
            for(int i = 0; i < books.Count; i++)
            {
                decimal avg = 0;
                if (books[i].Ratings.Count > 0) avg = books[i].Ratings.Average(r => r.Score);
                
                bookDTOs.Add(new BookDTO
                {
                    Id = books[i].Id,
                    Title = books[i].Title,
                    Author = books[i].Author,
                    Rating = avg,
                    ReviewsNumber = books[i].Reviews.Count
                });                
            }

            Log(this.Request);

            return bookDTOs;
        }

        [HttpGet]
        [Route("Seed")]
        public void Seed()
        {
            _db.SeedData();
            Log(this.Request);
        }

        [HttpGet]
        [Route("Recommended")]
        public async Task<List<BookDTO>> Recommended(string? genre)
        {
            List<BookDTO> bookDTOs = new List<BookDTO>();

            List<Book> books = await _db.Books
                .Include(b => b.Ratings)
                .Include(b => b.Reviews)
                .Where(b => b.Reviews.Count > 10)
                .Where(b => b.Genre == genre)
                .OrderByDescending(b => b.Ratings.Average(r => r.Score))
                .ToListAsync();

            for (int i = 0; i < books.Count; i++)
            {
                decimal avg = 0;
                if (books[i].Ratings.Count > 0) avg = books[i].Ratings.Average(r => r.Score);
                bookDTOs.Add(new BookDTO
                {
                    Id = books[i].Id,
                    Title = books[i].Title,
                    Author = books[i].Author,
                    Rating = avg,
                    ReviewsNumber = books[i].Reviews.Count
                });
            }

            Log(this.Request);

            return bookDTOs;
        }

        [HttpGet]
        [Route("Books/{id}")]
        public BookDetailsDTO Books(int id)
        {
            Book book = _db.Books
                .Where(b => b.Id == id)
                .Include(b => b.Reviews)
                .Include(b => b.Ratings)
                .FirstOrDefault();
            
            List<ReviewDTO> reviewDTOs = new List<ReviewDTO>();
            for(int i = 0; i < book.Reviews.Count; i++)
            {
                reviewDTOs.Add(new ReviewDTO
                {
                    Id = book.Reviews[i].Id,
                    Message = book.Reviews[i].Message,
                    Reviewer = book.Reviews[i].Reviewer
                });
            }
            BookDetailsDTO bookDetailsDTO = new BookDetailsDTO
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                Cover = book.Cover,
                Content = book.Content,
                Rating = book.Ratings.Average(r => r.Score),
                Reviews = reviewDTOs
            };

            Log(this.Request);

            return bookDetailsDTO;
        }

        [HttpDelete]
        [Route("Books/{id}/secret")]
        public void Delete(int id, string secret)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json").Build();
            string secretKey = config.GetSection("Secret").Value;

            if (secret == secretKey)
            {
                Book book = _db.Books.Find(id);
                if(book is not null)
                {
                    _db.Books.Remove(book);
                    _db.SaveChanges();
                }                
            }

            Log(this.Request);
        }

        [HttpPost]
        [Route("Books/Save")]
        public int AddBook([FromBody] BookDetailsDTO bookDetailsDTO)
        {
            Book book = new Book
            {
                Id = bookDetailsDTO.Id,
                Title = bookDetailsDTO.Title,
                Content = bookDetailsDTO.Content,
                Cover = bookDetailsDTO.Cover,
                Author = bookDetailsDTO.Author,
                Genre = bookDetailsDTO.Genre
            };
            if(book.Id != 0 && _db.Books.Find(book.Id) is not null) _db.Books.Update(book);
            else _db.Books.Add(book);

            _db.SaveChanges();

            Log(this.Request);

            return _db.Books.OrderBy(b => b.Id).Last().Id;
        }

        [HttpPut]
        [Route("Books/{id}/Review")]
        public int AddReview(int id, ReviewDTO reviewDTO)
        {
            Book book = _db.Books.Find(id);
            Review review = new Review
            {
                Book = book,
                Reviewer = reviewDTO.Reviewer,
                Message = reviewDTO.Message
            };

            _db.Reviews.Add(review);
            _db.SaveChanges();

            Log(this.Request);

            return _db.Reviews.OrderBy(r => r.Id).Last().Id;
        }

        [HttpPut]
        [Route("Books/{id}/Rate")]
        public int AddRate(int id, decimal score)
        {
            Book book = _db.Books.Find(id);
            Rating rating = new Rating
            {
                Score = score,
                Book = book
            };

            _db.Ratings.Add(rating);
            _db.SaveChanges();

            Log(this.Request);

            return _db.Ratings.OrderBy(r => r.Id).Last().Id;
        }

        private async void Log(HttpRequest request)
        {
            string headers = "\n";
            foreach(var key in request.Headers.Keys)
            {
                headers += $"{key}: {request.Headers[key]}\n";
            }

            /*request.EnableBuffering();
            request.Body.Position = 0;
            var rawRequestBody = await new StreamReader(request.Body).ReadToEndAsync();*/

            _logger.Log(LogLevel.Information, $"Request type: {request.HttpContext.Request.Method.ToString()}");
            _logger.Log(LogLevel.Information, $"Request headers: {headers}");
            //_logger.Log(LogLevel.Information, $"Request body: {request.Body}");
            _logger.Log(LogLevel.Information, $"Request params: {request.QueryString.Value}");
            _logger.Log(LogLevel.Information, $"Request path: {request.Path}");
        }
    }
}
