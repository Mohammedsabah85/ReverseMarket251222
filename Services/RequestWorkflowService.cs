using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReverseMarket.Data;
using ReverseMarket.Models;
using ReverseMarket.Models.Identity;

namespace ReverseMarket.Services
{
    public class RequestWorkflowService : IRequestWorkflowService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RequestWorkflowService> _logger;

        public RequestWorkflowService(
            ApplicationDbContext context,
            INotificationService notificationService,
            UserManager<ApplicationUser> userManager,
            ILogger<RequestWorkflowService> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task NotifyAdminAboutNewRequestAsync(Request request)
        {
            try
            {
                // جلب بيانات الطلب كاملة
                var fullRequest = await GetFullRequestAsync(request.Id);
                if (fullRequest == null) return;

                var title = "📋 طلب جديد بانتظار المراجعة";
                var message = $"تم إرسال طلب جديد من المستخدم {fullRequest.User?.FullName}\n\n" +
                             $"العنوان: {fullRequest.Title}\n" +
                             $"الفئة: {GetCategoryPath(fullRequest)}\n" +
                             $"الموقع: {fullRequest.City} - {fullRequest.District}";

                // إرسال إشعار للإدارة فقط
                var notification = await _notificationService.CreateNotificationAsync(
                    title: title,
                    message: message,
                    type: NotificationType.NewRequestForAdmin,
                    userId: null, // للإدارة
                    targetUserType: null, // سيتم تحديد الإدارة في الكنترولر
                    requestId: fullRequest.Id,
                    link: $"/Admin/Requests/Details/{fullRequest.Id}",
                    isFromAdmin: false
                );

                await _notificationService.SendNotificationAsync(notification);

                _logger.LogInformation("تم إرسال إشعار للإدارة عن الطلب الجديد #{RequestId}", request.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إرسال إشعار الطلب الجديد للإدارة #{RequestId}", request.Id);
            }
        }

        public async Task NotifyRequestApprovalAsync(Request request)
        {
            try
            {
                var fullRequest = await GetFullRequestAsync(request.Id);
                if (fullRequest == null) return;

                // 1. إشعار صاحب الطلب
                var userTitle = "✅ تم اعتماد طلبك!";
                var userMessage = $"تم اعتماد طلبك '{fullRequest.Title}' بنجاح!\n\n" +
                                 $"يمكن للمتاجر المتخصصة الآن مشاهدة طلبك وتقديم عروضهم.\n" +
                                 $"ستصلك إشعارات عند تلقي عروض جديدة.";

                var userNotification = await _notificationService.CreateNotificationAsync(
                    title: userTitle,
                    message: userMessage,
                    type: NotificationType.RequestApproved,
                    userId: fullRequest.UserId,
                    requestId: fullRequest.Id,
                    link: $"/Requests/Details/{fullRequest.Id}"
                );

                await _notificationService.SendNotificationAsync(userNotification);

                // 2. إشعار المتاجر المتخصصة
                await NotifyRelevantStoresAsync(fullRequest);

                _logger.LogInformation("تم إرسال إشعارات الموافقة على الطلب #{RequestId}", request.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إرسال إشعارات الموافقة على الطلب #{RequestId}", request.Id);
            }
        }

        public async Task NotifyRequestRejectionAsync(Request request, string rejectionReason)
        {
            try
            {
                var fullRequest = await GetFullRequestAsync(request.Id);
                if (fullRequest == null) return;

                var title = "❌ تم رفض طلبك";
                var message = $"نأسف لإبلاغك أنه تم رفض طلبك '{fullRequest.Title}'\n\n" +
                             $"سبب الرفض: {rejectionReason}\n\n" +
                             $"يمكنك تعديل طلبك وإعادة إرساله مرة أخرى.";

                // إرسال إشعار لصاحب الطلب فقط
                var notification = await _notificationService.CreateNotificationAsync(
                    title: title,
                    message: message,
                    type: NotificationType.RequestRejected,
                    userId: fullRequest.UserId,
                    requestId: fullRequest.Id,
                    link: $"/Requests/Details/{fullRequest.Id}",
                    isFromAdmin: true
                );

                await _notificationService.SendNotificationAsync(notification);

                _logger.LogInformation("تم إرسال إشعار رفض الطلب #{RequestId} للمستخدم", request.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إرسال إشعار رفض الطلب #{RequestId}", request.Id);
            }
        }

        public async Task NotifyAdminAboutRequestModificationAsync(Request request)
        {
            try
            {
                var fullRequest = await GetFullRequestAsync(request.Id);
                if (fullRequest == null) return;

                var title = "📝 طلب تعديل بانتظار المراجعة";
                var message = $"تم تعديل الطلب '{fullRequest.Title}' من قبل {fullRequest.User?.FullName}\n\n" +
                             $"يرجى مراجعة التعديلات والموافقة عليها أو رفضها.";

                var notification = await _notificationService.CreateNotificationAsync(
                    title: title,
                    message: message,
                    type: NotificationType.RequestModified,
                    userId: null, // للإدارة
                    requestId: fullRequest.Id,
                    link: $"/Admin/Requests/Details/{fullRequest.Id}",
                    isFromAdmin: false
                );

                await _notificationService.SendNotificationAsync(notification);

                _logger.LogInformation("تم إرسال إشعار تعديل الطلب #{RequestId} للإدارة", request.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إرسال إشعار تعديل الطلب للإدارة #{RequestId}", request.Id);
            }
        }

        public async Task NotifyRequestModificationApprovalAsync(Request request)
        {
            try
            {
                var fullRequest = await GetFullRequestAsync(request.Id);
                if (fullRequest == null) return;

                var title = "✅ تم اعتماد تعديل طلبك!";
                var message = $"تم اعتماد التعديلات على طلبك '{fullRequest.Title}' بنجاح!\n\n" +
                             $"طلبك الآن متاح للمتاجر المتخصصة.";

                var notification = await _notificationService.CreateNotificationAsync(
                    title: title,
                    message: message,
                    type: NotificationType.RequestModificationApproved,
                    userId: fullRequest.UserId,
                    requestId: fullRequest.Id,
                    link: $"/Requests/Details/{fullRequest.Id}",
                    isFromAdmin: true
                );

                await _notificationService.SendNotificationAsync(notification);

                _logger.LogInformation("تم إرسال إشعار اعتماد تعديل الطلب #{RequestId}", request.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إرسال إشعار اعتماد تعديل الطلب #{RequestId}", request.Id);
            }
        }

        public async Task NotifyRequestModificationRejectionAsync(Request request, string rejectionReason)
        {
            try
            {
                var fullRequest = await GetFullRequestAsync(request.Id);
                if (fullRequest == null) return;

                var title = "❌ تم رفض تعديل طلبك";
                var message = $"نأسف لإبلاغك أنه تم رفض التعديلات على طلبك '{fullRequest.Title}'\n\n" +
                             $"سبب الرفض: {rejectionReason}\n\n" +
                             $"يمكنك إجراء تعديلات أخرى وإعادة المحاولة.";

                var notification = await _notificationService.CreateNotificationAsync(
                    title: title,
                    message: message,
                    type: NotificationType.RequestModificationRejected,
                    userId: fullRequest.UserId,
                    requestId: fullRequest.Id,
                    link: $"/Requests/Details/{fullRequest.Id}",
                    isFromAdmin: true
                );

                await _notificationService.SendNotificationAsync(notification);

                _logger.LogInformation("تم إرسال إشعار رفض تعديل الطلب #{RequestId}", request.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إرسال إشعار رفض تعديل الطلب #{RequestId}", request.Id);
            }
        }

        public async Task NotifyRequestDeletionAsync(Request request)
        {
            try
            {
                var fullRequest = await GetFullRequestAsync(request.Id);
                if (fullRequest == null) return;

                // 1. إشعار للإدارة
                var adminTitle = "🗑️ تم حذف طلب";
                var adminMessage = $"تم حذف الطلب '{fullRequest.Title}' من قبل {fullRequest.User?.FullName}";

                var adminNotification = await _notificationService.CreateNotificationAsync(
                    title: adminTitle,
                    message: adminMessage,
                    type: NotificationType.RequestDeleted,
                    userId: null, // للإدارة
                    requestId: fullRequest.Id,
                    isFromAdmin: false
                );

                await _notificationService.SendNotificationAsync(adminNotification);

                // 2. إشعار تأكيد للمستخدم
                var userTitle = "🗑️ تم حذف طلبك";
                var userMessage = $"تم حذف طلبك '{fullRequest.Title}' بنجاح.";

                var userNotification = await _notificationService.CreateNotificationAsync(
                    title: userTitle,
                    message: userMessage,
                    type: NotificationType.RequestDeleted,
                    userId: fullRequest.UserId
                );

                await _notificationService.SendNotificationAsync(userNotification);

                _logger.LogInformation("تم إرسال إشعارات حذف الطلب #{RequestId}", request.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إرسال إشعارات حذف الطلب #{RequestId}", request.Id);
            }
        }

        public async Task NotifyRelevantStoresAsync(Request request)
        {
            try
            {
                var fullRequest = await GetFullRequestAsync(request.Id);
                if (fullRequest == null) return;

                // البحث عن المتاجر المتخصصة بنفس الاختصاص والفئات الفرعية من المستوى الثاني
                var relevantStores = await GetRelevantStoresAsync(fullRequest);

                if (!relevantStores.Any())
                {
                    _logger.LogWarning("لم يتم العثور على متاجر متخصصة للطلب #{RequestId}", request.Id);
                    return;
                }

                var title = "🛒 طلب جديد في تخصصك!";
                var message = $"طلب جديد متاح في فئتك المتخصصة:\n\n" +
                             $"العنوان: {fullRequest.Title}\n" +
                             $"الفئة: {GetCategoryPath(fullRequest)}\n" +
                             $"الموقع: {fullRequest.City} - {fullRequest.District}\n" +
                             $"المشتري: {fullRequest.User?.FullName}\n\n" +
                             $"اضغط للاطلاع على التفاصيل وتقديم عرضك!";

                foreach (var store in relevantStores)
                {
                    var notification = await _notificationService.CreateNotificationAsync(
                        title: title,
                        message: message,
                        type: NotificationType.NewRequestForStore,
                        userId: store.Id,
                        requestId: fullRequest.Id,
                        link: $"/Requests/Details/{fullRequest.Id}"
                    );

                    await _notificationService.SendNotificationAsync(notification);
                }

                _logger.LogInformation("تم إرسال إشعارات للمتاجر المتخصصة ({Count} متجر) للطلب #{RequestId}", 
                    relevantStores.Count, request.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إرسال إشعارات المتاجر للطلب #{RequestId}", request.Id);
            }
        }

        #region Helper Methods

        private async Task<Request?> GetFullRequestAsync(int requestId)
        {
            return await _context.Requests
                .Include(r => r.User)
                .Include(r => r.Category)
                .Include(r => r.SubCategory1)
                .Include(r => r.SubCategory2)
                .Include(r => r.Images)
                .FirstOrDefaultAsync(r => r.Id == requestId);
        }

        private async Task<List<ApplicationUser>> GetRelevantStoresAsync(Request request)
        {
            var query = _context.Users
                .Where(u => u.UserType == UserType.Seller && 
                           u.IsStoreApproved && 
                           u.IsActive);

            // البحث بناءً على الفئات الفرعية من المستوى الثاني
            if (request.SubCategory2Id.HasValue)
            {
                query = query.Where(u => u.StoreCategories
                    .Any(sc => sc.SubCategory2Id == request.SubCategory2Id.Value));
            }
            else if (request.SubCategory1Id.HasValue)
            {
                query = query.Where(u => u.StoreCategories
                    .Any(sc => sc.SubCategory1Id == request.SubCategory1Id.Value));
            }
            else
            {
                query = query.Where(u => u.StoreCategories
                    .Any(sc => sc.CategoryId == request.CategoryId));
            }

            return await query.ToListAsync();
        }

        private string GetCategoryPath(Request request)
        {
            var path = request.Category?.Name ?? "غير محدد";
            
            if (request.SubCategory1 != null)
                path += $" > {request.SubCategory1.Name}";
                
            if (request.SubCategory2 != null)
                path += $" > {request.SubCategory2.Name}";
                
            return path;
        }

        #endregion
    }
}