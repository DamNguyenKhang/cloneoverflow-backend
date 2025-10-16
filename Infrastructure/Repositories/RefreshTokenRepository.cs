using Domain;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.AppDbContext;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{

    public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
    {

        public RefreshTokenRepository(ApplicationDbContext context) : base(context) { }


    }
}
