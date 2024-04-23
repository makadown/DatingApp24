using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces
{
    public interface IUserRepository
    {
        void Update(AppUser user);
        Task<IEnumerable<AppUser>> GetUsersAsync();
        Task<AppUser> GetUserByIdAsync(int id);
        Task<AppUser> GetUserByUsernameAsync(string username);
        Task<AppUser> GetUserByPhotoIdAsync(int photoId);
        /// <summary>
        /// Método que invoca los usuarios mapeando
        /// directamente al DTO y evitar la fatiga de usar automapper
        /// en controlador
        /// </summary>
        /// <returns></returns>
        Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams);
        /// <summary>
        /// Método que obtiene usuario por nombre mapenado directo a DTO
        /// y evitar la fatiga de usar automapper en controller
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        Task<MemberDto> GetMemberAsync(string username, bool onlyApproved);
        Task<string> GetUserGender(string username);
    }
}