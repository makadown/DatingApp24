using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;

namespace API.Interfaces
{
    public interface IPhotoRepository
    {
        Task<IEnumerable<PhotoForApprovalDto>> GetUnapprovedPhotos();
        Task<PhotoForApprovalDto> GetPhotoById(int Id);
        Task RemovePhoto(int Id);
        Task ApprovePhoto(int PhotoId);
    }
}