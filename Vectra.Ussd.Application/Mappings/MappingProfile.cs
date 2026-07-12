using AutoMapper;
using Vectra.Ussd.Domain.Entities.CoreBanking;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<MockBvnRecord, BvnQueryDto>();
        CreateMap<ServiceAccount, AccountQueryDto>();
        CreateMap<Customer, CustomrQueryDto>();
        CreateMap<NubanQueryDto, NubanInterbankAccounts>();
        CreateMap<DataBundleQueryDto, DataBundle>();
        CreateMap<CardQueryDto, MockATMCard>();
    }
}