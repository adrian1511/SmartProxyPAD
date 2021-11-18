﻿using Common.Models;
using System.Net.Http;
using MovieAPI.Settings;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Common.Utilities;

namespace MovieAPI.Services
{
    public class SyncService<T> : ISyncService<T> where T : MongoDocument
    {
        private readonly ISyncServiceSettings _settings;
        private readonly string _origin;
        public SyncService(ISyncServiceSettings settings, IHttpContextAccessor httpContext)
        {
            _settings = settings;
            _origin = httpContext.HttpContext.Request.Host.ToString();
        }
        public HttpResponseMessage Delete(T record)
        {
            var syncType = _settings.DeleteHttpMethod;
            var json = ToSyncEntityJson(record, syncType);

            var response = HttpClientUtility.SendJson(json, _settings.Host, "POST");

            return response;
        }

        public HttpResponseMessage Upsert(T record)
        {
            var syncType = _settings.UpsertHttpMethod;
            var json = ToSyncEntityJson(record, syncType);

            var response = HttpClientUtility.SendJson(json, _settings.Host, "POST");

            return response;
        }

        private string ToSyncEntityJson(T record, string syncType)
        {
            var objectType = typeof(T);

            var syncEntity = new SyncEntity()
            {
                JsonData = JsonSerializer.Serialize(record),
                SyncType = syncType,
                ObjectType = objectType.Name,
                Id = record.Id,
                LastChangeAt = record.LastChangedAt,
                Origin = _origin
            };

            var json = JsonSerializer.Serialize(syncEntity);

            return json;
        }
    }
}
