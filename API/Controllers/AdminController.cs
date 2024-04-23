using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AdminController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public AdminController(UserManager<AppUser> userManager,
                    IUnitOfWork unitOfWork, IMapper mapper)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUsersWithRoles()
        {
            var users = await _userManager.Users
                .Include(r => r.UserRoles)
                .ThenInclude(r => r.Role)
                .OrderBy(u => u.UserName)
                .Select(u => new
                {
                    u.Id,
                    Username = u.UserName,
                    Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditRoles(string username, [FromQuery] string roles)
        {
            var selectedRoles = roles.Split(",").ToArray();
            // Checo si existe el username
            var user = await _userManager.FindByNameAsync(username);

            if (user == null) return NotFound("Could not find user");

            // Obtengo sus roles actuales
            var userRoles = await _userManager.GetRolesAsync(user);

            // Agrego los nuevos roles enviados, exceptuando los existentes que coincidan
            var result = await _userManager
                .AddToRolesAsync(user, selectedRoles.Except(userRoles));

            if (!result.Succeeded)
            {
                return BadRequest("Failed to add roles");
            }

            // Elimino los roles existentes exceptuando los roles enviados
            result = await _userManager
                .RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

            if (!result.Succeeded)
            {
                return BadRequest("Failed to remove from roles");
            }

            return Ok(await _userManager.GetRolesAsync(user));
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-for-approval")]
        public async Task<ActionResult> GetPhotosForApproval()
        {
            return Ok(await _unitOfWork.PhotoRepository.GetUnapprovedPhotos());
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("approve-photo/{photoId}")]
        public async Task<ActionResult> ApprovePhoto(int PhotoId)
        {
            await _unitOfWork.PhotoRepository.ApprovePhoto(PhotoId);

            /* Checo que si el usuario no tiene foto Main alguna, automaticamente establecer
            como main la foto que se está aprobando. */
            var user = await _unitOfWork.UserRepository
                    .GetUserByPhotoIdAsync(PhotoId);

            var photo = user.Photos.FirstOrDefault(x => x.IsApproved && x.IsMain);

            if (photo == null)
            {
                var currentMain = user.Photos.FirstOrDefault(x => x.Id == PhotoId);
                currentMain.IsMain = true;
            }

            if (await _unitOfWork.Complete())
            {
                return NoContent();
            }

            return BadRequest("Failed to approve photo");
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("reject-photo/{photoId}")]
        public async Task<ActionResult> RejectPhoto(int photoId)
        {
            await _unitOfWork.PhotoRepository.RemovePhoto(photoId);
            if (await _unitOfWork.Complete())
            {
                return NoContent();
            }

            return BadRequest("Failed to reject photo");
        }
    }
}