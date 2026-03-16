using AutoMapper;
using ArtemisBank.Core.Application.DTOs.CreditCardConsumption;
using ArtemisBank.Core.Application.Interfaces.IServices;
using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Domain.Interfaces;

namespace ArtemisBank.Core.Application.Interfaces.Services
{
    public class CreditCardConsumptionService : ICreditCardConsumptionService
    {
        private readonly ICreditCardConsumptionRepository _repo;
        private readonly IMapper _mapper;

        public CreditCardConsumptionService(ICreditCardConsumptionRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<CreditCardConsumptionDto> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            return _mapper.Map<CreditCardConsumptionDto>(entity);
        }

        public async Task<IEnumerable<CreditCardConsumptionDto>> GetByCardIdAsync(int creditCardId)
        {
            var entities = await _repo.GetByCardIdAsync(creditCardId);
            return _mapper.Map<IEnumerable<CreditCardConsumptionDto>>(entities);
        }

        public async Task AddAsync(CreditCardConsumptionDto dto)
        {
            var entity = _mapper.Map<CreditCardConsumption>(dto);
            entity.TransactionDate = DateTime.UtcNow;
            await _repo.AddAsync(entity);
        }
    }
}
