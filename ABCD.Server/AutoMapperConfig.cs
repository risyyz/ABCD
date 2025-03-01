using ABCD.Lib.Auth;
using ABCD.Server.RequestModels;

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
        CreateMap<RegisterRequestModel, UserRegistration>();
        CreateMap<SignInRequestModel, SignInCredentials>();
    }
}