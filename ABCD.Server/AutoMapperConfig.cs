using ABCD.Server.Requests;
using ABCD.Application;

using AutoMapper;

public static class AutoMapperConfig {
    public static IMapper Initialize() {
        var config = new MapperConfiguration(cfg => {
            cfg.AddProfile<MappingProfile>();
        });

        return config.CreateMapper();
    }
}

public class MappingProfile : Profile {
    public MappingProfile() {
        CreateMap<RegisterRequest, UserRegistration>();
        CreateMap<SignInRequest, SignInCredentials>();
    }
}