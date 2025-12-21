using Microsoft.AspNetCore.Mvc;

namespace ReverseMarket.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/AccessDenied")]
        public IActionResult AccessDenied()
        {
            return View("~/Views/Shared/AccessDenied.cshtml");
        }

        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            switch (statusCode)
            {
                case 403:
                    return View("~/Views/Shared/AccessDenied.cshtml");
                case 404:
                    ViewBag.ErrorMessage = "الصفحة المطلوبة غير موجودة";
                    break;
                case 500:
                    ViewBag.ErrorMessage = "خطأ داخلي في الخادم";
                    break;
                default:
                    ViewBag.ErrorMessage = "حدث خطأ غير متوقع";
                    break;
            }

            ViewBag.StatusCode = statusCode;
            return View("Error");
        }
    }
}