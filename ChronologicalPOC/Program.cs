using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace ChronologicalPOC
{
    class Program
    {
        static void Main(string[] args)
        {
            var myIp = Dns.GetHostEntry(Dns.GetHostName()).AddressList
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

            var shardId = (int)((myIp?.GetAddressBytes().GetHashCode() & ChronologicalId.ShardIdMask ?? 0));

            var sut1 = new ChronologicalId(DateTime.Now.Add(TimeSpan.FromHours(-1)), 1, shardId);
            var sut2 = new ChronologicalId(DateTime.Now, 1, shardId);
            var sut3 = new ChronologicalId(DateTime.Now, 100, shardId);
            var manySuts = Enumerable.Range(0, 10000).Select(i => ChronologicalId.NewId(shardId)).ToArray();

            Console.WriteLine(sut1);
            Console.WriteLine(sut2);
            Console.WriteLine(sut3);

            foreach (var sut in manySuts)
                Console.WriteLine(sut);

            Console.ReadLine();
        }
    }
}
