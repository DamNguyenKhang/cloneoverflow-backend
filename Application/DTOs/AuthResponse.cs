using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class AuthResponse
    {
        public String? AccessToken;
        public String? RefreshToken;
        public Boolean IsSuccess;
        public String? Message;
    }
}
