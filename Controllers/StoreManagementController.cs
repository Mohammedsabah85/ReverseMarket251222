// Controllers/StoreManagementController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReverseMarket.Data;
using ReverseMarket.Models;
using ReverseMarket.Models.Identity;
using ReverseMarket.Models.ViewModels;
using ReverseMarket.Services;
using System.Text.Json;

namespace ReverseMarket.Controllers
{
    /// <summary>
    /// Controller لإدارة معلومات المتجر للبائعين
    /// </summary>
    [Authorize]
    public class StoreManagementController : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<StoreManagementController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly INotificationService _notificationService;

        public StoreManagementController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<StoreManagementController> logger,
            IWebHostEnvironment webHostEnvironment,
            INotificationService notificationService)
            : base(context)
        {
            _userManager = userManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _notificationService = notificationService;
        }

        /// <summary>
        /// عرض صفحة تعديل المتجر
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.Users
                .Include(u => u.StoreCategories)
                    .ThenInclude(sc => sc.Category)
                .Include(u => u.StoreCategories)
                    .ThenInclude(sc => sc.SubCategory1)
                .Include(u => u.StoreCategories)
                    .ThenInclude(sc => sc.SubCategory2)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                TempData["ErrorMessage"] = "المستخدم غير موجود";
                return RedirectToAction("Index", "Home");
            }

            // التحقق من أن المستخدم بائع
            if (user.UserType != UserType.Seller)
            {
                TempData["ErrorMessage"] = "هذه الصفحة متاحة للبائعين فقط";
                return RedirectToAction("Index", "Profile");
            }

            var model = new EditStoreProfileViewModel
            {
                StoreName = user.StoreName ?? string.Empty,
                StoreDescription = user.StoreDescription,
                City = user.City ?? string.Empty,
                District = user.District ?? string.Empty,
                Location = user.Location,
                CurrentProfileImage = user.ProfileImage,
                WebsiteUrl1 = user.WebsiteUrl1,
                WebsiteUrl2 = user.WebsiteUrl2,
                WebsiteUrl3 = user.WebsiteUrl3,
                PendingWebsiteUrl1 = user.PendingWebsiteUrl1,
                PendingWebsiteUrl2 = user.PendingWebsiteUrl2,
                PendingWebsiteUrl3 = user.PendingWebsiteUrl3,
                HasPendingUrlChanges = !string.IsNullOrEmpty(user.PendingWebsiteUrl1) ||
                                      !string.IsNullOrEmpty(user.PendingWebsiteUrl2) ||
                                      !string.IsNullOrEmpty(user.PendingWebsiteUrl3),
                IsStoreApproved = user.IsStoreApproved,
                CreatedAt = user.CreatedAt,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                CurrentStoreCategories = user.StoreCategories.Select(sc => new StoreCategoryDisplayItem
                {
                    Id = sc.Id,
                    CategoryId = sc.CategoryId,
                    SubCategory1Id = sc.SubCategory1Id,
                    SubCategory2Id = sc.SubCategory2Id,
                    CategoryName = sc.Category?.Name ?? "",
                    SubCategory1Name = sc.SubCategory1?.Name,
                    SubCategory2Name = sc.SubCategory2?.Name
                }).ToList()
            };

            // تحديد حالة الروابط
            model.Url1Status = GetUrlStatus(user.WebsiteUrl1, user.PendingWebsiteUrl1);
            model.Url2Status = GetUrlStatus(user.WebsiteUrl2, user.PendingWebsiteUrl2);
            model.Url3Status = GetUrlStatus(user.WebsiteUrl3, user.PendingWebsiteUrl3);

            // تحميل الفئات للاختيار
            await LoadCategoriesForView();

            return View(model);
        }

        /// <summary>
        /// حفظ تعديلات المتجر
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditStoreProfileViewModel model)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.Users
                .Include(u => u.StoreCategories)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || user.UserType != UserType.Seller)
            {
                TempData["ErrorMessage"] = "غير مصرح لك بتعديل هذه الصفحة";
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                await LoadCategoriesForView();
                model.CurrentStoreCategories = await GetCurrentStoreCategoriesAsync(userId);
                return View(model);
            }

            try
            {
                // تحديث المعلومات الأساسية
                user.StoreName = model.StoreName;
                user.StoreDescription = model.StoreDescription;
                user.City = model.City;
                user.District = model.District;
                user.Location = model.Location;

                // معالجة صورة المتجر
                if (model.RemoveCurrentImage && !string.IsNullOrEmpty(user.ProfileImage))
                {
                    DeleteOldImage(user.ProfileImage);
                    user.ProfileImage = null;
                }

                if (model.NewProfileImage != null && model.NewProfileImage.Length > 0)
                {
                    var imagePath = await SaveProfileImageAsync(model.NewProfileImage);
                    if (!string.IsNullOrEmpty(imagePath))
                    {
                        // حذف الصورة القديمة
                        if (!string.IsNullOrEmpty(user.ProfileImage))
                        {
                            DeleteOldImage(user.ProfileImage);
                        }
                        user.ProfileImage = imagePath;
                    }
                }

                // معالجة الروابط (تحتاج موافقة الإدارة)
                await ProcessUrlChangesAsync(user, model);

                // معالجة تخصصات المتجر
                if (!string.IsNullOrEmpty(model.StoreCategories))
                {
                    await UpdateStoreCategoriesAsync(user, model.StoreCategories);
                }

                await _userManager.UpdateAsync(user);

                _logger.LogInformation("تم تحديث معلومات المتجر للمستخدم {UserId}", userId);

                // إرسال إشعار للإدارة إذا تم تغيير الروابط
                if (HasUrlChanges(user))
                {
                    await NotifyAdminAboutUrlChangesAsync(user);
                    TempData["SuccessMessage"] = "تم حفظ التعديلات بنجاح! الروابط الجديدة في انتظار موافقة الإدارة.";
                }
                else
                {
                    TempData["SuccessMessage"] = "تم حفظ التعديلات بنجاح!";
                }

                return RedirectToAction("Edit");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحديث معلومات المتجر للمستخدم {UserId}", userId);
                TempData["ErrorMessage"] = "حدث خطأ أثناء حفظ التعديلات. يرجى المحاولة مرة أخرى.";
            }

            await LoadCategoriesForView();
            model.CurrentStoreCategories = await GetCurrentStoreCategoriesAsync(userId);
            return View(model);
        }

        /// <summary>
        /// حذف صورة المتجر (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeleteProfileImage()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "جلسة منتهية" });
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || user.UserType != UserType.Seller)
                {
                    return Json(new { success = false, message = "غير مصرح" });
                }

                if (!string.IsNullOrEmpty(user.ProfileImage))
                {
                    DeleteOldImage(user.ProfileImage);
                    user.ProfileImage = null;
                    await _userManager.UpdateAsync(user);
                }

                return Json(new { success = true, message = "تم حذف الصورة بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في حذف صورة المتجر");
                return Json(new { success = false, message = "حدث خطأ أثناء حذف الصورة" });
            }
        }

        /// <summary>
        /// حذف تخصص من المتجر (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RemoveCategory(int categoryId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "جلسة منتهية" });
                }

                var storeCategory = await _context.StoreCategories
                    .FirstOrDefaultAsync(sc => sc.Id == categoryId && sc.UserId == userId);

                if (storeCategory == null)
                {
                    return Json(new { success = false, message = "التخصص غير موجود" });
                }

                _context.StoreCategories.Remove(storeCategory);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "تم حذف التخصص بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في حذف تخصص المتجر");
                return Json(new { success = false, message = "حدث خطأ أثناء حذف التخصص" });
            }
        }

        #region Helper Methods

        private string GetUrlStatus(string? currentUrl, string? pendingUrl)
        {
            if (!string.IsNullOrEmpty(pendingUrl))
                return "pending";
            if (!string.IsNullOrEmpty(currentUrl))
                return "approved";
            return "empty";
        }

        private bool HasUrlChanges(ApplicationUser user)
        {
            return !string.IsNullOrEmpty(user.PendingWebsiteUrl1) ||
                   !string.IsNullOrEmpty(user.PendingWebsiteUrl2) ||
                   !string.IsNullOrEmpty(user.PendingWebsiteUrl3);
        }

        private async Task ProcessUrlChangesAsync(ApplicationUser user, EditStoreProfileViewModel model)
        {
            // معالجة الرابط الأول
            if (model.WebsiteUrl1 != user.WebsiteUrl1)
            {
                if (string.IsNullOrEmpty(model.WebsiteUrl1))
                {
                    // طلب حذف الرابط
                    user.PendingWebsiteUrl1 = "[DELETE]";
                }
                else
                {
                    user.PendingWebsiteUrl1 = model.WebsiteUrl1;
                }
            }

            // معالجة الرابط الثاني
            if (model.WebsiteUrl2 != user.WebsiteUrl2)
            {
                if (string.IsNullOrEmpty(model.WebsiteUrl2))
                {
                    user.PendingWebsiteUrl2 = "[DELETE]";
                }
                else
                {
                    user.PendingWebsiteUrl2 = model.WebsiteUrl2;
                }
            }

            // معالجة الرابط الثالث
            if (model.WebsiteUrl3 != user.WebsiteUrl3)
            {
                if (string.IsNullOrEmpty(model.WebsiteUrl3))
                {
                    user.PendingWebsiteUrl3 = "[DELETE]";
                }
                else
                {
                    user.PendingWebsiteUrl3 = model.WebsiteUrl3;
                }
            }
        }

        private async Task UpdateStoreCategoriesAsync(ApplicationUser user, string categoriesJson)
        {
            try
            {
                var categoryIds = JsonSerializer.Deserialize<List<int>>(categoriesJson);
                if (categoryIds == null || !categoryIds.Any())
                    return;

                // حذف الفئات القديمة
                var existingCategories = await _context.StoreCategories
                    .Where(sc => sc.UserId == user.Id)
                    .ToListAsync();
                _context.StoreCategories.RemoveRange(existingCategories);

                // إضافة الفئات الجديدة
                foreach (var subCategory2Id in categoryIds.Distinct())
                {
                    var subCategory2 = await _context.SubCategories2
                        .Include(sc => sc.SubCategory1)
                        .FirstOrDefaultAsync(sc => sc.Id == subCategory2Id);

                    if (subCategory2 != null)
                    {
                        var storeCategory = new StoreCategory
                        {
                            UserId = user.Id,
                            CategoryId = subCategory2.SubCategory1?.CategoryId ?? 0,
                            SubCategory1Id = subCategory2.SubCategory1Id,
                            SubCategory2Id = subCategory2Id,
                            CreatedAt = DateTime.Now
                        };
                        _context.StoreCategories.Add(storeCategory);
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحديث تخصصات المتجر");
            }
        }

        private async Task<string?> SaveProfileImageAsync(IFormFile image)
        {
            try
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profiles");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    _logger.LogWarning("نوع ملف غير مدعوم: {Extension}", extension);
                    return null;
                }

                if (image.Length > 5 * 1024 * 1024) // 5 MB
                {
                    _logger.LogWarning("حجم الملف كبير جداً");
                    return null;
                }

                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }

                return $"/uploads/profiles/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في حفظ صورة الملف الشخصي");
                return null;
            }
        }

        private void DeleteOldImage(string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath) || imagePath.Contains("default"))
                    return;

                var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, imagePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في حذف الصورة القديمة");
            }
        }

        private async Task LoadCategoriesForView()
        {
            ViewBag.Categories = await _context.Categories
                .Where(c => c.IsActive)
                .ToListAsync();
        }

        private async Task<List<StoreCategoryDisplayItem>> GetCurrentStoreCategoriesAsync(string userId)
        {
            return await _context.StoreCategories
                .Where(sc => sc.UserId == userId)
                .Include(sc => sc.Category)
                .Include(sc => sc.SubCategory1)
                .Include(sc => sc.SubCategory2)
                .Select(sc => new StoreCategoryDisplayItem
                {
                    Id = sc.Id,
                    CategoryId = sc.CategoryId,
                    SubCategory1Id = sc.SubCategory1Id,
                    SubCategory2Id = sc.SubCategory2Id,
                    CategoryName = sc.Category.Name,
                    SubCategory1Name = sc.SubCategory1 != null ? sc.SubCategory1.Name : null,
                    SubCategory2Name = sc.SubCategory2 != null ? sc.SubCategory2.Name : null
                })
                .ToListAsync();
        }

        private async Task NotifyAdminAboutUrlChangesAsync(ApplicationUser user)
        {
            try
            {
                var notification = await _notificationService.CreateNotificationAsync(
                    title: "طلب تعديل روابط متجر",
                    message: $"المتجر: {user.StoreName}\n" +
                            $"البائع: {user.FirstName} {user.LastName}\n" +
                            "قام بتحديث روابط المتجر ويحتاج موافقة الإدارة",
                    type: NotificationType.StoreUrlChangeRequest,
                    targetUserType: null,
                    userId: user.Id,
                    link: $"/Admin/Users/Edit/{user.Id}",
                    isFromAdmin: false
                );

                await _notificationService.SendNotificationAsync(notification, sendEmail: true, sendWhatsApp: false, sendInApp: true);

                _logger.LogInformation("تم إرسال إشعار للإدارة عن تعديل روابط المتجر {StoreName}", user.StoreName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إرسال إشعار تعديل روابط المتجر");
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
