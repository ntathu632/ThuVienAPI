using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using LapTrinhWeb2_API.Models.Domain;

namespace LapTrinhWeb2_API
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> dbContextOptions) : base(dbContextOptions)
        {
            //Constructor
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //co the dinh nghia qh giua cac table = fluent API
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Book_Author>()
                .HasKey(ba => new { ba.BookId, ba.AuthorId });
            modelBuilder.Entity<Book_Author>()
                .HasOne(b => b.Book)
                .WithMany(ba => ba.Book_Authors)
                .HasForeignKey(bi => bi.BookId);
            modelBuilder.Entity<Book_Author>()
                .HasOne(b => b.Author)
                .WithMany(ba => ba.Book_Authors)
                .HasForeignKey(bi => bi.AuthorId);
        }
        public DbSet<Books> Books { get; set; }
        public DbSet<Authors> Authors { get; set; }
        public DbSet<Book_Author> Book_Authors { get; set; }
        public DbSet<Publishers> Publishers { get; set; }
    }
}
