using LapTrinhWeb2_API.Models.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LapTrinhWeb2_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        public BookController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        //GET: http://localhost:5000/api/get-all-books
        [HttpGet("Get-all-books")]
        public IActionResult GetAll()
        {
            var allBookDomain = _dbContext.Books;
            //Map domain to DTO
            var allBookDTO = allBookDomain.Select(Books => new BookWithAuthorAndPublisherDTO()
            {
                Id = Books.Id,
                Title = Books.Title,
                Description = Books.Description,
                IsRead = Books.IsRead,
                DateRead = Books.IsRead ? Books.DateRead.Value : null,
                Rate = Books.IsRead ? Books.Rate.Value : null,
                Genre = Books.Genre,
                CoverUrl = Books.CoverUrl,
                DateAdded = Books.DateAdded,
                PublisherName = Books.Publisher.Name,
                AuthorNames = Books.Book_Authors.Select(n => n.Author.FullName).ToList()
            }).ToList();
            //return DTO
            return Ok(allBookDomain);
        }

        [HttpGet]
        [Route("get-book-by-id/{id:int}")]
        public IActionResult GetBookById([FromRoute] int id)
        {
            //get Book domain object from DB
            var bookDomain = _dbContext.Books.Include(b => b.Publisher).Include(b => b.Book_Authors).ThenInclude(ba => ba.Author).FirstOrDefault(b => b.Id == id); ;
            if (bookDomain == null)
            {
                return NotFound();
            }

            //map bookDomain object to DTO
            var bookDTO = new Models.DTO.BookWithAuthorAndPublisherDTO()
            {
                Id = bookDomain.Id,
                Title = bookDomain.Title,
                Description = bookDomain.Description,
                IsRead = bookDomain.IsRead,
                DateRead = bookDomain.DateRead,
                Rate = bookDomain.Rate,
                Genre = bookDomain.Genre,
                CoverUrl = bookDomain.CoverUrl,
                DateAdded = bookDomain.DateAdded,
                PublisherName = bookDomain.Publisher != null ? bookDomain.Publisher.Name : "Unknown",
                AuthorNames = bookDomain.Book_Authors?.Where(y => y.Author != null).Select(y => y.Author.FullName).ToList() ?? new List<string>()
            };
            return Ok(bookDTO);
        }

        [HttpPost("add-book")]
        public IActionResult AddBook([FromBody] addBookRequestDTO addBookRequestDTO)
        {
            //kiem tra publisher có ton tai hay k
            var publisherDomain = _dbContext.Publishers.FirstOrDefault(x => x.Id == addBookRequestDTO.PublisherId);
            if (publisherDomain == null)
            {
                return NotFound(new { message = "Không tìm thấy NXB" });
            }
            //create new book domain object
            var bookDomain = new Models.Domain.Books()
            {
                Title = addBookRequestDTO.Title,
                Description = addBookRequestDTO.Description,
                IsRead = addBookRequestDTO.IsRead,
                DateRead = addBookRequestDTO.DateRead,
                Rate = addBookRequestDTO.Rate,
                Genre = addBookRequestDTO.Genre,
                CoverUrl = addBookRequestDTO.CoverUrl,
                DateAdded = addBookRequestDTO.DateAdded,
                PublisherId = publisherDomain.Id
            };
            _dbContext.Books.Add(bookDomain);
            _dbContext.SaveChanges();
            //check author exists or not then add to Book_Author table
            foreach (var authorId in addBookRequestDTO.AuthorIds)
            {
                var authorDomain = _dbContext.Authors.FirstOrDefault(x => x.Id == authorId);
                if (authorDomain == null)
                {
                    return NotFound(new { message = $"Không tìm thấy tác giả" });
                }
                var bookAuthorDomain = new Models.Domain.Book_Author()
                {
                    BookId = bookDomain.Id,
                    AuthorId = authorDomain.Id
                };
                _dbContext.Book_Authors.Add(bookAuthorDomain);
                _dbContext.SaveChanges();
            }
            return Ok();
        }

        [HttpPut("update-book-by-id/{id:int}")]
        public IActionResult UpdateBookById(int id, [FromBody] addBookRequestDTO addBookRequestDTO)
        {
            var bookDomain = _dbContext.Books.FirstOrDefault(x => x.Id == id);
            if (bookDomain != null)
            {
                bookDomain.Title = addBookRequestDTO.Title;
                bookDomain.Description = addBookRequestDTO.Description;
                bookDomain.IsRead = addBookRequestDTO.IsRead;
                bookDomain.DateRead = addBookRequestDTO.DateRead;
                bookDomain.Rate = addBookRequestDTO.Rate;
                bookDomain.Genre = addBookRequestDTO.Genre;
                bookDomain.CoverUrl = addBookRequestDTO.CoverUrl;
                bookDomain.DateAdded = addBookRequestDTO.DateAdded;
                bookDomain.PublisherId = addBookRequestDTO.PublisherId;
                _dbContext.SaveChanges();
            }
            var existingBookAuthors = _dbContext.Book_Authors.Where(x => x.BookId == id).ToList();
            if (existingBookAuthors != null & existingBookAuthors.Count > 0)
            {
                _dbContext.Book_Authors.RemoveRange(existingBookAuthors);
                _dbContext.SaveChanges();
            }
            foreach (var authorId in addBookRequestDTO.AuthorIds)
            {
                var authorDomain = _dbContext.Authors.FirstOrDefault(x => x.Id == authorId);
                if (authorDomain == null)
                {
                    return NotFound();
                }
                var bookAuthorDomain = new Models.Domain.Book_Author()
                {
                    BookId = bookDomain.Id,
                    AuthorId = authorDomain.Id
                };
                _dbContext.Book_Authors.Add(bookAuthorDomain);
                _dbContext.SaveChanges();
            }
            return Ok(addBookRequestDTO);
        }
    }
}
