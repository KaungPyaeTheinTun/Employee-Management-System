using System.Security.Claims;

namespace EmployeesManagement.Services
{
    public static class UserProfileService
    {
        public static string GetUserId(this ClaimsPrincipal user)
        {
            if (!user.Identity.IsAuthenticated)
            {
                return null;
            }
            else
            {
                ClaimsPrincipal currentloggedinuser = user;
                if (currentloggedinuser != null)
                {
                    return currentloggedinuser.FindFirst(ClaimTypes.NameIdentifier).Value;
                }
                else
                {
                    return null;
                }

            }
        }

        public static string GetUserName(this ClaimsPrincipal user)
        {
            if (!user.Identity.IsAuthenticated)
            {
                return null;
            }
            else
            {
                ClaimsPrincipal currentloggedinuser = user;
                if (currentloggedinuser != null)
                {
                    return currentloggedinuser.FindFirst(ClaimTypes.Name).Value;
                }
                else
                {
                    return null;
                }

            }
        }

        public static string GetUserEmail(this ClaimsPrincipal user)
        {
            if (!user.Identity.IsAuthenticated)
            {
                return null;
            }
            else
            {
                ClaimsPrincipal currentloggedinuser = user;
                if (currentloggedinuser != null)
                {
                    return currentloggedinuser.FindFirst(ClaimTypes.Email).Value;
                }
                else
                {
                    return null;
                }

            }
        }

        public static string GetUserRoleId(this ClaimsPrincipal user)
        {
            if (!user.Identity.IsAuthenticated)
            {
                return null;
            }
            else
            {
                ClaimsPrincipal currentloggedinuser = user;
                if (currentloggedinuser != null)
                {
                    return currentloggedinuser.FindFirst("RoleId").Value;
                }
                else
                {
                    return null;
                }

            }
        }
    }
}
