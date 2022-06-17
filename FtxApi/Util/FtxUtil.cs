﻿using FtxApi.Models;
using System;
using System.Threading.Tasks;

namespace FtxApi
{
    public static class FtxUtil
    {
        private static DateTime _epochTime = new DateTime(1970, 1, 1, 0, 0, 0);

        public static long GetMillisecondsFromEpochStart()
        {
            return GetMillisecondsFromEpochStart(DateTime.UtcNow);
        }

        public static long GetMillisecondsFromEpochStart(DateTime time)
        {
            return (long)(time - _epochTime).TotalMilliseconds;
        }

        public static long GetSecondsFromEpochStart(DateTime time)
        {
            return (long)(time - _epochTime).TotalSeconds;
        }

        public static async Task<T> GetResult<T>(this Task<FtxResult<T>> item)
        {
            var result = await item;
            if (result.Success)
                return result.Result;
            else if (string.IsNullOrEmpty(result.Error))
                throw new Exception();
            else
                throw new Exception(result.Error);
        }

    }
}