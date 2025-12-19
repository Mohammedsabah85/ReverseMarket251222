using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReverseMarket.Data;
using ReverseMarket.Models;
using ReverseMarket.Models.Identity;

namespace ReverseMarket.Services
{
    public class StoreWorkflowService : IStoreWorkflowService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<StoreWorkflowService> _logger;

        public StoreWorkflowService(
            ApplicationDbContext context,
            INotificationService notificationService,
            UserManager<ApplicationUser> userManager,
            ILogger<StoreWorkflowService> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task NotifyAdminAboutNewStoreAsync(ApplicationUser store)
        {
            try
            {
                var title = "🏪 متجر جديد بانتظار المراجعة";
                var message = $"تم تسجيل متجر جديد بانتظار الموافقة:\n\n" +
                             $"اسم المتجر: {store.StoreName}\n" +
                             $"صاحب المتجر: {store.FullName}\n" +
                             $"رقم الهاتف: {store.PhoneNumber}\n" +
                             $"البريد الإلكتروني: {store.Email}";

                var notification = await _notificationService.CreateNotificationAsync(
                    title: title,
                    message: message,
                    type: NotificationType.NewStoreForAdmin,
                    userId: null, // للإدارة
                    link: $"/Admin/Stores/Details/{store.Id}",
                    isFromAdmin: false
                );

                await _notificationService.SendNotificationAsync(notification);

                _logger.LogInformation("تم إرسال إشعار للإدارة عن المتجر الجديد {StoreId}", store.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إرسال إشعار المتجر الجديد للإدارة {StoreId}", store.Id);
            }
        }

        public async Task NotifyStoreApprovalAsync(ApplicationUser store)
        {
            try
            {
                var title = "✅ تم اعتماد متجرك!";
                var message = $"مبروك! تم اعتماد متجرك '{store.StoreName}' بنجاح!\n\n" +
                             $"يمكنك الآن:\n" +
                             $"• استقبال الطلبات من العملاء\n" +
                             $"• تقديم عروض على الطلبات المتخصصة\n" +
                             $"• إدارة متجرك بالكامل\n\n" +
                             $"نتمنى لك التوفيق في رحلتك التجارية معنا!";

                var notification = await _notificationService.CreateNotificationAsync(
                    title: title,
                    message: message,
                    type: NotificationType.StoreApproved,
                    userId: store.Id,
                    link: "/Store/Dashboard",
                    isFromAdmin: true
                );

                await _notificationService.SendNotificationAsync(notification);

                _logger.LogInformation("تم إرسال إشعار اعتماد المتجر {StoreId}", store.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إرسال إشعار اعتماد المتجر {StoreId}", store.Id);
            }
        }

        public async Task NotifyStoreRejectionAsync(ApplicationUser store, string rejectionReason)
        {
            try
            {
                var title = "❌ تم رفض متجرك";
                var message = $"نأسف لإبلاغك أنه تم رفض طلب إنشاء متجرك '{store.StoreName}'\n\n" +
                             $"سبب الرفض: {rejectionReason}\n\n" +
                             $"يمكنك تعديل بيانات متجرك وإعادة تقديم الطلب مرة أخرى.\n" +
                             $"تأكد من استيفاء جميع الشروط المطلوبة.";

                var notification = await _notificationService.CreateNotificationAsync(
                    title: title,
                    message: message,
                    type: NotificationType.StoreRejected,
                    userId: store.Id,
                    link: "/Store/Profile",
                    isFromAdmin: true
                );

                await _notificationService.SendNotificationAsync(notification);

                _logger.LogInformation("تم إرسال إشعار رفض المتجر {StoreId}", store.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إرسال إشعار رفض المتجر {StoreId}", store.Id);
            }
        }

        public async Task NotifyAdminAboutStoreModificationAsync(ApplicationUser store)
        {
            try
            {
                var title = "📝 تعديل متجر بانتظار المراجعة";
                var message = $"تم تعديل بيانات المتجر '{store.StoreName}' من قبل {store.FullName}\n\n" +
                             $"يرجى مراجعة التعديلات والموافقة عليها أو رفضها.";

                var notification = await _notificationService.CreateNotificationAsync(
                    title: title,
                    message: message,
                    type: NotificationType.StoreModified,
                    userId: null, // للإدارة
                    link: $"/Admin/Stores/Details/{store.Id}",
                    isFromAdmin: false
                );

                await _notificationService.SendNotificationAsync(notification);

                _logger.LogInformation("تم إرسال إشعار تعديل المتجر {StoreId} للإدارة", store.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إرسال إشعار تعديل المتجر للإدارة {StoreId}", store.Id);
            }
        }

        public async Task NotifyStoreModificationApprovalAsync(ApplicationUser store)
        {
            try
            {
                var title = "✅ تم اعتماد تعديل متجرك!";
                var message = $"تم اعتماد التعديلات على متجرك '{store.StoreName}' بنجاح!\n\n" +
                             $"التعديلات الآن مفعلة ومرئية للعملاء.";

                var notification = await _notificationService.CreateNotificationAsync(
                    title: title,
                    message: message,
                    type: NotificationType.StoreModificationApproved,
                    userId: store.Id,
                    link: "/Store/Profile",
                    isFromAdmin: true
                );

                await _notificationService.SendNotificationAsync(notification);

                _logger.LogInformation("تم إرسال إشعار اعتماد تعديل المتجر {StoreId}", store.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إرسال إشعار اعتماد تعديل المتجر {StoreId}", store.Id);
            }
        }

        public async Task NotifyStoreModificationRejectionAsync(ApplicationUser store, string rejectionReason)
        {
            try
            {
                var title = "❌ تم رفض تعديل متجرك";
                var message = $"نأسف لإبلاغك أنه تم رفض التعديلات على متجرك '{store.StoreName}'\n\n" +
                             $"سبب الرفض: {rejectionReason}\n\n" +
                             $"يمكنك إجراء تعديلات أخرى وإعادة المحاولة.";

                var notification = await _notificationService.CreateNotificationAsync(
                    title: title,
                    message: message,
                    type: NotificationType.StoreModificationRejected,
                    userId: store.Id,
                    link: "/Store/Profile",
                    isFromAdmin: true
                );

                await _notificationService.SendNotificationAsync(notification);

                _logger.LogInformation("تم إرسال إشعار رفض تعديل المتجر {StoreId}", store.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إرسال إشعار رفض تعديل المتجر {StoreId}", store.Id);
            }
        }

        public async Task NotifyStoreDeletionAsync(ApplicationUser store)
        {
            try
            {
                // 1. إشعار للإدارة
                var adminTitle = "🗑️ تم حذف متجر";
                var adminMessage = $"تم حذف المتجر '{store.StoreName}' من قبل {store.FullName}";

                var adminNotification = await _notificationService.CreateNotificationAsync(
                    title: adminTitle,
                    message: adminMessage,
                    type: NotificationType.StoreDeleted,
                    userId: null, // للإدارة
                    isFromAdmin: false
                );

                await _notificationService.SendNotificationAsync(adminNotification);

                // 2. إشعار تأكيد لصاحب المتجر
                var userTitle = "🗑️ تم حذف متجرك";
                var userMessage = $"تم حذف متجرك '{store.StoreName}' بنجاح.\n\n" +
                                 $"إذا كان هذا خطأ، يمكنك التواصل مع الدعم الفني.";

                var userNotification = await _notificationService.CreateNotificationAsync(
                    title: userTitle,
                    message: userMessage,
                    type: NotificationType.StoreDeleted,
                    userId: store.Id
                );

                await _notificationService.SendNotificationAsync(userNotification);

                _logger.LogInformation("تم إرسال إشعارات حذف المتجر {StoreId}", store.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إرسال إشعارات حذف المتجر {StoreId}", store.Id);
            }
        }
    }
}