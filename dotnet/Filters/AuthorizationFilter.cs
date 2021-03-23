using System;
using System.Security.Principal;
using Microsoft.AspNetCore.Mvc.Filters;
using ReviewsRatings.Models;
using ReviewsRatings.Services;

namespace ReviewsRatings.Filters
{
    public class AuthorizationFilter : IAuthorizationFilter
    {
        private const string HEADER_VTEX_COOKIE = "VtexIdclientAutCookie";
        private const string AUTH_SUCCESS = "Success";
        private const string HEADER_VTEX_APP_KEY = "X-VTEX-API-AppKey";
        private const string HEADER_VTEX_APP_TOKEN = "X-VTEX-API-AppToken";
        private const string FORWARDED_HOST = "X-Forwarded-Host";

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var productReviewService =
                context.HttpContext.RequestServices.GetService(typeof(IProductReviewService)) as IProductReviewService;

            //Response.Headers.Add("Cache-Control", "no-cache");
            var vtexCookie = context.HttpContext.Request.Headers[HEADER_VTEX_COOKIE];
            ValidatedUser validatedUser = null;
            var keyAndTokenValid = false;
            var vtexAppKey = context.HttpContext.Request.Headers[HEADER_VTEX_APP_KEY];
            var vtexAppToken = context.HttpContext.Request.Headers[HEADER_VTEX_APP_TOKEN];
            if (!string.IsNullOrEmpty(vtexCookie))
            {
                validatedUser = productReviewService!.ValidateUserToken(vtexCookie).Result;
                if (validatedUser == null)
                {
                    throw new InvalidOperationException();
                }

                if (!validatedUser.AuthStatus.Equals(AUTH_SUCCESS))
                {
                    throw new InvalidOperationException();
                }
            }

            if(!string.IsNullOrEmpty(vtexAppKey) && !string.IsNullOrEmpty(vtexAppToken))
            {
                string baseUrl = context.HttpContext.Request.Headers[FORWARDED_HOST];
                keyAndTokenValid = productReviewService!.ValidateKeyAndToken(vtexAppKey, vtexAppToken, baseUrl).Result;
            }
            
            if (!keyAndTokenValid)
            {
                throw new InvalidOperationException();
            }

            context.HttpContext.User = new GenericPrincipal(new GenericIdentity(validatedUser!.Id), new string[1]);
            throw new System.NotImplementedException();
        }
    }
}