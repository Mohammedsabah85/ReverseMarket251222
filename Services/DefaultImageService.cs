using Microsoft.EntityFrameworkCore;
using ReverseMarket.Data;
using ReverseMarket.Models;

namespace ReverseMarket.Services
{
    public class DefaultImageService : IDefaultImageService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DefaultImageService> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private const string DEFAULT_REQUEST_IMAGE_PATH = "/images/default-request.svg";

        // Default fallback images
        private const string FallbackRequestImage = "/images/default-request.png";
        private const string FallbackStoreImage = "/images/default-store.png";
        private const string FallbackUserAvatar = "/images/default-avatar.png";

        public DefaultImageService(
            ApplicationDbContext context,
            ILogger<DefaultImageService> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
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

        public async Task<string> GetDefaultRequestImageAsync()
        {
            try
            {
                var settings = await _context.SiteSettings.FirstOrDefaultAsync();
                return !string.IsNullOrEmpty(settings?.DefaultRequestImage)
                    ? settings.DefaultRequestImage
                    : FallbackRequestImage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على الصورة الافتراضية للطلبات");
                return FallbackRequestImage;
            }
        }

        public async Task<string> GetDefaultStoreImageAsync()
        {
            try
            {
                var settings = await _context.SiteSettings.FirstOrDefaultAsync();
                return !string.IsNullOrEmpty(settings?.DefaultStoreImage)
                    ? settings.DefaultStoreImage
                    : FallbackStoreImage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على الصورة الافتراضية للمتاجر");
                return FallbackStoreImage;
            }
        }

        public async Task<string> GetDefaultUserAvatarAsync()
        {
            try
            {
                var settings = await _context.SiteSettings.FirstOrDefaultAsync();
                return !string.IsNullOrEmpty(settings?.DefaultUserAvatar)
                    ? settings.DefaultUserAvatar
                    : FallbackUserAvatar;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على الصورة الافتراضية للمستخدمين");
                return FallbackUserAvatar;
            }
        }

        public string GetRequestImageOrDefault(string? requestImage)
        {
            try
            {
                if (!string.IsNullOrEmpty(requestImage))
                {
                    // Check if file exists
                    var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, requestImage.TrimStart('/'));
                    if (File.Exists(fullPath))
                    {
                        return requestImage;
                    }
                }

                // Return default from database or fallback
                var settings = _context.SiteSettings.FirstOrDefault();
                return !string.IsNullOrEmpty(settings?.DefaultRequestImage)
                    ? settings.DefaultRequestImage
                    : FallbackRequestImage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على صورة الطلب أو الصورة الافتراضية");
                return FallbackRequestImage;
            }
        }

        public string GetStoreImageOrDefault(string? storeImage)
        {
            try
            {
                if (!string.IsNullOrEmpty(storeImage))
                {
                    // Check if file exists
                    var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, storeImage.TrimStart('/'));
                    if (File.Exists(fullPath))
                    {
                        return storeImage;
                    }
                }

                // Return default from database or fallback
                var settings = _context.SiteSettings.FirstOrDefault();
                return !string.IsNullOrEmpty(settings?.DefaultStoreImage)
                    ? settings.DefaultStoreImage
                    : FallbackStoreImage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على صورة المتجر أو الصورة الافتراضية");
                return FallbackStoreImage;
            }
        }

        public string GetUserAvatarOrDefault(string? userAvatar)
        {
            try
            {
                if (!string.IsNullOrEmpty(userAvatar))
                {
                    // Check if file exists
                    var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, userAvatar.TrimStart('/'));
                    if (File.Exists(fullPath))
                    {
                        return userAvatar;
                    }
                }

                // Return default from database or fallback
                var settings = _context.SiteSettings.FirstOrDefault();
                return !string.IsNullOrEmpty(settings?.DefaultUserAvatar)
                    ? settings.DefaultUserAvatar
                    : FallbackUserAvatar;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على صورة المستخدم أو الصورة الافتراضية");
                return FallbackUserAvatar;
            }
        }
    }
}