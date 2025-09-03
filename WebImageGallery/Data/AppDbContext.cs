
using Microsoft.EntityFrameworkCore;
using WebImageGallery.Controllers;

namespace WebImageGallery.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<UploadedImage> UploadedImage {  get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UploadedImage>()
                .HasKey(x => x.Id);


            base.OnModelCreating(modelBuilder);
        }
    }
}
