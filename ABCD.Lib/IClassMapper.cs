namespace ABCD.Lib {
    public interface IClassMapper {
        public Tout Map<Tin, Tout>(Tin source);
    }
}
