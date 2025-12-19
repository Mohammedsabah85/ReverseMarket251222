using ReverseMarket.Models.Identity;

namespace ReverseMarket.Services
{
    public interface IStoreWorkflowService
    {
        /// <summary>
        /// إرسال إشعار للإدارة عند إنشاء متجر جديد
        /// </summary>
        /// <param name="store">المتجر الجديد</param>
        /// <returns></returns>
        Task NotifyAdminAboutNewStoreAsync(ApplicationUser store);

        /// <summary>
        /// إرسال إشعار لصاحب المتجر عند الموافقة على المتجر
        /// </summary>
        /// <param name="store">المتجر المعتمد</param>
        /// <returns></returns>
        Task NotifyStoreApprovalAsync(ApplicationUser store);

        /// <summary>
        /// إرسال إشعار لصاحب المتجر عند رفض المتجر مع السبب
        /// </summary>
        /// <param name="store">المتجر المرفوض</param>
        /// <param name="rejectionReason">سبب الرفض</param>
        /// <returns></returns>
        Task NotifyStoreRejectionAsync(ApplicationUser store, string rejectionReason);

        /// <summary>
        /// إرسال إشعار للإدارة عند تعديل بيانات المتجر
        /// </summary>
        /// <param name="store">المتجر المعدل</param>
        /// <returns></returns>
        Task NotifyAdminAboutStoreModificationAsync(ApplicationUser store);

        /// <summary>
        /// إرسال إشعار لصاحب المتجر عند الموافقة على تعديل المتجر
        /// </summary>
        /// <param name="store">المتجر المعدل</param>
        /// <returns></returns>
        Task NotifyStoreModificationApprovalAsync(ApplicationUser store);

        /// <summary>
        /// إرسال إشعار لصاحب المتجر عند رفض تعديل المتجر
        /// </summary>
        /// <param name="store">المتجر المعدل</param>
        /// <param name="rejectionReason">سبب رفض التعديل</param>
        /// <returns></returns>
        Task NotifyStoreModificationRejectionAsync(ApplicationUser store, string rejectionReason);

        /// <summary>
        /// إرسال إشعار للإدارة عند حذف المتجر
        /// </summary>
        /// <param name="store">المتجر المحذوف</param>
        /// <returns></returns>
        Task NotifyStoreDeletionAsync(ApplicationUser store);
    }
}