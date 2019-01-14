using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Base_AVL.Toastr;

namespace Base_AVL.Controllers
{
    public abstract class MessageControllerBase : Controller
    {
        public MessageControllerBase()
        {
            Toastr = new Base_AVL.Toastr.Toastr();
        }
        public Base_AVL.Toastr.Toastr Toastr { get; set; }

        public ToastMessage AddToastMessage(string title, string message, ToastType toastType)
        {
            return Toastr.AddToastMessage(title, message, toastType);
        }
    }
}