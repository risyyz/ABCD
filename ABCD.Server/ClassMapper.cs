using ABCD.Lib;

using AutoMapper;

namespace ABCD.Server {
    public class ClassMapper : IClassMapper {
        private readonly IMapper _mapper;
        public ClassMapper(IMapper mapper) {
            _mapper = mapper;
        }
        
        public Tout Map<Tin, Tout>(Tin source) {
            return _mapper.Map<Tin, Tout>(source);
        }
    }
}