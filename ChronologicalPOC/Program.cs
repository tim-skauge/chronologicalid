using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChronologicalPOC
{
    class Program
    {
        static void Main(string[] args)
        {
            // note check IP as shard

            //var ip = Dns.GetHostAddresses()
            var myIp =
                Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

            uint myBigEndian = (uint)myIp.Address;

            Console.WriteLine(myBigEndian.ToString("X8"));

            uint mLittleEndian = ((myBigEndian & 0xFF000000) >> 24) | ((myBigEndian &
 0x00FF0000) >> 8) | ((myBigEndian & 0x0000FF00) << 8) | ((myBigEndian &
 0x000000FF) << 24);

            Console.WriteLine(mLittleEndian.ToString("X8"));
            var shardId = (int)(mLittleEndian & ChronologicalId.ShardIdMask);

            //ulong l = 0xFFFFFFFFFFFFFFFF;
            //var sutMax = new ChronologicalId((long)l);
            //Console.WriteLine(sutMax);

            var sut = new ChronologicalId(DateTime.MaxValue, shardId, 3);
            Console.WriteLine(sut);
            var raw = sut.GetRaw();
            ////DateTime.Now.Ticks
            //var dt = DateTime.Now;
            //var sut2 = new ChronologicalId(dt, 2, 3);
            //Console.WriteLine(ChronologicalId.TimestampMask);
            //Console.WriteLine(sut);
            //Console.WriteLine(sut2);

            var random = new Random(0);
            var list = new List<ChronologicalId>();
            var dt = DateTime.Now;
            
            list.Add(sut);

            for (int i = 0; i < 20; i++)
            {

                list.Add(new ChronologicalId(dt, random.Next((int)ChronologicalId.UniqueIdMask), shardId));
                dt = dt.AddMilliseconds((random.NextDouble()-0.5)* 100000.0);
            }

            foreach (var chronologicalId in list)
            {
                Console.WriteLine(chronologicalId);
            }
            Console.WriteLine("SORTING!!!!!");
            list.Sort();

            foreach (var chronologicalId in list)
            {
                Console.WriteLine(chronologicalId);
            }

            //Debug.Assert(sut2.Timestamp == dt, "Fyyyy føj");
            //Debug.Assert(sut2.UniqueId == 2);
            //Debug.Assert(sut2.ShardId == 3);

            Console.ReadLine();
        }
    }
    
    //[DebuggerDisplay("Timestamp: {Timestamp} UniqueId: {UniqueId} ShardId: {ShardId}")]
    public struct ChronologicalId : IComparable<ChronologicalId>
    {
        private const int ShardIdBits = 8;
        private const int UniqueIdBits = 17;
        private const int TimestampBits = 38;

        private const int UniqueIdShift = ShardIdBits;
        private const int TimestampShift = UniqueIdShift + UniqueIdBits;

        public const long ShardIdMask = (1L << ShardIdBits) - 1;
        public const long UniqueIdMask = (1L << UniqueIdBits) - 1;
        public const long TimestampMask = (1L << TimestampBits) - 1;

        public static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        private readonly long raw;

        public ChronologicalId(long v)
        {
            this.raw = v;
        }

        private ChronologicalId(long timestamp, int uniqueId, int shardId)
        {
            if (timestamp < 0 || timestamp > TimestampMask) throw new ArgumentOutOfRangeException(nameof(timestamp));
            if (uniqueId < 0 || uniqueId > UniqueIdMask) throw new ArgumentOutOfRangeException(nameof(uniqueId));
            if (shardId < 0 || shardId > ShardIdMask) throw new ArgumentOutOfRangeException(nameof(shardId));
            raw = (timestamp << TimestampShift) + (uniqueId << UniqueIdShift) + (shardId);
        }

        public ChronologicalId(DateTime dateTime, int uniqueId, int shardId)
            : this(GetTimestamp(dateTime), uniqueId, shardId)
        {
        }

        private static long GetTimestamp(DateTime dateTime)
        {
            var timeSpan = dateTime.ToUniversalTime() - Epoch;
            return (long) timeSpan.TotalSeconds;
        }

        public int ShardId { get { return (int) (raw & ShardIdMask); } }

        public int UniqueId { get { return (int) ((raw >> UniqueIdShift) & UniqueIdMask); } }
        
        public DateTime Timestamp => (Epoch + TimeSpan.FromSeconds(GetTimestamp())).ToLocalTime();

        private long GetTimestamp()
        {
            return ((raw >> TimestampShift) & TimestampMask);
        }

        public int CompareTo(ChronologicalId other)
        {
            if (raw < other.raw) return -1;
            if (raw > other.raw) return 1;
            return 0;
        }

        public bool Equals(ChronologicalId other)
        {
            return raw == other.raw;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ChronologicalId && Equals((ChronologicalId)obj);
        }

        public override int GetHashCode()
        {
            return raw.GetHashCode();
        }
        
        public override string ToString()
        {
            return $"Timestamp: {Timestamp} UniqueId: {UniqueId} ShardId: {ShardId} -> {raw:X16}";
        }

        public long GetRaw()
        {
            return raw;
        }
    }
}
