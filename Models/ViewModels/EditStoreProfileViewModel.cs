using System.ComponentModel.DataAnnotations;

namespace ReverseMarket.Models.ViewModels
{
    /// <summary>
    /// ViewModel شامل لتعديل معلومات المتجر
    /// يستخدم للبائعين فقط
    /// </summary>
    public class EditStoreProfileViewModel
    {
        // ═══════════════════════════════════════════════════════════════════
        // معلومات المتجر الأساسية
        // ═══════════════════════════════════════════════════════════════════

        [Required(ErrorMessage = "اسم المتجر مطلوب")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "اسم المتجر يجب أن يكون بين 3 و 100 حرف")]
        [Display(Name = "اسم المتجر")]
        public string StoreName { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "وصف المتجر يجب ألا يتجاوز 1000 حرف")]
        [Display(Name = "وصف المتجر")]
        public string? StoreDescription { get; set; }

        // ═══════════════════════════════════════════════════════════════════
        // معلومات الموقع
        // ═══════════════════════════════════════════════════════════════════

        [Required(ErrorMessage = "المحافظة مطلوبة")]
        [Display(Name = "المحافظة")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "المنطقة مطلوبة")]
        [Display(Name = "المنطقة")]
        public string District { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "العنوان يجب ألا يتجاوز 500 حرف")]
        [Display(Name = "العنوان التفصيلي")]
        public string? Location { get; set; }

        // ═══════════════════════════════════════════════════════════════════
        // صورة المتجر
        // ═══════════════════════════════════════════════════════════════════

        [Display(Name = "صورة المتجر الحالية")]
        public string? CurrentProfileImage { get; set; }

        [Display(Name = "صورة جديدة للمتجر")]
        public IFormFile? NewProfileImage { get; set; }

        [Display(Name = "حذف الصورة الحالية")]
        public bool RemoveCurrentImage { get; set; }

        // ═══════════════════════════════════════════════════════════════════
        // روابط المتجر (تحتاج موافقة الإدارة)
        // ═══════════════════════════════════════════════════════════════════

        [Url(ErrorMessage = "يرجى إدخال رابط صحيح (مثال: https://example.com)")]
        [StringLength(500, ErrorMessage = "الرابط يجب ألا يتجاوز 500 حرف")]
        [Display(Name = "رابط الموقع الإلكتروني 1")]
        public string? WebsiteUrl1 { get; set; }

        [Url(ErrorMessage = "يرجى إدخال رابط صحيح (مثال: https://example.com)")]
        [StringLength(500, ErrorMessage = "الرابط يجب ألا يتجاوز 500 حرف")]
        [Display(Name = "رابط الموقع الإلكتروني 2")]
        public string? WebsiteUrl2 { get; set; }

        [Url(ErrorMessage = "يرجى إدخال رابط صحيح (مثال: https://example.com)")]
        [StringLength(500, ErrorMessage = "الرابط يجب ألا يتجاوز 500 حرف")]
        [Display(Name = "رابط الموقع الإلكتروني 3")]
        public string? WebsiteUrl3 { get; set; }

        // الروابط المعلقة (في انتظار الموافقة)
        public string? PendingWebsiteUrl1 { get; set; }
        public string? PendingWebsiteUrl2 { get; set; }
        public string? PendingWebsiteUrl3 { get; set; }

        // حالة الموافقة على الروابط
        public string? Url1Status { get; set; }
        public string? Url2Status { get; set; }
        public string? Url3Status { get; set; }

        public bool HasPendingUrlChanges { get; set; }

        // ═══════════════════════════════════════════════════════════════════
        // تخصصات المتجر (فئات)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// فئات المتجر المختارة (JSON string of SubCategory2 IDs)
        /// </summary>
        [Display(Name = "تخصصات المتجر")]
        public string? StoreCategories { get; set; }

        /// <summary>
        /// الفئات الحالية للعرض
        /// </summary>
        public List<StoreCategoryDisplayItem>? CurrentStoreCategories { get; set; }

        // ═══════════════════════════════════════════════════════════════════
        // معلومات إضافية للعرض فقط
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// هل المتجر معتمد؟
        /// </summary>
        public bool IsStoreApproved { get; set; }

        /// <summary>
        /// تاريخ إنشاء الحساب
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// رقم الهاتف (للعرض فقط)
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// البريد الإلكتروني (للعرض فقط)
        /// </summary>
        public string? Email { get; set; }
    }

    /// <summary>
    /// نموذج لعرض فئة المتجر
    /// </summary>
    public class StoreCategoryDisplayItem
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public int? SubCategory1Id { get; set; }
        public int? SubCategory2Id { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? SubCategory1Name { get; set; }
        public string? SubCategory2Name { get; set; }

        public string FullPath
        {
            get
            {
                var path = CategoryName;
                if (!string.IsNullOrEmpty(SubCategory1Name))
                    path += $" > {SubCategory1Name}";
                if (!string.IsNullOrEmpty(SubCategory2Name))
                    path += $" > {SubCategory2Name}";
                return path;
            }
        }
    }
}
