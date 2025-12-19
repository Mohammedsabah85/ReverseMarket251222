using Microsoft.EntityFrameworkCore;
using ReverseMarket.Data;
using ReverseMarket.Models;

namespace ReverseMarket.Services
{
    public class DefaultImageService : IDefaultImageService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DefaultImageService> _logger;
        private const string DEFAULT_REQUEST_IMAGE_PATH = "/images/default-request.svg";

        public DefaultImageService(
            ApplicationDbContext context,
            ILogger<DefaultImageService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddDefaultImageIfNeededAsync(int requestId)
        {
            try
            {
                // التحقق من وجود صور للطلب
                var hasImages = await HasImagesAsync(requestId);
                
                if (!hasImages)
                {
                    // إضافة الصورة الافتراضية
                    var defaultImage = new RequestImage
                    {
                        RequestId = requestId,
                        ImagePath = DEFAULT_REQUEST_IMAGE_PATH,
                        CreatedAt = DateTime.Now
                    };

                    _context.RequestImages.Add(defaultImage);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("تم إضافة صورة افتراضية للطلب {RequestId}", requestId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إضافة الصورة الافتراضية للطلب {RequestId}", requestId);
            }
        }

        public string GetDefaultRequestImagePath()
        {
            return DEFAULT_REQUEST_IMAGE_PATH;
        }

        public async Task<bool> HasImagesAsync(int requestId)
        {
            try
            {
                return await _context.RequestImages
                    .AnyAsync(ri => ri.RequestId == requestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في التحقق من وجود صور للطلب {RequestId}", requestId);
                return false;
            }
        }
    }
}