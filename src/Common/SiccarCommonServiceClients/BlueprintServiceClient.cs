﻿/*
* Copyright (c) 2024 Siccar (Registered co. Wallet.Services (Scotland) Ltd).
* All rights reserved.
*
* This file is part of a proprietary software product developed by Siccar.
*
* This source code is licensed under the Siccar Proprietary Limited Use License.
* Use, modification, and distribution of this software is subject to the terms
* and conditions of the license agreement. The full text of the license can be
* found in the LICENSE file or at https://github.com/siccar/SICCARV3/blob/main/LICENCE.txt.
*
* Unauthorized use, copying, modification, merger, publication, distribution,
* sublicensing, and/or sale of this software or any part thereof is strictly
* prohibited except as explicitly allowed by the license agreement.
*/

using Siccar.Application;
using Siccar.Platform;
using System.Text.Json;
#nullable enable

namespace Siccar.Common.ServiceClients
{
	public class BlueprintServiceClient : IBlueprintServiceClient
	{
		private readonly SiccarBaseClient _baseClient;
		private readonly string _endpoint = "blueprints";

		public BlueprintServiceClient(SiccarBaseClient baseClient)
		{
			_baseClient = baseClient;
		}
		public async Task<Blueprint> GetBlueprintDraft(string blueprintId)
		{
			var callPath = $"{_endpoint}/{blueprintId}";
			var response = await _baseClient.GetJsonAsync(callPath);
			return JsonSerializer.Deserialize<Blueprint>(response, _baseClient.serializerOptions)!;
		}
		public async Task<Blueprint> CreateBlueprintDraft(Blueprint blueprint)
		{
			var callPath = $"{_endpoint}";
			var payload = JsonSerializer.Serialize(blueprint, _baseClient.serializerOptions);
			var response = await _baseClient.PostJsonAsync(callPath, payload);
			return JsonSerializer.Deserialize<Blueprint>(response, _baseClient.serializerOptions)!;
		}
		public async Task<Blueprint> UpdateBlueprintDraft(Blueprint blueprint)
		{
			var callPath = $"{_endpoint}/{blueprint.Id}";
			var payload = JsonSerializer.Serialize(blueprint, _baseClient.serializerOptions);
			var response = await _baseClient.PutJsonAsync(callPath, payload);
			var contentString = await response.Content.ReadAsStringAsync();
			return JsonSerializer.Deserialize<Blueprint>(contentString, _baseClient.serializerOptions)!;
		}
		public async Task<TransactionModel> PublishBlueprint(string walletAddress, string registerId, Blueprint blueprint)
		{
			var callPath = $"{_endpoint}/{walletAddress}/{registerId}/publish";
			var payload = JsonSerializer.Serialize(blueprint, _baseClient.serializerOptions);
			var response = await _baseClient.PostJsonAsync(callPath, payload);
			return JsonSerializer.Deserialize<TransactionModel>(response, _baseClient.serializerOptions)!;
		}
		public async Task<List<Blueprint>> GetAllPublished(string walletAddress, string registerId)
		{
			var callPath = $"{_endpoint}/{registerId}/published?wallet-address={walletAddress}";
			var response = await _baseClient.GetJsonAsync(callPath);
			return JsonSerializer.Deserialize<List<Blueprint>>(response, _baseClient.serializerOptions)!;
		}
		public async Task<List<Blueprint>> GetAllPublished(string registerId)
		{
			var callPath = $"{_endpoint}/{registerId}/published";
			var response = await _baseClient.GetJsonAsync(callPath);
			return JsonSerializer.Deserialize<List<Blueprint>>(response, _baseClient.serializerOptions)!;
		}
		public async Task<List<Blueprint>> GetAllDraft()
		{
			var callPath = $"{_endpoint}";
			var response = await _baseClient.GetJsonAsync(callPath);
			return JsonSerializer.Deserialize<List<Blueprint>>(response, _baseClient.serializerOptions)!;
		}
		public async Task<Blueprint> GetPublished(string walletAddress, string registerId, string blueprintId)
		{
			var callPath = $"{_endpoint}/{registerId}/{blueprintId}/published?wallet-address={walletAddress}";
			var response = await _baseClient.GetJsonAsync(callPath);
			return response.Deserialize<Blueprint>(_baseClient.serializerOptions)!;
		}
		public async Task<Blueprint> GetPublished(string registerId, string blueprintId)
		{
			var callPath = $"{_endpoint}/{registerId}/{blueprintId}/published";
			var response = await _baseClient.GetJsonAsync(callPath);
			return response.Deserialize<Blueprint>(_baseClient.serializerOptions)!;
		}
	}
}
