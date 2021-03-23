using System.Security.Principal;
using Microsoft.AspNetCore.Mvc;

namespace ReviewsRatings.Controllers
{
    public class ControllerBase : Controller
    {
        protected IIdentity VtexIdentity => HttpContext.User.Identity;
    }
}