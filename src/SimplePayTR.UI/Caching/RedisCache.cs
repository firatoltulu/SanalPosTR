using SimplePayTR.UI.Extensions;
using SimplePayTR.UI.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace SimplePayTR.UI.Caching
{
    public class RedisCache : ICache
    {
        private readonly AppConfig _setting;
        private readonly ConnectionMultiplexer _connection = null;
        private readonly IDatabase _database = null;
        private readonly ConfigurationOptions _options;

        public RedisCache(AppConfig setting)
        {
            _setting = setting;
            _connection = ConnectionMultiplexer.Connect(setting.RedisConnectionString);
            _database = _connection.GetDatabase(setting.RedisDatabaseId.HasValue ? Convert.ToInt32(setting.RedisDatabaseId) : 0);

            _options = ConfigurationOptions.Parse(setting.RedisConnectionString);
        }

        public int DefaultExpireDuration
        {
            get
            {
                return _setting.RedisDefaultExpireTimeOut;
            }
        }

        public IEnumerable<T> Get<T>(string[] key)
        {
            var keys = key.Select(i => (RedisKey)i).ToArray();
            var getValues = _database.StringGet(keys);
            IEnumerable<T> arrange = new List<T>();

            getValues.ToList().ForEach(itm =>
            {
                if (itm.HasValue)
                {
                    arrange.Concat(((string)itm).FromJson<IEnumerable<T>>());
                }
            });

            return arrange;
        }

        public T Get<T>(string key, Func<T> item)
        {
            return Get<T>(key, item, DefaultExpireDuration);
        }

        public T Get<T>(string key, Func<T> item, int expire)
        {
            return Get<T>(key, item, () => expire);
        }

        public T Get<T>(string key, Func<T> item, Func<int> expire)
        {
            T result = default(T);
            if (Exists(key))
            {
                string getValue = _database.StringGet(getKey(key));
                result = getValue.FromJson<T>();
            }
            else
            {
                result = item();
                Set<T>(key, result, expire());
            }

            return result;
        }

        public List<string> Keys()
        {
            var _server = _connection.GetServer(_connection.Configuration);
            var _keys = _server.Keys();
            return _keys.Select(i => i.ToString()).ToList();
        }

        public void Set<T>(string key, T item)
        {
            Set<T>(key, item, DefaultExpireDuration);
        }

        public void Set<T>(string key, T item, int expire)
        {
            if (expire > 0)
                _database.StringSet(getKey(key), item.ToJson(), TimeSpan.FromMinutes(expire));
            else
                _database.StringSet(getKey(key), item.ToJson());
        }

        public bool Exists(string key)
        {
            var exists = _database.KeyExists(getKey(key));
            return exists;
        }

        public T Get<T>(string key)
        {
            T result = default(T);

            if (Exists(key))
            {
                string getValue = _database.StringGet(getKey(key));
                result = getValue.FromJson<T>();
            }
            return result;
        }

        private string getKey(string key)
        {
            if (string.IsNullOrEmpty(_setting.RedisPrefix) == false && key.StartsWith(_setting.RedisPrefix))
                return string.Format("{0}:{1}", _setting.RedisPrefix, key);
            else
                return key;
        }

        public void Delete(string key)
        {
            _database.KeyDelete(getKey(key));
        }

        public void RemoveByPrefix(string prefix)
        {
            foreach (var endPoint in _connection.GetEndPoints())
            {
                var keys = GetKeys(endPoint, getKey(prefix));
                _database.KeyDelete(keys.ToArray());
            }
        }

        protected virtual IEnumerable<RedisKey> GetKeys(EndPoint endPoint, string prefix = null)
        {
            var server = _connection.GetServer(endPoint);

            var keys = server.Keys(_database.Database, string.IsNullOrEmpty(prefix) ? null : $"{prefix}*");

            //we should always persist the data protection key list
            //         keys = keys.Where(key => !key.ToStr);

            return keys;
        }
    }
}