using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.AppDbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class InvalidTokenRepository : GenericRepository<InvalidToken>, IInvalidTokenRepository
    {
        public InvalidTokenRepository(ApplicationDbContext context) : base(context) { }

        public async Task InvalidAToken(string token)
        {
            await Add(new InvalidToken
            {
                Token = token
            });
        }
    }
}
