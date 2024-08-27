﻿using System.Text.Json;
using Json.More;
using Siccar.Platform;

namespace Siccar.Common.ServiceClients
{
    public class UserServiceClient : IUserServiceClient 
    {
        private readonly SiccarBaseClient _baseClient;
        private const string _usersUri = "users";
        private readonly JsonSerializerOptions JsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

        public UserServiceClient(SiccarBaseClient baseClient)
        {
            _baseClient = baseClient;
        }

        public async Task<HttpResponseMessage> Delete(Guid id)
        {
            return await _baseClient.Delete($"{_usersUri}/{id}");
        }

        public async Task<JsonDocument> Get(Guid id)
        {
            return await _baseClient.GetJsonAsync($"{_usersUri}/{id}");
        }

        public async Task<HttpResponseMessage> AddToRole(Guid id, string role)
        {
            var user = await Get(id);
            var existingUser = user.Deserialize<User>(JsonSerializerOptions);
            existingUser?.Roles?.Add(role);
            return await _baseClient.PutJsonAsync($"{_usersUri}/{id}", existingUser.ToJsonDocument());
        }

        public async Task<HttpResponseMessage> RemoveFromRole(Guid id, string role)
        {
            var user = await Get(id);
            var existingUser = user.Deserialize<User>(JsonSerializerOptions);
            existingUser?.Roles?.Remove(role);
            return await _baseClient.PutJsonAsync($"{_usersUri}/{id}", existingUser.ToJsonDocument());
        }

        public async Task<List<User>> All()
        {
            var URI = $"{_usersUri}";
            var response = await _baseClient.GetJsonAsync(URI);
            var responseData = response.Deserialize<List<User>>(_baseClient.serializerOptions);
            return responseData!;
        }
    }
}
