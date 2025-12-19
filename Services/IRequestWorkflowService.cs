using ReverseMarket.Models;

namespace ReverseMarket.Services
{
    public interface IRequestWorkflowService
    {
        /// <summary>
        /// إرسال إشعار للإدارة عند إضافة طلب جديد
        /// </summary>
        /// <param name="request">الطلب الجديد</param>
        /// <returns></returns>
        Task NotifyAdminAboutNewRequestAsync(Request request);

        /// <summary>
        /// إرسال إشعار للمستخدم والمتاجر عند الموافقة على الطلب
        /// </summary>
        /// <param name="request">الطلب المعتمد</param>
        /// <returns></returns>
        Task NotifyRequestApprovalAsync(Request request);

        /// <summary>
        /// إرسال إشعار للمستخدم عند رفض الطلب مع السبب
        /// </summary>
        /// <param name="request">الطلب المرفوض</param>
        /// <param name="rejectionReason">سبب الرفض</param>
        /// <returns></returns>
        Task NotifyRequestRejectionAsync(Request request, string rejectionReason);

        /// <summary>
        /// إرسال إشعار للإدارة عند تعديل الطلب
        /// </summary>
        /// <param name="request">الطلب المعدل</param>
        /// <returns></returns>
        Task NotifyAdminAboutRequestModificationAsync(Request request);

        /// <summary>
        /// إرسال إشعار للمستخدم عند الموافقة على تعديل الطلب
        /// </summary>
        /// <param name="request">الطلب المعدل</param>
        /// <returns></returns>
        Task NotifyRequestModificationApprovalAsync(Request request);

        /// <summary>
        /// إرسال إشعار للمستخدم عند رفض تعديل الطلب
        /// </summary>
        /// <param name="request">الطلب المعدل</param>
        /// <param name="rejectionReason">سبب رفض التعديل</param>
        /// <returns></returns>
        Task NotifyRequestModificationRejectionAsync(Request request, string rejectionReason);

        /// <summary>
        /// إرسال إشعار للإدارة والمستخدم عند حذف الطلب
        /// </summary>
        /// <param name="request">الطلب المحذوف</param>
        /// <returns></returns>
        Task NotifyRequestDeletionAsync(Request request);

        /// <summary>
        /// إرسال إشعارات للمتاجر المتخصصة عند الموافقة على الطلب
        /// </summary>
        /// <param name="request">الطلب المعتمد</param>
        /// <returns></returns>
        Task NotifyRelevantStoresAsync(Request request);
    }
}