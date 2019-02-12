using System.Web.Mvc;
using IntelliTraxx.Toastr;

namespace IntelliTraxx.Controllers
{
    public abstract class MessageControllerBase : Controller
    {
        public MessageControllerBase()
        {
            Toastr = new Toastr.Toastr();
        }
        public Toastr.Toastr Toastr { get; set; }

        public ToastMessage AddToastMessage(string title, string message, ToastType toastType)
        {
            return Toastr.AddToastMessage(title, message, toastType);
        }
    }
}