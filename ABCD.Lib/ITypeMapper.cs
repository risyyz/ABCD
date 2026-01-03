using Mapster;

namespace ABCD.Lib {
    public interface ITypeMapper {
        public Tout Map<Tin, Tout>(Tin source);
    }

    public class TypeMapper : ITypeMapper {
        public Tout Map<Tin, Tout>(Tin source) {
            return source.Adapt<Tout>();
        }
    }
}
