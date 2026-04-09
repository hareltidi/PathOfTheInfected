using System;
using System.Collections.Generic;
using UnityEngine;

namespace TidiGC
{
    public static class GCArtificialMemoryPressure
    {
        public static List<byte[]> Allocated1KbChunks = new List<byte[]>();

        public static long AllocatedBytes => Allocated1KbChunks.Count * 1024;

        public static long FreeBytesCount => (long)TidiGCManager.HeapUsageThreshold * 1024L /*0x0400*/ * 1024L /*0x0400*/ -
                                             TidiGCManager.GetMonoHeapUsage();

        public static long MaxUsageThreshold =>
            (long)TidiGCManager.HeapUsageThreshold * 1024L /*0x0400*/ * 1024L /*0x0400*/;

        public static void ClearAllocatedMemory()
        {
            Allocated1KbChunks.Clear();
        }

        public static void IncreaseGCPressure(float t)
        {
            if (t < 0.0 || t > 1.0)
            {
                throw new InvalidOperationException("Invalid argument");
            }
            long bytesToThreshold = GetBytesToThreshold();
            if (bytesToThreshold < 0L)
            {
                Debug.LogWarning((object)"Memory usage exceeds the threshold.");
            }
            else
            {
                Allocate((long)(bytesToThreshold * (double)t));
            }
        }

        public static void DecreaseGCPressure(float t)
        {
            if (t < 0.0 || t > 1.0)
            {
                throw new InvalidOperationException("Invalid argument");
            }
            Free((long)(AllocatedBytes * (double)t));
        }

        public static long GetBytesToThreshold()
        {
            return (long)TidiGCManager.HeapUsageThreshold * 1024L /*0x0400*/ * 1024L /*0x0400*/ -
                   TidiGCManager.GetMonoHeapUsage();
        }

        public static void Free(long bytesCount)
        {
            bytesCount -= bytesCount % 1024L /*0x0400*/;
            var count = bytesCount / 1024L /*0x0400*/;
            if (count > (long)Allocated1KbChunks.Count)
            {
                Allocated1KbChunks.Clear();
            }
            else
            {
                Allocated1KbChunks.RemoveRange(0, (int)count);
            }
        }

        public static void Allocate(long bytesCount)
        {
            bytesCount -= bytesCount % 1024L /*0x0400*/;
            var num = bytesCount / 1024L /*0x0400*/;
            for (var index = 0; index < num; ++index)
            {
                Allocate1Kb();
            }
        }

        public static void Allocate1Kb()
        {
            var numArray = new byte[1008];
            Allocated1KbChunks.Add(numArray);
        }
    }
}