using LapTrinhWeb2_API.Models.Domain;
using LapTrinhWeb2_API.Models.DTO;

namespace LapTrinhWeb2_API.Repositories
{
    public interface IBookRepository
    {
        List<BookWithAuthorAndPublisherDTO> GetAllBooks();
        BookWithAuthorAndPublisherDTO GetBookById(int id);
        addBookRequestDTO AddBook(addBookRequestDTO addBookRequestDTO);
        addBookRequestDTO? UpdateBookById(int id, addBookRequestDTO bookDTO);
        Books? DeleteBookById(int id);
    }
}
