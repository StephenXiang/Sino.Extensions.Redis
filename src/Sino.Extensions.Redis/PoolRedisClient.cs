﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Sino.Extensions.Redis
{
    public partial class PoolRedisClient : IDisposable
    {
        private EndPoint _endpoint;
        private string _password;

        private readonly ConcurrentQueue<RedisClient> _pool;
        private SemaphoreSlim _semaphore;

        public PoolRedisClient(string host, int port, int max = 100)
            : this(host, port, null, max) { }

        public PoolRedisClient(string host, int port, string password, int max = 100)
        {
            _endpoint = new IPEndPoint(IPAddress.Parse(host), port);
            _password = password;
            _pool = new ConcurrentQueue<RedisClient>();
            _semaphore = new SemaphoreSlim(max, max);
        }

        public T Multi<T>(Func<RedisClient, T> func)
        {
            _semaphore.Wait(30000);

            RedisClient client = null;
            try
            {
                if (!_pool.TryDequeue(out client))
                {
                    client = new RedisClient(_endpoint);
                    if (!string.IsNullOrEmpty(_password))
                        client.Auth(_password);
                }
                var result = func(client);
                _pool.Enqueue(client);
                return result;
            }
            catch (SocketException)
            {
                if (client != null)
                    client.Dispose();
                throw;
            }
            catch (Exception exception)
            {
                throw exception;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public Task<T> MultiAsync<T>(Func<RedisClient, Task<T>> func)
        {
            _semaphore.Wait(30000);

            RedisClient client = null;
            try
            {
                if (!_pool.TryDequeue(out client))
                {
                    client = new RedisClient(_endpoint);
                    if (!string.IsNullOrEmpty(_password))
                        client.Auth(_password);
                }
                var result = func(client);
                _pool.Enqueue(client);
                return result;
            }
            catch (SocketException)
            {
                if (client != null)
                    client.Dispose();
                throw;
            }
            catch (Exception exception)
            {
                throw exception;
            }
            finally
            {
                _semaphore.Release();
            }
        }
        
        #region Connection

        public string Echo(string message)
        {
            return Multi((client) => client.Echo(message));
        }

        public string Ping()
        {
            return Multi((client) => client.Ping());
        }

        public string Quit()
        {
            return Multi((client) => client.Quit());
        }

        public string Select(int index)
        {
            return Multi((client) => client.Select(index));
        }

        #endregion
        
        #region Keys

        public long Del(params string[] keys)
        {
            return Multi((client) => client.Del(keys));
        }

        public byte[] Dump(string key)
        {
            return Multi((client) => client.Dump(key));
        }

        public bool Exists(string key)
        {
            return Multi((client) => client.Exists(key));
        }

        public bool Expire(string key, TimeSpan expiration)
        {
            return Multi((client) => client.Expire(key, expiration));
        }

        public bool Expire(string key, int seconds)
        {
            return Multi((client) => client.Expire(key, seconds));
        }

        public bool ExpireAt(string key, DateTime expirationDate)
        {
            return Multi((client) => client.ExpireAt(key, expirationDate));
        }

        public bool ExpireAt(string key, int timestamp)
        {
            return Multi((client) => client.ExpireAt(key, timestamp));
        }

        public string[] Keys(string pattern)
        {
            return Multi((client) => client.Keys(pattern));
        }

        public string Migrate(string host, int port, string key, int destinationDb, int timeoutMilliseconds)
        {
            return Multi((client) => client.Migrate(host, port, key, destinationDb, timeoutMilliseconds));
        }

        public string Migrate(string host, int port, string key, int destinationDb, TimeSpan timeout)
        {
            return Multi((client) => client.Migrate(host, port, key, destinationDb, timeout));
        }

        public bool Move(string key, int database)
        {
            return Multi((client) => client.Move(key, database));
        }

        public string ObjectEncoding(params string[] arguments)
        {
            return Multi((client) => client.ObjectEncoding(arguments));
        }

        public long? Object(RedisObjectSubCommand subCommand, params string[] arguments)
        {
            return Multi((client) => client.Object(subCommand, arguments));
        }

        public bool Persist(string key)
        {
            return Multi((client) => client.Persist(key));
        }

        public bool PExpire(string key, TimeSpan expiration)
        {
            return Multi((client) => client.PExpire(key, expiration));
        }

        public bool PExpire(string key, long milliseconds)
        {
            return Multi((client) => client.PExpire(key, milliseconds));
        }

        public bool PExpireAt(string key, DateTime date)
        {
            return Multi((client) => client.PExpireAt(key, date));
        }

        public bool PExpireAt(string key, long timestamp)
        {
            return Multi((client) => client.PExpireAt(key, timestamp));
        }

        public long PTtl(string key)
        {
            return Multi((client) => client.PTtl(key));
        }

        public string RandomKey()
        {
            return Multi((client) => client.RandomKey());
        }

        public string Rename(string key, string newKey)
        {
            return Multi((client) => client.Rename(key, newKey));
        }

        public bool RenameNx(string key, string newKey)
        {
            return Multi((client) => client.RenameNx(key, newKey));
        }

        public string Restore(string key, long ttl, string serializedValue)
        {
            return Multi((client) => client.Restore(key, ttl, serializedValue));
        }

        public string[] Sort(string key, long? offset = null, long? count = null, string by = null, RedisSortDir? dir = null, bool? isAlpha = null, params string[] get)
        {
            return Multi((client) => client.Sort(key, offset, count, by, dir, isAlpha, get));
        }

        public long SortAndStore(string key, string destination, long? offset = null, long? count = null, string by = null, RedisSortDir? dir = null, bool? isAlpha = false, params string[] get)
        {
            return Multi((client) => client.SortAndStore(key, destination, offset, count, by, dir, isAlpha, get));
        }

        public long Ttl(string key)
        {
            return Multi((client) => client.Ttl(key));
        }

        public string Type(string key)
        {
            return Multi((client) => client.Type(key));
        }

        public RedisScan<string> Scan(long cursor, string pattern = null, long? count = null)
        {
            return Multi((client) => client.Scan(cursor, pattern, count));
        }

        #endregion

        #region Hashes
        
        public long HDel(string key, params string[] fields)
        {
            return Multi((client) => client.HDel(key, fields));
        }

        public bool HExists(string key, string field)
        {
            return Multi((client) => client.HExists(key, field));
        }

        public string HGet(string key, string field)
        {
            return Multi((client) => client.HGet(key, field));
        }

        public Dictionary<string, string> HGetAll(string key)
        {
            return Multi((client) => client.HGetAll(key));
        }

        public long HIncrBy(string key, string field, long increment)
        {
            return Multi((client) => client.HIncrBy(key, field, increment));
        }

        public double HIncrByFloat(string key, string field, double increment)
        {
            return Multi((client) => client.HIncrByFloat(key, field, increment));
        }

        public string[] HKeys(string key)
        {
            return Multi((client) => client.HKeys(key));
        }

        public long HLen(string key)
        {
            return Multi((client) => client.HLen(key));
        }

        public string[] HMGet(string key, params string[] fields)
        {
            return Multi((client) => client.HMGet(key, fields));
        }

        public string HMSet(string key, Dictionary<string, string> dict)
        {
            return Multi((client) => client.HMSet(key, dict));
        }

        public string HMSet(string key, params string[] keyValues)
        {
            return Multi((client) => client.HMSet(key, keyValues));
        }

        public bool HSet(string key, string field, object value)
        {
            return Multi((client) => client.HSet(key, field, value));
        }

        public bool HSetNx(string key, string field, object value)
        {
            return Multi((client) => client.HSetNx(key, field, value));
        }

        public string[] HVals(string key)
        {
            return Multi((client) => client.HVals(key));
        }

        public RedisScan<Tuple<string, string>> HScan(string key, long cursor, string pattern = null, long? count = null)
        {
            return Multi((client) => client.HScan(key, cursor, pattern, count));
        }

        #endregion

        #region Lists
        
        public Tuple<string, string> BLPopWithKey(int timeout, params string[] keys)
        {
            return Multi((client) => client.BLPopWithKey(timeout, keys));
        }

        public Tuple<string, string> BLPopWithKey(TimeSpan timeout, params string[] keys)
        {
            return Multi((client) => client.BLPopWithKey(timeout, keys));
        }

        public string BLPop(int timeout, params string[] keys)
        {
            return Multi((client) => client.BLPop(timeout, keys));
        }

        public string BLPop(TimeSpan timeout, params string[] keys)
        {
            return Multi((client) => client.BLPop(timeout, keys));
        }

        public Tuple<string, string> BRPopWithKey(int timeout, params string[] keys)
        {
            return Multi((client) => client.BRPopWithKey(timeout, keys));
        }

        public Tuple<string, string> BRPopWithKey(TimeSpan timeout, params string[] keys)
        {
            return Multi((client) => client.BRPopWithKey(timeout, keys));
        }

        public string BRPop(int timeout, params string[] keys)
        {
            return Multi((client) => client.BRPop(timeout, keys));
        }

        public string BRPop(TimeSpan timeout, params string[] keys)
        {
            return Multi((client) => client.BRPop(timeout, keys));
        }

        public string BRPopLPush(string source, string destination, int timeout)
        {
            return Multi((client) => client.BRPopLPush(source, destination, timeout));
        }

        public string LIndex(string key, long index)
        {
            return Multi((client) => client.LIndex(key, index));
        }

        public long LInsert(string key, RedisInsert insertType, string pivot, object value)
        {
            return Multi((client) => client.LInsert(key, insertType, pivot, value));
        }

        public long LLen(string key)
        {
            return Multi((client) => client.LLen(key));
        }

        public string LPop(string key)
        {
            return Multi((client) => client.LPop(key));
        }

        public long LPush(string key, params object[] values)
        {
            return Multi((client) => client.LPush(key, values));
        }

        public long LPushX(string key, object value)
        {
            return Multi((client) => client.LPushX(key, value));
        }

        public string[] LRange(string key, long start, long stop)
        {
            return Multi((client) => client.LRange(key, start, stop));
        }

        public long LRem(string key, long count, object value)
        {
            return Multi((client) => client.LRem(key, count, value));
        }

        public string LSet(string key, long index, object value)
        {
            return Multi((client) => client.LSet(key, index, value));
        }

        public string LTrim(string key, long start, long stop)
        {
            return Multi((client) => client.LTrim(key, start, stop));
        }

        public string RPop(string key)
        {
            return Multi((client) => client.RPop(key));
        }

        public string RPopLPush(string source, string destination)
        {
            return Multi((client) => client.RPopLPush(source, destination));
        }

        public long RPush(string key, params object[] values)
        {
            return Multi((client) => client.RPush(key, values));
        }

        public long RPushX(string key, params object[] values)
        {
            return Multi((client) => client.RPushX(key, values));
        }

        #endregion

        #region Sets

        public long SAdd(string key, params object[] members)
        {
            return Multi((client) => client.SAdd(key, members));
        }

        public long SCard(string key)
        {
            return Multi((client) => client.SCard(key));
        }

        public string[] SDiff(params string[] keys)
        {
            return Multi((client) => client.SDiff(keys));
        }

        public long SDiffStore(string destination, params string[] keys)
        {
            return Multi((client) => client.SDiffStore(destination, keys));
        }

        public string[] SInter(params string[] keys)
        {
            return Multi((client) => client.SInter(keys));
        }

        public long SInterStore(string destination, params string[] keys)
        {
            return Multi((client) => client.SInterStore(destination, keys));
        }

        public bool SIsMember(string key, object member)
        {
            return Multi((client) => client.SIsMember(key, member));
        }

        public string[] SMembers(string key)
        {
            return Multi((client) => client.SMembers(key));
        }

        public bool SMove(string source, string destination, object member)
        {
            return Multi((client) => client.SMove(source, destination, member));
        }

        public string SPop(string key)
        {
            return Multi((client) => client.SPop(key));
        }

        public string SRandMember(string key)
        {
            return Multi((client) => client.SRandMember(key));
        }

        public string[] SRandMember(string key, long count)
        {
            return Multi((client) => client.SRandMember(key, count));
        }

        public long SRem(string key, params object[] members)
        {
            return Multi((client) => client.SRem(key, members));
        }

        public string[] SUnion(params string[] keys)
        {
            return Multi((client) => client.SUnion(keys));
        }

        public long SUnionStore(string destination, params string[] keys)
        {
            return Multi((client) => client.SUnionStore(destination, keys));
        }

        public RedisScan<string> SScan(string key, long cursor, string pattern = null, long? count = null)
        {
            return Multi((client) => client.SScan(key, cursor, pattern, count));
        }

        #endregion

        #region Sorted Sets
        
        public long ZAdd<TScore, TMember>(string key, params Tuple<TScore, TMember>[] memberScores)
        {
            return Multi((client) => client.ZAdd(key, memberScores));
        }

        public long ZAdd(string key, params string[] memberScores)
        {
            return Multi((client) => client.ZAdd(key, memberScores));
        }

        public long ZCard(string key)
        {
            return Multi((client) => client.ZCard(key));
        }

        public long ZCount(string key, double min, double max, bool exclusiveMin = false, bool exclusiveMax = false)
        {
            return Multi((client) => client.ZCount(key, min, max, exclusiveMin, exclusiveMax));
        }

        public long ZCount(string key, string min, string max)
        {
            return Multi((client) => client.ZCount(key, min, max));
        }

        public double ZIncrBy(string key, double increment, string member)
        {
            return Multi((client) => client.ZIncrBy(key, increment, member));
        }

        public long ZInterStore(string destination, double[] weights = null, RedisAggregate? aggregate = null, params string[] keys)
        {
            return Multi((client) => client.ZInterStore(destination, weights, aggregate, keys));
        }

        public long ZInterStore(string destination, params string[] keys)
        {
            return Multi((client) => client.ZInterStore(destination, keys));
        }

        public string[] ZRange(string key, long start, long stop, bool withScores = false)
        {
            return Multi((client) => client.ZRange(key, start, stop, withScores));
        }

        public Tuple<string, double>[] ZRangeWithScores(string key, long start, long stop)
        {
            return Multi((client) => client.ZRangeWithScores(key, start, stop));
        }

        public string[] ZRangeByScore(string key, double min, double max, bool withScores = false, bool exclusiveMin = false, bool exclusiveMax = false, long? offset = null, long? count = null)
        {
            return Multi((client) => client.ZRangeByScore(key, min, max, withScores, exclusiveMin, exclusiveMax, offset, count));
        }

        public Tuple<string, double>[] ZRangeByScoreWithScores(string key, double min, double max, bool exclusiveMin = false, bool exclusiveMax = false, long? offset = null, long? count = null)
        {
            return Multi((client) => client.ZRangeByScoreWithScores(key, min, max, exclusiveMin, exclusiveMax, offset, count));
        }

        public Tuple<string, double>[] ZRangeByScoreWithScores(string key, string min, string max, long? offset = null, long? count = null)
        {
            return Multi((client) => client.ZRangeByScoreWithScores(key, min, max, offset, count));
        }

        public long? ZRank(string key, string member)
        {
            return Multi((client) => client.ZRank(key, member));
        }

        public long ZRem(string key, params object[] members)
        {
            return Multi((client) => client.ZRem(key, members));
        }

        public long ZRemRangeByRank(string key, long start, long stop)
        {
            return Multi((client) => client.ZRemRangeByRank(key, start, stop));
        }

        public long ZRemRangeByScore(string key, double min, double max, bool exclusiveMin = false, bool exclusiveMax = false)
        {
            return Multi((client) => client.ZRemRangeByScore(key, min, max, exclusiveMin, exclusiveMax));
        }

        public string[] ZRevRange(string key, long start, long stop, bool withScores = false)
        {
            return Multi((client) => client.ZRevRange(key, start, stop, withScores));
        }

        public Tuple<string, double>[] ZRevRangeWithScores(string key, long start, long stop)
        {
            return Multi((client) => client.ZRevRangeWithScores(key, start, stop));
        }

        public string[] ZRevRangeByScore(string key, double max, double min, bool withScores = false, bool exclusiveMax = false, bool exclusiveMin = false, long? offset = null, long? count = null)
        {
            return Multi((client) => client.ZRevRangeByScore(key, max, min, withScores, exclusiveMax, exclusiveMin, offset, count));
        }

        public string[] ZRevRangeByScore(string key, string max, string min, bool withScores = false, long? offset = null, long? count = null)
        {
            return Multi((client) => client.ZRevRangeByScore(key, max, min, withScores, offset, count));
        }

        public Tuple<string, double>[] ZRevRangeByScoreWithScores(string key, double max, double min, bool exclusiveMax = false, bool exclusiveMin = false, long? offset = null, long? count = null)
        {
            return Multi((client) => client.ZRevRangeByScoreWithScores(key, max, min, exclusiveMax, exclusiveMin, offset, count));
        }

        public Tuple<string, double>[] ZRevRangeByScoreWithScores(string key, string max, string min, long? offset = null, long? count = null)
        {
            return Multi((client) => client.ZRevRangeByScoreWithScores(key, max, min, offset, count));
        }

        public long? ZRevRank(string key, string member)
        {
            return Multi((client) => client.ZRevRank(key, member));
        }

        public double? ZScore(string key, string member)
        {
            return Multi((client) => client.ZScore(key, member));
        }

        public long ZUnionStore(string destination, double[] weights = null, RedisAggregate? aggregate = null, params string[] keys)
        {
            return Multi((client) => client.ZUnionStore(destination, weights, aggregate, keys));
        }

        public long ZUnionStore(string destination, params string[] keys)
        {
            return Multi((client) => client.ZUnionStore(destination, keys));
        }

        public RedisScan<Tuple<string, double>> ZScan(string key, long cursor, string pattern = null, long? count = null)
        {
            return Multi((client) => client.ZScan(key, cursor, pattern, count));
        }

        public string[] ZRangeByLex(string key, string min, string max, long? offset = null, long? count = null)
        {
            return Multi((client) => client.ZRangeByLex(key, min, max, offset, count));
        }

        public long ZRemRangeByLex(string key, string min, string max)
        {
            return Multi((client) => client.ZRemRangeByLex(key, min, max));
        }

        public long ZLexCount(string key, string min, string max)
        {
            return Multi((client) => client.ZLexCount(key, min, max));
        }

        #endregion

        #region Scripting
        
        public object Eval(string script, string[] keys, params string[] arguments)
        {
            return Multi((client) => client.Eval(script, keys, arguments));
        }

        public object EvalSHA(string sha1, string[] keys, params string[] arguments)
        {
            return Multi((client) => client.EvalSHA(sha1, keys, arguments));
        }

        public bool[] ScriptExists(params string[] scripts)
        {
            return Multi((client) => client.ScriptExists(scripts));
        }

        public string ScriptFlush()
        {
            return Multi((client) => client.ScriptFlush());
        }

        public string ScriptKill()
        {
            return Multi((client) => client.ScriptKill());
        }

        public string ScriptLoad(string script)
        {
            return Multi((client) => client.ScriptLoad(script));
        }

        #endregion

        #region Strings

        public long Append(string key, object value) => Multi((client) => client.Append(key, value));

        public long BitCount(string key, long? start = null, long? end = null) => Multi((client) => client.BitCount(key, start, end));

        public long BitOp(RedisBitOp operation, string destKey, params string[] keys) => Multi((client) => client.BitOp(operation, destKey, keys));

        public long BitPos(string key, bool bit, long? start = null, long? end = null) => Multi((client) => client.BitPos(key, bit, start, end));

        public long Decr(string key) => Multi((client) => client.Decr(key));

        public long DecrBy(string key, long decrement) => Multi((client) => client.DecrBy(key, decrement));

        public string Get(string key) => Multi((client) => client.Get(key));

        public bool GetBit(string key, uint offset) => Multi((client) => client.GetBit(key, offset));

        public string GetRange(string key, long start, long end) => Multi((client) => client.GetRange(key, start, end));

        public string GetSet(string key, object value) => Multi((client) => client.GetSet(key, value));

        public long Incr(string key) => Multi((client) => client.Incr(key));

        public long IncrBy(string key, long increment) => Multi((client) => client.IncrBy(key, increment));

        public double IncrByFloat(string key, double increment) => Multi((client) => client.IncrByFloat(key, increment));

        public string[] MGet(params string[] keys) => Multi((client) => client.MGet(keys));

        public string MSet(params Tuple<string, string>[] keyValues) => Multi((client) => client.MSet(keyValues));

        public string MSet(params string[] keyValues) => Multi((client) => client.MSet(keyValues));

        public bool MSetNx(params Tuple<string, string>[] keyValues) => Multi((client) => client.MSetNx(keyValues));

        public bool MSetNx(params string[] keyValues) => Multi((client) => client.MSetNx(keyValues));

        public string PSetEx(string key, long milliseconds, object value) => Multi((client) => client.PSetEx(key, milliseconds, value));

        public string Set(string key, object value) => Multi((client) => client.Set(key, value));

        public string Set(string key, object value, TimeSpan expiration, RedisExistence? condition = null) => Multi((client) => client.Set(key, value, expiration, condition));

        public string Set(string key, object value, int? expirationSeconds = null, RedisExistence? condition = null) => Multi((client) => client.Set(key, value, expirationSeconds, condition));

        public string Set(string key, object value, long? expirationMilliseconds = null, RedisExistence? condition = null) => Multi((client) => client.Set(key, value, expirationMilliseconds, condition));

        public bool SetBit(string key, uint offset, bool value) => Multi((client) => client.SetBit(key, offset, value));

        public string SetEx(string key, long seconds, object value) => Multi((client) => client.SetEx(key, seconds, value));

        public bool SetNx(string key, object value) => Multi((client) => client.SetNx(key, value));

        public long SetRange(string key, uint offset, object value) => Multi((client) => client.SetRange(key, offset, value));

        public long StrLen(string key) => Multi((client) => client.StrLen(key));

        #endregion

        public void Dispose()
        {
            for (int i = 0; i < _pool.Count; i++)
            {
                RedisClient client;
                if (_pool.TryDequeue(out client))
                    client.Dispose();
            }
        }
    }
}
