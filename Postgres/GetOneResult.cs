
namespace BreadTh.PersistenceAccessors.Postgres
{
    public struct GetOneResult<T>
    {
        public readonly TryGetOneStatus status;
        public readonly T result;

        public GetOneResult(TryGetOneStatus status, T result) 
        {
            this.status = status;
            this.result = result;
        }

        public static bool operator ==(GetOneResult<T> lhs, GetOneResult<T> rhs) => lhs.Equals(rhs);
        public static bool operator !=(GetOneResult<T> lhs, GetOneResult<T> rhs) => !lhs.Equals(rhs);
        public override bool Equals(object obj) => obj is GetOneResult<T> && Equals((GetOneResult<T>) obj);
        private bool Equals(GetOneResult<T> other) => other.AsTuple().Equals(AsTuple());
        public override int GetHashCode() => AsTuple().GetHashCode();
        private (TryGetOneStatus, T) AsTuple() =>
            (status, result);
    }
}
