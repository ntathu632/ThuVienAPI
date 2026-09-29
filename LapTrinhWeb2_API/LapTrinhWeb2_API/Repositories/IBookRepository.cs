using LapTrinhWeb2_API.Models.Domain;
using LapTrinhWeb2_API.Models.DTO;

namespace LapTrinhWeb2_API.Repositories
{
    public interface IBookRepository
    {
        List<BookWithAuthorAndPublisherDTO> GetAllBooks(string? filterOn = null, string? filterQuery = null, string? sortBy = null, bool isAscending = true, int pageNumber = 1, int pageSize = 1000);
        BookWithAuthorAndPublisherDTO GetBookById(int id);
        addBookRequestDTO AddBook(addBookRequestDTO addBookRequestDTO);
        addBookRequestDTO? UpdateBookById(int id, addBookRequestDTO bookDTO);
        Books? DeleteBookById(int id);
        bool PublisherExists(int publisherId);
    }
}
