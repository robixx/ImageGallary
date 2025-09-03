using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;
using System.Drawing.Imaging;
using WebImageGallery.Data;

namespace WebImageGallery.Controllers
{
    public class ImageController : Controller
    {
        private readonly string? _imagePath;
        private readonly AppDbContext _dbcontext;

        public ImageController(IConfiguration configuration, AppDbContext dbContext)
        {
            _imagePath = configuration["ImageStorage:TokenImagePath"];
            _dbcontext = dbContext;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveImages(List<IFormFile> files)
        {

            if (files == null || files.Count == 0)
                return BadRequest("No files uploaded");

            var savedImages = new List<UploadedImage>();

            // Use first available system font
            var fontFamily = SystemFonts.Families.First();

            foreach (var file in files)
            {
                if (file.Length == 0) continue;

                var safeName = Path.GetFileName(file.FileName);
                var fileName = $"{DateTime.Now:yyyyMMddHHmmssfff}_{safeName}";
                var filePath = Path.Combine(_imagePath, fileName);

                using (var image = Image.Load<Rgba32>(file.OpenReadStream()))
                {
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    // Set proportional font size (1/10th of image width)
                    float fontSize = image.Width / 17f;
                    var font = new Font(fontFamily, fontSize, FontStyle.Bold);

                    // Measure text size to position top-right
                    var textSize = TextMeasurer.MeasureSize(timestamp, new RendererOptions(font));
                    var position = new PointF(image.Width - textSize.Width - 10, 10);

                    // Draw timestamp
                    image.Mutate(ctx =>
                    {
                        // Optional: add shadow for better visibility
                        ctx.DrawText(timestamp, font, Color.Black, new PointF(position.X + 2, position.Y + 2));
                        ctx.DrawText(timestamp, font, Color.Yellow, position);
                    });

                    // Save image to disk
                    await image.SaveAsync(filePath);
                }

                // Save metadata to database
                var imageRecord = new UploadedImage
                {
                    FileName = fileName,
                    FilePath = filePath,
                    UploadedAt = DateTime.Now
                };

                _dbcontext.UploadedImage.Add(imageRecord);
                savedImages.Add(imageRecord);
            }

            await _dbcontext.SaveChangesAsync();

            return Ok(new { message = "Images uploaded successfully", images = savedImages });

        }
    }

    internal class RendererOptions : TextOptions
    {
        public RendererOptions(Font font) : base(font)
        {
        }
    }

    public class UploadedImage
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
    }

}
