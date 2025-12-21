// Controllers/RequestsController.cs - النسخة المحدثة مع دعم التعديل وإدارة الصور
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReverseMarket.Data;
using ReverseMarket.Models;
using ReverseMarket.Models.ViewModels;
using ReverseMarket.Services;
using Microsoft.AspNetCore.Identity;
using ReverseMarket.Models.Identity;
using ReverseMarket.CustomWhatsappService;

namespace ReverseMarket.Controllers
{
    public class RequestsController : BaseController
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IWhatsAppService _whatsAppService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RequestsController> _logger;
        private readonly WhatsAppService _customWhatsAppService;
        private readonly INotificationService _notificationService;
        private readonly IDefaultImageService _defaultImageService;
        private readonly IRequestWorkflowService _requestWorkflowService;

        public RequestsController(
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment,
            IWhatsAppService whatsAppService,
            UserManager<ApplicationUser> userManager,
            ILogger<RequestsController> logger,
            WhatsAppService customWhatsAppService,
            INotificationService notificationService,
            IDefaultImageService defaultImageService,
            IRequestWorkflowService requestWorkflowService)
            : base(context)
        {
            _webHostEnvironment = webHostEnvironment;
            _whatsAppService = whatsAppService;
            _userManager = userManager;
            _logger = logger;
            _customWhatsAppService = customWhatsAppService;
            _notificationService = notificationService;
            _defaultImageService = defaultImageService;
            _requestWorkflowService = requestWorkflowService;
        }

        public async Task<IActionResult> Index(string search, int? categoryId, int? subCategory1Id, int? subCategory2Id, int page = 1)
        {
            var pageSize = 12;

            var query = _context.Requests
                .Where(r => r.Status == RequestStatus.Approved)
                .Include(r => r.Category)
                .Include(r => r.SubCategory1)
                .Include(r => r.SubCategory2)
                .Include(r => r.Images)
                .Include(r => r.User)
                .AsQueryable();

            // البحث النصي
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r => r.Title.Contains(search) || r.Description.Contains(search));
            }

            // الفلترة حسب الفئات
            if (subCategory2Id.HasValue)
            {
                query = query.Where(r => r.SubCategory2Id == subCategory2Id.Value);
            }
            else if (subCategory1Id.HasValue)
            {
                query = query.Where(r => r.SubCategory1Id == subCategory1Id.Value);
            }
            else if (categoryId.HasValue)
            {
                query = query.Where(r => r.CategoryId == categoryId.Value);
            }

            var totalRequests = await query.CountAsync();
            var requests = await query
                .OrderByDescending(r => r.ApprovedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // جلب الفئات الفرعية بناءً على الاختيارات
            var subCategories1 = new List<SubCategory1>();
            var subCategories2 = new List<SubCategory2>();

            if (categoryId.HasValue)
            {
                subCategories1 = await _context.SubCategories1
                    .Where(sc => sc.CategoryId == categoryId.Value && sc.IsActive)
                    .ToListAsync();
            }

            if (subCategory1Id.HasValue)
            {
                subCategories2 = await _context.SubCategories2
                    .Where(sc => sc.SubCategory1Id == subCategory1Id.Value && sc.IsActive)
                    .ToListAsync();
            }

            var model = new RequestsViewModel
            {
                Requests = requests,
                Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync(),
                SubCategories1 = subCategories1,
                SubCategories2 = subCategories2,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalRequests / pageSize),
                Search = search,
                SelectedCategoryId = categoryId,
                SelectedSubCategory1Id = subCategory1Id,
                SelectedSubCategory2Id = subCategory2Id,
                Advertisements = await _context.Advertisements
                    .Where(a => a.IsActive &&
                           a.StartDate <= DateTime.Now &&
                           (a.EndDate == null || a.EndDate >= DateTime.Now))
                    .OrderBy(a => a.DisplayOrder)
                    .ThenBy(a => a.CreatedAt)
                    .ToListAsync(),
            };

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var request = await _context.Requests
                .Include(r => r.Category)
                .Include(r => r.SubCategory1)
                .Include(r => r.SubCategory2)
                .Include(r => r.Images)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id && r.Status == RequestStatus.Approved);

            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Create()
        {
            // التحقق من نوع المستخدم - فقط المشترين يمكنهم إضافة طلبات
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "جلسة المستخدم منتهية الصلاحية";
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                TempData["ErrorMessage"] = "المستخدم غير موجود";
                return RedirectToAction("Login", "Account");
            }

            // منع البائعين من إضافة طلبات
            if (user.UserType != UserType.Buyer)
            {
                TempData["ErrorMessage"] = "عذراً، إضافة الطلبات متاحة للمشترين فقط. البائعون يستطيعون الرد على الطلبات الموجودة.";
                return RedirectToAction("Index");
            }

            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(CreateRequestViewModel model)
        {
            // التحقق من وجود فئات فرعية ثانية للفئة المختارة
            if (model.SubCategory1Id.HasValue)
            {
                var hasSubCategories2 = await _context.SubCategories2.AnyAsync(s => s.SubCategory1Id == model.SubCategory1Id.Value);
                if (!hasSubCategories2)
                {
                    model.SubCategory2Id = null;
                }
            }

            if (ModelState.IsValid)
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "جلسة المستخدم منتهية الصلاحية";
                    return RedirectToAction("Login", "Account");
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || user.UserType != UserType.Buyer)
                {
                    TempData["ErrorMessage"] = "عذراً، إضافة الطلبات متاحة للمشترين فقط.";
                    return RedirectToAction("Index");
                }

                try
                {
                    var request = new Request
                    {
                        Title = model.Title,
                        Description = model.Description,
                        CategoryId = model.CategoryId,
                        SubCategory1Id = model.SubCategory1Id,
                        SubCategory2Id = model.SubCategory2Id,
                        City = model.City,
                        District = model.District,
                        Location = model.Location,
                        UserId = userId,
                        Status = RequestStatus.Pending,
                        CreatedAt = DateTime.Now
                    };

                    _context.Requests.Add(request);
                    await _context.SaveChangesAsync();

                    // معالجة رفع الصور المتعددة (حتى 5 صور)
                    if (model.Images != null && model.Images.Any())
                    {
                        await SaveRequestImagesAsync(request.Id, model.Images);
                    }

                    // إضافة صورة افتراضية إذا لم يتم رفع أي صورة
                    await _defaultImageService.AddDefaultImageIfNeededAsync(request.Id);

                    // إرسال إشعار للإدارة عن الطلب الجديد
                    await _requestWorkflowService.NotifyAdminAboutNewRequestAsync(request);

                    TempData["SuccessMessage"] = "تم إرسال طلبك بنجاح! سيتم مراجعته والموافقة عليه في أقرب وقت.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "خطأ في إنشاء الطلب");
                    TempData["ErrorMessage"] = "حدث خطأ أثناء إرسال الطلب. يرجى المحاولة مرة أخرى.";
                }
            }

            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            return View(model);
        }

        /// <summary>
        /// عرض طلبات المستخدم الحالي (للمشترين فقط)
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> MyRequests(int page = 1)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.UserType != UserType.Buyer)
            {
                TempData["ErrorMessage"] = "هذه الصفحة متاحة للمشترين فقط";
                return RedirectToAction("Index");
            }

            var pageSize = 10;

            var query = _context.Requests
                .Where(r => r.UserId == userId)
                .Include(r => r.Category)
                .Include(r => r.SubCategory1)
                .Include(r => r.SubCategory2)
                .Include(r => r.Images)
                .OrderByDescending(r => r.CreatedAt);

            var totalRequests = await query.CountAsync();
            var requests = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new MyRequestsViewModel
            {
                Requests = requests,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalRequests / pageSize)
            };

            return View(model);
        }

        /// <summary>
        /// عرض صفحة تعديل الطلب
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var request = await _context.Requests
                .Include(r => r.Category)
                .Include(r => r.SubCategory1)
                .Include(r => r.SubCategory2)
                .Include(r => r.Images)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (request == null)
            {
                TempData["ErrorMessage"] = "الطلب غير موجود أو ليس لديك صلاحية لتعديله";
                return RedirectToAction("MyRequests");
            }

            // تحميل الفئات
            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();

            // تحميل الفئات الفرعية إذا كانت موجودة
            if (request.CategoryId > 0)
            {
                ViewBag.SubCategories1 = await _context.SubCategories1
                    .Where(sc => sc.CategoryId == request.CategoryId && sc.IsActive)
                    .ToListAsync();
            }

            if (request.SubCategory1Id.HasValue)
            {
                ViewBag.SubCategories2 = await _context.SubCategories2
                    .Where(sc => sc.SubCategory1Id == request.SubCategory1Id.Value && sc.IsActive)
                    .ToListAsync();
            }

            // إضافة تنبيه للطلبات المعتمدة
            if (request.Status == RequestStatus.Approved)
            {
                TempData["InfoMessage"] = "تنبيه: هذا الطلب معتمد حالياً. أي تعديل سيتطلب موافقة الإدارة مرة أخرى وسيتم إخفاء الطلب مؤقتاً حتى الموافقة.";
            }

            var model = new EditRequestViewModel
            {
                Id = request.Id,
                Title = request.Title,
                Description = request.Description,
                CategoryId = request.CategoryId,
                SubCategory1Id = request.SubCategory1Id,
                SubCategory2Id = request.SubCategory2Id,
                City = request.City,
                District = request.District,
                Location = request.Location,
                ExistingImages = request.Images.ToList()
            };

            return View(model);
        }

        /// <summary>
        /// حفظ تعديلات الطلب
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(EditRequestViewModel model)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var request = await _context.Requests
                .Include(r => r.Images)
                .FirstOrDefaultAsync(r => r.Id == model.Id && r.UserId == userId);

            if (request == null)
            {
                TempData["ErrorMessage"] = "الطلب غير موجود أو ليس لديك صلاحية لتعديله";
                return RedirectToAction("MyRequests");
            }

            // حفظ الحالة السابقة للطلب
            var previousStatus = request.Status;
            var wasApproved = previousStatus == RequestStatus.Approved;

            // التحقق من وجود فئات فرعية ثانية للفئة المختارة
            if (model.SubCategory1Id.HasValue)
            {
                var hasSubCategories2 = await _context.SubCategories2.AnyAsync(s => s.SubCategory1Id == model.SubCategory1Id.Value);
                if (!hasSubCategories2)
                {
                    model.SubCategory2Id = null;
                    ModelState.Remove("SubCategory2Id");
                }
            }

            if (!ModelState.IsValid)
            {
                // إعادة تحميل البيانات للعرض
                model.ExistingImages = request.Images.ToList();
                ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();

                if (model.CategoryId > 0)
                {
                    ViewBag.SubCategories1 = await _context.SubCategories1
                        .Where(sc => sc.CategoryId == model.CategoryId && sc.IsActive)
                        .ToListAsync();
                }

                if (model.SubCategory1Id.HasValue)
                {
                    ViewBag.SubCategories2 = await _context.SubCategories2
                        .Where(sc => sc.SubCategory1Id == model.SubCategory1Id.Value && sc.IsActive)
                        .ToListAsync();
                }

                return View(model);
            }

            try
            {
                // حذف الصور المحددة
                if (model.ImagesToDelete != null && model.ImagesToDelete.Any())
                {
                    await DeleteRequestImagesAsync(model.ImagesToDelete, request.Id);
                }

                // إعادة تحميل الصور بعد الحذف
                await _context.Entry(request).Collection(r => r.Images).LoadAsync();

                // حساب عدد الصور المتبقية بعد الحذف
                var remainingImagesCount = request.Images.Count;
                var maxNewImages = 5 - remainingImagesCount;

                // رفع الصور الجديدة
                if (model.NewImages != null && model.NewImages.Any())
                {
                    var imagesToUpload = model.NewImages.Take(maxNewImages).ToList();
                    if (imagesToUpload.Any())
                    {
                        await SaveRequestImagesAsync(request.Id, imagesToUpload);
                    }

                    if (model.NewImages.Count > maxNewImages)
                    {
                        TempData["WarningMessage"] = $"تم رفع {maxNewImages} صور فقط. الحد الأقصى هو 5 صور للطلب الواحد.";
                    }
                }

                // تحديث بيانات الطلب
                request.Title = model.Title;
                request.Description = model.Description;
                request.CategoryId = model.CategoryId;
                request.SubCategory1Id = model.SubCategory1Id;
                request.SubCategory2Id = model.SubCategory2Id;
                request.City = model.City;
                request.District = model.District;
                request.Location = model.Location;
                request.LastModifiedAt = DateTime.Now;
                request.IsModified = true;
                request.ModificationCount++;

                // تغيير الحالة إلى "تعديل قيد المراجعة" لجميع الحالات
                request.Status = RequestStatus.ModificationPending;

                await _context.SaveChangesAsync();

                // إرسال إشعار للإدارة عن التعديل
                await NotifyAdminAboutRequestModificationAsync(request, wasApproved);

                if (wasApproved)
                {
                    TempData["SuccessMessage"] = "تم حفظ التعديلات بنجاح! الطلب الآن في انتظار موافقة الإدارة وسيعود للظهور بعد الموافقة.";
                }
                else
                {
                    TempData["SuccessMessage"] = "تم حفظ التعديلات بنجاح! سيتم مراجعتها من قبل الإدارة.";
                }

                return RedirectToAction("MyRequests");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تعديل الطلب #{RequestId}", model.Id);
                TempData["ErrorMessage"] = "حدث خطأ أثناء حفظ التعديلات. يرجى المحاولة مرة أخرى.";
            }

            // إعادة تحميل البيانات للعرض في حالة الخطأ
            model.ExistingImages = request.Images.ToList();
            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            return View(model);
        }

        /// <summary>
        /// حذف صورة من الطلب (AJAX)
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteImage(int imageId, int requestId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "جلسة منتهية" });
                }

                var request = await _context.Requests
                    .Include(r => r.Images)
                    .FirstOrDefaultAsync(r => r.Id == requestId && r.UserId == userId);

                if (request == null)
                {
                    return Json(new { success = false, message = "الطلب غير موجود" });
                }

                if (request.Status == RequestStatus.Approved)
                {
                    return Json(new { success = false, message = "لا يمكن تعديل طلب معتمد" });
                }

                var image = request.Images.FirstOrDefault(i => i.Id == imageId);
                if (image == null)
                {
                    return Json(new { success = false, message = "الصورة غير موجودة" });
                }

                // حذف الملف الفعلي
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, image.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                _context.RequestImages.Remove(image);
                await _context.SaveChangesAsync();

                _logger.LogInformation("تم حذف الصورة #{ImageId} من الطلب #{RequestId}", imageId, requestId);

                return Json(new { success = true, message = "تم حذف الصورة بنجاح", remainingCount = request.Images.Count - 1 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في حذف الصورة #{ImageId}", imageId);
                return Json(new { success = false, message = "حدث خطأ أثناء حذف الصورة" });
            }
        }

        /// <summary>
        /// رفع صور جديدة للطلب (AJAX)
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UploadImages(int requestId, List<IFormFile> images)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "جلسة منتهية" });
                }

                var request = await _context.Requests
                    .Include(r => r.Images)
                    .FirstOrDefaultAsync(r => r.Id == requestId && r.UserId == userId);

                if (request == null)
                {
                    return Json(new { success = false, message = "الطلب غير موجود" });
                }

                if (request.Status == RequestStatus.Approved)
                {
                    return Json(new { success = false, message = "لا يمكن تعديل طلب معتمد" });
                }

                var currentCount = request.Images.Count;
                var maxAllowed = 5 - currentCount;

                if (maxAllowed <= 0)
                {
                    return Json(new { success = false, message = "تم الوصول للحد الأقصى من الصور (5 صور)" });
                }

                if (images == null || !images.Any())
                {
                    return Json(new { success = false, message = "لم يتم اختيار صور" });
                }

                var imagesToUpload = images.Take(maxAllowed).ToList();
                var uploadedImages = await SaveRequestImagesAsync(request.Id, imagesToUpload);

                return Json(new
                {
                    success = true,
                    message = $"تم رفع {uploadedImages.Count} صورة بنجاح",
                    uploadedImages = uploadedImages.Select(img => new { img.Id, img.ImagePath }),
                    totalCount = currentCount + uploadedImages.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في رفع الصور للطلب #{RequestId}", requestId);
                return Json(new { success = false, message = "حدث خطأ أثناء رفع الصور" });
            }
        }

        /// <summary>
        /// حذف طلب
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Delete([FromForm] int id)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "جلسة منتهية. يرجى تسجيل الدخول مرة أخرى." });
            }

            var request = await _context.Requests
                .Include(r => r.Images)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (request == null)
            {
                return Json(new { success = false, message = "الطلب غير موجود أو ليس لديك صلاحية لحذفه" });
            }

            try
            {
                var wasApproved = request.Status == RequestStatus.Approved;
                var requestTitle = request.Title;

                // حذف الإشعارات المرتبطة بالطلب أولاً
                var relatedNotifications = await _context.Notifications
                    .Where(n => n.RequestId == id)
                    .ToListAsync();

                if (relatedNotifications.Any())
                {
                    _context.Notifications.RemoveRange(relatedNotifications);
                }

                // حذف الصور المرتبطة من الملفات
                foreach (var image in request.Images)
                {
                    try
                    {
                        var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, image.ImagePath.TrimStart('/'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "فشل في حذف ملف الصورة: {ImagePath}", image.ImagePath);
                    }
                }

                // حذف الصور من قاعدة البيانات
                _context.RequestImages.RemoveRange(request.Images);

                // حذف الطلب
                _context.Requests.Remove(request);
                await _context.SaveChangesAsync();

                // إرسال إشعار عن الحذف
                await NotifyAboutRequestDeletionAsync(requestTitle, userId, wasApproved);

                _logger.LogInformation("تم حذف الطلب #{RequestId} بواسطة المستخدم {UserId}", id, userId);

                return Json(new { success = true, message = "تم حذف الطلب بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في حذف الطلب #{RequestId}", id);
                return Json(new { success = false, message = "حدث خطأ أثناء حذف الطلب. يرجى المحاولة مرة أخرى." });
            }
        }

        #region Helper Methods

        private async Task<List<RequestImage>> SaveRequestImagesAsync(int requestId, List<IFormFile> images)
        {
            var savedImages = new List<RequestImage>();

            try
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "requests");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var maxFileSize = 5 * 1024 * 1024; // 5 MB

                foreach (var image in images)
                {
                    if (image?.Length > 0)
                    {
                        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

                        if (!allowedExtensions.Contains(extension))
                        {
                            _logger.LogWarning("نوع الملف غير مدعوم: {FileName}", image.FileName);
                            continue;
                        }

                        if (image.Length > maxFileSize)
                        {
                            _logger.LogWarning("حجم الملف كبير جداً: {FileName}", image.FileName);
                            continue;
                        }

                        var fileName = $"{Guid.NewGuid()}{extension}";
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(fileStream);
                        }

                        var requestImage = new RequestImage
                        {
                            RequestId = requestId,
                            ImagePath = $"/uploads/requests/{fileName}",
                            CreatedAt = DateTime.Now
                        };

                        _context.RequestImages.Add(requestImage);
                        savedImages.Add(requestImage);
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("تم حفظ {Count} صورة للطلب #{RequestId}", savedImages.Count, requestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في حفظ الصور للطلب #{RequestId}", requestId);
            }

            return savedImages;
        }

        private async Task DeleteRequestImagesAsync(List<int> imageIds, int requestId)
        {
            try
            {
                var images = await _context.RequestImages
                    .Where(i => imageIds.Contains(i.Id) && i.RequestId == requestId)
                    .ToListAsync();

                foreach (var image in images)
                {
                    // حذف الملف الفعلي
                    var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, image.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }

                    _context.RequestImages.Remove(image);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("تم حذف {Count} صورة من الطلب #{RequestId}", images.Count, requestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في حذف الصور من الطلب #{RequestId}", requestId);
            }
        }

        private async Task NotifyAdminAboutRequestModificationAsync(Request request, bool wasApproved = false)
        {
            try
            {
                var fullRequest = await _context.Requests
                    .Include(r => r.User)
                    .Include(r => r.Category)
                    .Include(r => r.SubCategory1)
                    .Include(r => r.SubCategory2)
                    .FirstOrDefaultAsync(r => r.Id == request.Id);

                if (fullRequest == null) return;

                var categoryPath = fullRequest.Category?.Name ?? "غير محدد";
                if (fullRequest.SubCategory1 != null)
                {
                    categoryPath += $" > {fullRequest.SubCategory1.Name}";
                }
                if (fullRequest.SubCategory2 != null)
                {
                    categoryPath += $" > {fullRequest.SubCategory2.Name}";
                }

                var title = wasApproved
                    ? "⚠️ طلب معتمد تم تعديله - يحتاج مراجعة عاجلة"
                    : "طلب تم تعديله يحتاج مراجعة";

                var priorityNote = wasApproved
                    ? "\n⚠️ ملاحظة: هذا الطلب كان معتمداً سابقاً وتم إخفاؤه مؤقتاً"
                    : "";

                var notification = await _notificationService.CreateNotificationAsync(
                    title: title,
                    message: $"تم تعديل الطلب من {fullRequest.User?.FirstName} {fullRequest.User?.LastName}\n" +
                            $"العنوان: {fullRequest.Title}\n" +
                            $"الفئة: {categoryPath}\n" +
                            $"عدد التعديلات: {fullRequest.ModificationCount}" +
                            priorityNote,
                    type: NotificationType.RequestModified,
                    targetUserType: null,
                    requestId: request.Id,
                    link: $"/Admin/Requests/Details/{request.Id}",
                    isFromAdmin: false
                );

                await _notificationService.SendNotificationAsync(notification, sendEmail: true, sendWhatsApp: wasApproved, sendInApp: true);

                _logger.LogInformation("تم إرسال إشعار للإدارة عن تعديل الطلب #{RequestId} (كان معتمداً: {WasApproved})", request.Id, wasApproved);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إرسال إشعار للإدارة عن تعديل الطلب #{RequestId}", request.Id);
            }
        }

        private async Task NotifyAboutRequestDeletionAsync(string requestTitle, string odeletedByUserId, bool wasApproved)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(odeletedByUserId);

                var title = wasApproved
                    ? "⚠️ تم حذف طلب معتمد"
                    : "تم حذف طلب";

                var adminNotification = await _notificationService.CreateNotificationAsync(
                    title: title,
                    message: $"تم حذف الطلب: {requestTitle}\n" +
                            $"من المستخدم: {user?.FirstName} {user?.LastName}" +
                            (wasApproved ? "\n⚠️ الطلب كان معتمداً" : ""),
                    type: NotificationType.RequestDeleted,
                    targetUserType: null,
                    isFromAdmin: false
                );

                await _notificationService.SendNotificationAsync(adminNotification, sendEmail: wasApproved, sendWhatsApp: false, sendInApp: true);

                _logger.LogInformation("تم إرسال إشعارات حذف الطلب '{RequestTitle}'", requestTitle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إرسال إشعارات حذف الطلب");
            }
        }

        private async Task NotifyRequestOwnerAsync(Request request, NotificationType notificationType, string title, string message)
        {
            try
            {
                if (request.UserId == null)
                    return;

                var notification = await _notificationService.CreateNotificationAsync(
                    title: title,
                    message: message,
                    type: notificationType,
                    targetUserType: null,
                    requestId: request.Id,
                    userId: request.UserId,
                    isFromAdmin: false
                );

                await _notificationService.SendNotificationAsync(notification, sendEmail: true, sendWhatsApp: false, sendInApp: true);

                _logger.LogInformation("تم إرسال إشعار لصاحب الطلب #{RequestId}", request.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إرسال إشعار لصاحب الطلب #{RequestId}", request.Id);
            }
        }

        #endregion

        #region API Endpoints

        [HttpGet]
        public async Task<IActionResult> GetSubCategories1(int categoryId)
        {
            var subCategories = await _context.SubCategories1
                .Where(sc => sc.CategoryId == categoryId && sc.IsActive)
                .Select(sc => new { id = sc.Id, name = sc.Name })
                .ToListAsync();

            return Json(subCategories);
        }

        [HttpGet]
        public async Task<IActionResult> GetSubCategories2(int subCategory1Id)
        {
            var subCategories = await _context.SubCategories2
                .Where(sc => sc.SubCategory1Id == subCategory1Id && sc.IsActive)
                .Select(sc => new { id = sc.Id, name = sc.Name })
                .ToListAsync();

            return Json(subCategories);
        }

        #endregion
    }
}