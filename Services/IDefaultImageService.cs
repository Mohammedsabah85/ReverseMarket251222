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

        /// <summary>
        /// الحصول على الصورة الافتراضية للطلبات من إعدادات الموقع
        /// </summary>
        /// <returns>مسار الصورة الافتراضية للطلبات</returns>
        Task<string> GetDefaultRequestImageAsync();

        /// <summary>
        /// الحصول على الصورة الافتراضية للمتاجر من إعدادات الموقع
        /// </summary>
        /// <returns>مسار الصورة الافتراضية للمتاجر</returns>
        Task<string> GetDefaultStoreImageAsync();

        /// <summary>
        /// الحصول على الصورة الافتراضية للمستخدمين من إعدادات الموقع
        /// </summary>
        /// <returns>مسار الصورة الافتراضية للمستخدمين</returns>
        Task<string> GetDefaultUserAvatarAsync();

        /// <summary>
        /// الحصول على صورة الطلب أو الصورة الافتراضية
        /// </summary>
        /// <param name="requestImage">صورة الطلب</param>
        /// <returns>صورة الطلب أو الصورة الافتراضية</returns>
        string GetRequestImageOrDefault(string? requestImage);

        /// <summary>
        /// الحصول على صورة المتجر أو الصورة الافتراضية
        /// </summary>
        /// <param name="storeImage">صورة المتجر</param>
        /// <returns>صورة المتجر أو الصورة الافتراضية</returns>
        string GetStoreImageOrDefault(string? storeImage);

        /// <summary>
        /// الحصول على صورة المستخدم أو الصورة الافتراضية
        /// </summary>
        /// <param name="userAvatar">صورة المستخدم</param>
        /// <returns>صورة المستخدم أو الصورة الافتراضية</returns>
        string GetUserAvatarOrDefault(string? userAvatar);
    }
}