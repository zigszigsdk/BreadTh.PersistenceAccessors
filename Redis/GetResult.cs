using System.Collections.Generic;

namespace BreadTh.PersistenceAccessors.Redis
{
    public enum TryGetStatus { Undefined, Ok, Empty, DataIsNotValidJson }
    public readonly struct GetResult<T>
    {
        public readonly TryGetStatus status;
        public readonly T result;

        public GetResult(TryGetStatus status, T result)
        {
            this.status = status;
            this.result = result;
        }

        public static bool operator ==(GetResult<T> lhs, GetResult<T> rhs) => lhs.Equals(rhs);
        public static bool operator !=(GetResult<T> lhs, GetResult<T> rhs) => !lhs.Equals(rhs);
        public override bool Equals(object obj) => obj is GetResult<T> && Equals((GetResult<T>) obj);
        private bool Equals(GetResult<T> other) => other.AsTuple().Equals(AsTuple());
        public override int GetHashCode() => AsTuple().GetHashCode();
        private (TryGetStatus, T) AsTuple() =>
            (status, result);
    }
}
