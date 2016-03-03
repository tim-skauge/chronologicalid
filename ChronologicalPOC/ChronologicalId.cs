using System;
using System.Diagnostics;
using System.Threading;

namespace ChronologicalPOC
{
    [DebuggerDisplay("{Value} -> Timestamp={Timestamp}, UniqueId={UniqueId}, ShardId={ShardId}")]
    public struct ChronologicalId : IComparable<ChronologicalId>
    {
        private static int counter;

        private const int TimestampBits = 38;
        private const int UniqueIdBits = 17;
        private const int ShardIdBits = 8;

        private const int UniqueIdShift = ShardIdBits;
        private const int TimestampShift = UniqueIdShift + UniqueIdBits;

        public const long TimestampMask = (1L << TimestampBits) - 1;
        public const long UniqueIdMask = (1L << UniqueIdBits) - 1;
        public const long ShardIdMask = (1L << ShardIdBits) - 1;

        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public ChronologicalId(long value)
        {
            Value = value;
        }

        public ChronologicalId(DateTime dateTime, int uniqueId, int shardId)
        {
            if (dateTime < epoch)
                throw new ArgumentException($"Must be before Unix Epoch ({epoch}). Given value is {dateTime}", nameof(dateTime));
            if (uniqueId < 0 || uniqueId > UniqueIdMask)
                throw new ArgumentOutOfRangeException(nameof(uniqueId), $"Must be a positive value of max {UniqueIdMask}. Given value is {uniqueId}");
            if (shardId < 0 || shardId > ShardIdMask)
                throw new ArgumentOutOfRangeException(nameof(shardId), $"Must be a positive value of max {ShardIdMask}. Given value is {shardId}");

            var timestamp = (long)(dateTime.ToUniversalTime() - epoch).TotalSeconds;
            Value = (timestamp << TimestampShift) + (uniqueId << UniqueIdShift) + (shardId);
        }

        public static ChronologicalId NewId(int shardId)
        {
            var uniqueId = (int)(Interlocked.Increment(ref counter) % UniqueIdMask);
            return new ChronologicalId(DateTime.UtcNow, uniqueId, shardId);
        }

        public long Value { get; }
        public int ShardId => (int) (Value & ShardIdMask);
        public int UniqueId => (int) ((Value >> UniqueIdShift) & UniqueIdMask);
        public DateTime Timestamp => (epoch + TimeSpan.FromSeconds((Value >> TimestampShift) & TimestampMask)).ToLocalTime();

        public int CompareTo(ChronologicalId other)
        {
            if (Value < other.Value) return -1;
            if (Value > other.Value) return 1;
            return 0;
        }

        public bool Equals(ChronologicalId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ChronologicalId && Equals((ChronologicalId)obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}