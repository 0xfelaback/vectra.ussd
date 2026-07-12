using AutoMapper;
using Microsoft.Extensions.Caching.Distributed;
using Vectra.Ussd.Application.Interfaces;

public class NIBSSIdentityVerificationService : INIBSSIdentityVerificationService
{
    private readonly IISimActionsRepository _repository;
    private readonly IBvnReadRepository _bvnReadRepository;
    private readonly IMapper _mapper;
    private readonly IDistributedCache _cache;
    public NIBSSIdentityVerificationService(IISimActionsRepository repository, IBvnReadRepository bvnReadRepository, IMapper mapper, IDistributedCache cache)
    {
        _repository = repository;
        _bvnReadRepository = bvnReadRepository;
        _mapper = mapper;
        _cache = cache;
    }
    public async Task<bool?> MatchValidation(DateOnly inputDate, CancellationToken token, string phoneNumber)
    {
        MockBvnRecord? bvnRecord = await _bvnReadRepository.QueryBvnByPhoneNumber(phoneNumber, token);
        if (bvnRecord is null) return null;
        BvnQueryDto dto = _mapper.Map<BvnQueryDto>(bvnRecord);
        if (dto.DateOfBirth != inputDate)
        {
            return false;
        }
        else
        {
            return true;
        }

    }
    public async Task<bool> IsSimSwapped(string phoneNumber, CancellationToken token)
    {
        return await _repository.CheckSwap(phoneNumber, token);
    }

    public async Task<bool> IsSimReassigned(string phoneNumber, CancellationToken token)
    {
        return await _repository.CheckReassigned(phoneNumber, token);
    }
}