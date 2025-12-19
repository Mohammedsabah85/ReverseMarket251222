using ReverseMarket.Models;

namespace ReverseMarket.Services
{
    public interface IDefaultImageService
    {
        /// <summary>
        /// إضافة صورة افتراضية للطلب إذا لم يتم رفع أي صورة
        /// </summary>
        /// <param name="requestId">معرف الطلب</param>
        /// <returns></returns>
        Task AddDefaultImageIfNeededAsync(int requestId);

        /// <summary>
        /// الحصول على مسار الصورة الافتراضية للطلبات
        /// </summary>
        /// <returns>مسار الصورة الافتراضية</returns>
        string GetDefaultRequestImagePath();

        /// <summary>
        /// التحقق من وجود صور للطلب
        /// </summary>
        /// <param name="requestId">معرف الطلب</param>
        /// <returns>true إذا كان للطلب صور، false إذا لم يكن له صور</returns>
        Task<bool> HasImagesAsync(int requestId);
    }
}