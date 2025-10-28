using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Responses;

namespace Application.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserResponse> GetByIdAsync(string id);
    }
}
