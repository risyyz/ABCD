namespace ABCD.Domain {
    public abstract class EntityId<T> : IEquatable<EntityId<T>>
    where T : struct {
        public T Value { get; }

        protected EntityId(T value) {
            if (EqualityComparer<T>.Default.Equals(value, default))
                throw new ArgumentException("Id value cannot be default.", nameof(value));
            Value = value;
        }

        public bool Equals(EntityId<T>? other) => other != null && Value.Equals(other.Value);
        public override bool Equals(object? obj) => obj is EntityId<T> other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();
    }

    public sealed class BlogId : EntityId<int> {
        public BlogId(int value) : base(value) { }
    }

    public sealed class PostId : EntityId<int> {
        public PostId(int value) : base(value) { }
    }

}
