using Base_AVL.Controllers;
using System.Web;
using System.Web.Mvc;

namespace Base_AVL
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new MessagesActionFilter());
        }
    }
}
