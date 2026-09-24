using LapTrinhWeb2_API.Data;
using LapTrinhWeb2_API.Models.Domain;
using LapTrinhWeb2_API.Models.DTO;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace LapTrinhWeb2_API.Repositories
{
    public class SQLPublisherRepository : IPublisherRepository
    {
        private readonly AppDbContext _dbContext;
        public SQLPublisherRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public List<PublisherDTO> GetAllPublishers()
        {
            //Get Data From Database -Domain Model
            var allPublishersDomain = _dbContext.Publishers.ToList();
            //Map domain models to DTOs
            var allPublisherDTO = new List<PublisherDTO>();
            foreach (var publisherDomain in allPublishersDomain)
            {
                allPublisherDTO.Add(new PublisherDTO()
                {
                    Id = publisherDomain.Id,
                    Name = publisherDomain.Name
                });
            }
            return allPublisherDTO;
        }
        public PublisherNoIdDTO GetPublisherById(int id)
        {
            // get book Domain model from Db
            var publisherWithIdDomain = _dbContext.Publishers.FirstOrDefault(x => x.Id ==id);
            if (publisherWithIdDomain != null)
            { //Map Domain Model to DTOs
                var publisherNoIdDTO = new PublisherNoIdDTO
                {
                    Name = publisherWithIdDomain.Name,
                };
                return publisherNoIdDTO;
            }
            return null;
        }
        public AddPublisherRequestDTO AddPublisher(AddPublisherRequestDTO addPublisherRequestDTO)
        {
            var publisherDomainModel = new Publishers
            {
                Name = addPublisherRequestDTO.Name,
            };
            //Use Domain Model to create Book
            _dbContext.Publishers.Add(publisherDomainModel);
            _dbContext.SaveChanges();
            return addPublisherRequestDTO;
        }
        public PublisherNoIdDTO UpdatePublisherById(int id, PublisherNoIdDTO publisherNoIdDTO)
        {
            var publisherDomain = _dbContext.Publishers.FirstOrDefault(n => n.Id == id);
            if (publisherDomain != null)
            {
                publisherDomain.Name = publisherNoIdDTO.Name;
                _dbContext.SaveChanges();
            }
            return null;
        }
        public Publishers? DeletePublisherById(int id)
        {
            var publisherDomain = _dbContext.Publishers.FirstOrDefault(n => n.Id == id);
            if (publisherDomain != null)
            {
                _dbContext.Publishers.Remove(publisherDomain);
                _dbContext.SaveChanges();
            }
            return null;
        }

        public List<BookWithAuthorAndPublisherDTO> GetBooksByPublisherId(int id)
        {
            var booksOfPublisher = _dbContext.Books.Where(b=> b.PublisherId == id).Select(book => new BookWithAuthorAndPublisherDTO()
            {
                Id = book.Id,
                Title = book.Title,
                Description = book.Description,
                IsRead = book.IsRead,
                DateRead = book.DateRead,
                Rate = book.Rate,
                Genre = book.Genre,
                CoverUrl = book.CoverUrl,
                DateAdded = book.DateAdded,
                PublisherName = book.Publisher.Name,
                AuthorNames = book.Book_Authors.Select(n => n.Author.FullName).ToList()
            }).ToList();
            return booksOfPublisher;

        }
    }
}
