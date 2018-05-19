using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;

namespace WebApplication.Attributes
{
    public class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            if (httpContext.Request.Cookies.Get("email") != null && httpContext.Request.Cookies.Get("status") != null && httpContext.Request.Cookies.Get("id") != null)
            {
                if (httpContext.Request.Cookies.Get("status").Value == "admin")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
