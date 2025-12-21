using System.ComponentModel.DataAnnotations;

namespace ReverseMarket.Models.ViewModels
{
    public class DefaultImagesViewModel
    {
        [Display(Name = "الصورة الافتراضية للطلبات")]
        public string? DefaultRequestImage { get; set; }

        [Display(Name = "الصورة الافتراضية للمتاجر")]
        public string? DefaultStoreImage { get; set; }

        [Display(Name = "الصورة الافتراضية للمستخدمين")]
        public string? DefaultUserAvatar { get; set; }

        [Display(Name = "رفع صورة افتراضية للطلبات")]
        public IFormFile? RequestImageFile { get; set; }

        [Display(Name = "رفع صورة افتراضية للمتاجر")]
        public IFormFile? StoreImageFile { get; set; }

        [Display(Name = "رفع صورة افتراضية للمستخدمين")]
        public IFormFile? UserAvatarFile { get; set; }
    }
}