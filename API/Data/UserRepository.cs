using System.Xml.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public UserRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                    .Include(p => p.Photos)
                    .FirstOrDefaultAsync(x => x.UserName == username);
        }

        public async Task<AppUser> GetUserByPhotoIdAsync(int PhotoId)
        {
            var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == PhotoId);

            return await _context.Users
                    .Include(p => p.Photos)
                    .FirstOrDefaultAsync(x => x.Id == photo.AppUserId);
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await _context.Users
                    .Include(p => p.Photos)
                    .ToListAsync();
        }

        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }
        public async Task<MemberDto> GetMemberAsync(string username, bool onlyApproved)
        {
            /* NOTA! El ProjectTo() nos ahorra el tener que codear el 
            .Include(p => p.Photos) !!! QUE CHILO! */
            var usuario = new MemberDto();
            usuario = await _context.Users
                                   .Where(x => x.UserName == username)
                                   .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                                   .FirstOrDefaultAsync();
            if (onlyApproved)
            {
                usuario.Photos = usuario.Photos.Where( p => p.IsApproved ).ToList();
            }
            return usuario;
        }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            var query = _context.Users.AsQueryable();

            query = query.Where(u => u.UserName != userParams.CurrentUsername);
            query = query.Where(u => u.Gender == userParams.Gender);

            // Ej. Si MinAge = 18 y MaxAge = 150
            // Si hoy es 14 de Feb de 2020
            // La fecha de nacimiento de la gente a buscar debe ser
            // entre 14 de Febrero de (2020-150-1) = 1869 y
            //       14 de Febrero de  (2020-150)  = 2002
            var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
            var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

            query = query.Where(u => u.DateOfBirth >= minDob &&
                       u.DateOfBirth <= maxDob);

            query = userParams.OrderBy switch
            {
                "created" => query.OrderByDescending(u => u.Created),
                _ => query.OrderByDescending(u => u.LastActive) // _ significa default
            };

            var lista = await PagedList<MemberDto>.CreateAsync( 
                query.ProjectTo<MemberDto>(_mapper.ConfigurationProvider).AsNoTracking(),
                userParams.PageNumber, userParams.PageSize);

            foreach(MemberDto elem in lista)
            {
                elem.Photos= elem.Photos.Where( p => p.IsApproved ).ToList();
            }
            return lista;
        }
        /// <summary>
        /// Obtiene genero del usuario.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public async Task<string> GetUserGender(string username)
        {
            return await _context.Users.Where(x => x.UserName.ToLower() == username.ToLower())
                    .Select(x => x.Gender).FirstOrDefaultAsync();
        }
    }
}