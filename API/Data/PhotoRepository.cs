using API.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using AutoMapper;
using System.Linq;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class PhotoRepository : IPhotoRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;

        public PhotoRepository(DataContext context, IMapper mapper,
                        IPhotoService photoService)
        {
            _photoService = photoService;
            _mapper = mapper;
            _context = context;
        }


        public async Task<PhotoForApprovalDto> GetPhotoById(int Id)
        {
            var foto = await _context.Photos
                        .Where(p => p.Id == Id)
                        .ProjectTo<PhotoForApprovalDto>(_mapper.ConfigurationProvider)
                        .FirstOrDefaultAsync();
            return foto;
        }

        public async Task<IEnumerable<PhotoForApprovalDto>> GetUnapprovedPhotos()
        {
            var fotos = await _context.Photos
                        .Include( u => u.AppUser )
                        .Where(p => p.IsApproved == false)
                        .ProjectTo<PhotoForApprovalDto>(_mapper.ConfigurationProvider)
                        .ToListAsync();
            return fotos;
        }

        public async Task RemovePhoto(int Id)
        {
            var foto = await _context.Photos
                        .Where(p => p.Id == Id)
                        .FirstOrDefaultAsync();
            if (foto.PublicId != null)
            {
                var result =
                await _photoService.DeletePhotoAsync(foto.PublicId);
            }

            _context.Photos.Remove(foto);
        }

        public async Task ApprovePhoto(int PhotoId)
        {
            var foto = await _context.Photos
                        .Where(p => p.Id == PhotoId)
                        .FirstOrDefaultAsync();
            foto.IsApproved = true;

            _context.Photos.Update(foto);
        }
    }
}