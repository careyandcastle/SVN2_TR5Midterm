using Microsoft.AspNetCore.Http;
using TscLibCore.Authority;
using TscLibCore.Modules;

namespace TR5MidTerm.Controllers
{
    public static class HttpContextExtension
    {
        public static UserAccountForSession UA(this HttpContext HttpContext)
        {
            return HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));
        }
    }
}
