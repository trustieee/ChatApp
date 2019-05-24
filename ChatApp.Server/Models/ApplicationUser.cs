using System;
using System.Linq.Expressions;
using ChatApp.Server.Models.DTO;
using Microsoft.AspNetCore.Identity;

namespace ChatApp.Server.Models
{
    public class ApplicationUser : IdentityUser
    {
        public static readonly Expression<Func<ApplicationUser, UserDTO>> Projection = c => new UserDTO
        {
            Id = c.Id,
            UserName = c.UserName
        };
    }
}