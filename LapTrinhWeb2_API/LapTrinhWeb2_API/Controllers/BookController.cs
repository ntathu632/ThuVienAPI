using LapTrinhWeb2_API.Data;
using LapTrinhWeb2_API.Models.Domain;
using LapTrinhWeb2_API.Models.DTO;
using LapTrinhWeb2_API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using WebAPI_simple.CustomActionFilter;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LapTrinhWeb2_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IBookRepository _bookRepository;
        public BookController(AppDbContext dbContext, IBookRepository bookRepository)
        {
            _dbContext = dbContext;
            _bookRepository = bookRepository;
        }


        //GET: http://localhost:5000/api/get-all-books
        [HttpGet("Get-all-books")]
        public IActionResult GetAll([FromQuery] string? filterOn, [FromQuery] string? filterQuery)
        {
            // su dung reposity pattern 
            var allBooks = _bookRepository.GetAllBooks(filterOn, filterQuery);
            return Ok(allBooks);
        }

        [HttpGet]
        [Route("get-book-by-id/{id:int}")]
        public IActionResult GetBookById([FromRoute] int id)
        {
            var bookWithIdDTO = _bookRepository.GetBookById(id);
            return Ok(bookWithIdDTO);
        }

        [HttpPost("add-book")]
        [ValidateModel]
        //[Authorize(Roles = "Write")]
        public IActionResult AddBook([FromBody] addBookRequestDTO addBookRequestDTO)
        {
            if (!_bookRepository.PublisherExists(addBookRequestDTO.PublisherId))
            {
                return BadRequest("PublisherId không tồn tại");
            }
            if (ValidateAddBook(addBookRequestDTO))
            {
                var bookAdd = _bookRepository.AddBook(addBookRequestDTO);
                return Ok(bookAdd);
            }
            else return BadRequest(ModelState);
        }

        

        [HttpPut("update-book-by-id/{id}")]
        public IActionResult UpdateBookById(int id, [FromBody] addBookRequestDTO bookDTO)
        {
            var updateBook = _bookRepository.UpdateBookById(id, bookDTO);
            return Ok(updateBook);
        }

        [HttpDelete("delete-book-by-id/{id}")]
        public IActionResult DeleteBookById(int id)
        {
            var deleteBook = _bookRepository.DeleteBookById(id);
            return Ok(deleteBook);
        }

        private bool ValidateAddBook(addBookRequestDTO addBookRequestDTO)
        {
            {
                ModelState.AddModelError(nameof(addBookRequestDTO), $"Please add book data"); 
                return false;
            }
            // kiem tra Description NotNull
            if (string.IsNullOrEmpty(addBookRequestDTO.Description))
            {
                ModelState.AddModelError(nameof(addBookRequestDTO.Description), $"{nameof(addBookRequestDTO.Description)} cannot be null");
            }
            // kiem tra rating (0,5) 
            if (addBookRequestDTO.Rate < 0 || addBookRequestDTO.Rate > 5)
            {
                ModelState.AddModelError(nameof(addBookRequestDTO.Rate), $"{nameof(addBookRequestDTO.Rate)} cannot be less than 0 and more than 5");
            }
            if (ModelState.ErrorCount > 0)
            {
                return false;
            }
            return true;
        }
    }
}
