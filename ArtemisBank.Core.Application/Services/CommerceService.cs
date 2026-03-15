using AutoMapper;
using ArtemisBank.Core.Application.DTOs;
using ArtemisBank.Core.Application.DTOs.Commerce;
using ArtemisBank.Core.Application.Interfaces.IServices;
using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Domain.Interfaces;

namespace ArtemisBank.Core.Application.Services
{
    public class CommerceService : ICommerceService
    {
        private readonly ICommerceRepository _repo;
        private readonly IMapper _mapper;

        public CommerceService(ICommerceRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<CommerceDto> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            return _mapper.Map<CommerceDto>(entity);
        }

        public async Task<PaginatedResult<CommerceDto>> GetAllPagedAsync(int page, int pageSize = 20)
        {
            var entities = await _repo.GetAllPagedAsync(page, pageSize);
            var items = _mapper.Map<IEnumerable<CommerceDto>>(entities);
            var all = await _repo.GetAllAsync();
            var totalCount = all.Count();

            return new PaginatedResult<CommerceDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task AddAsync(CommerceDto dto)
        {
            var entity = _mapper.Map<Commerce>(dto);
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            await _repo.AddAsync(entity);
        }

        public async Task UpdateAsync(CommerceDto dto)
        {
            var entity = await _repo.GetByIdAsync(dto.Id);
            if (entity == null) return;

            entity.Name = dto.Name;
            entity.Description = dto.Description;
            entity.Logo = dto.Logo;
            entity.IsActive = dto.IsActive;
            await _repo.UpdateAsync(entity);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity != null)
            {
                await _repo.DeleteAsync(entity);
            }
        }

        public async Task<bool> CommerceHasActiveUserAsync(int commerceId)
        {
            return await _repo.CommerceHasActiveUserAsync(commerceId);
        }
    }
}
