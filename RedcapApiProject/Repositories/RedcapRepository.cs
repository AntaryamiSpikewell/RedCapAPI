using Newtonsoft.Json;
using Redcap;
using Redcap.Models;
using RedcapApiProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace RedcapApiProject.Repositories
{
    public class RedcapRepository : IRedcapRepository
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;
        private readonly string _apiToken;
        private readonly RedcapApi _redcapApi;

        public RedcapRepository(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiUrl = configuration["Redcap:ApiUrl"];
            _apiToken = configuration["Redcap:ApiToken"];
            _redcapApi = new RedcapApi(_apiUrl);
        }

        public async Task<List<Dictionary<string, object>>> GetAllRecordDataAsync()
        {
            var requestData = new Dictionary<string, string>
            {
                { "token", _apiToken },
                { "content", "record" },
                { "format", "json" },
                { "type", "flat" },
                { "returnFormat", "json" }
            };

            var response = await _httpClient.PostAsync(_apiUrl, new FormUrlEncodedContent(requestData));
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(responseContent);
        }

        public async Task<List<Dictionary<string, object>>> GetRecordDataByIdAsync(string recordId)
        {
            var allData = await GetAllRecordDataAsync();
            return allData.Where(r => r.ContainsKey("record_id") && r["record_id"].ToString() == recordId).ToList();
        }

        public async Task<int> GetInstanceCountAsync(string recordId, string instrument)
        {
            var allData = await GetAllRecordDataAsync();
            return allData.Count(r =>
                r.ContainsKey("record_id") && r["record_id"].ToString() == recordId &&
                r.ContainsKey("redcap_repeat_instrument") && r["redcap_repeat_instrument"].ToString() == instrument
            );
        }

        public async Task<string> GenerateNextRecordNameAsync()
        {
            return await _redcapApi.GenerateNextRecordNameAsync(_apiToken);
        }

        public async Task<bool> CheckIfRecordExists(string recordId)
        {
            var response = await _redcapApi.ExportRecordsAsync(
                token: _apiToken,
                format: RedcapFormat.json,
                redcapDataType: RedcapDataType.flat,
                records: new string[] { recordId },
                fields: new string[] { "record_id" },
                returnFormat: RedcapReturnFormat.json
            );
            return !string.IsNullOrEmpty(response) && response != "[]";
        }

        public async Task<string> GetSurveyLinkByInstrumentAsync(string instrument, RedcapRecord record)
        {
            if (string.IsNullOrWhiteSpace(record.Id))
            {
                record.Id = await GenerateNextRecordNameAsync();
            }

            bool recordExists = await CheckIfRecordExists(record.Id);

            int instanceCount = await GetInstanceCountAsync(record.Id, instrument);
            int newInstance = (instanceCount == 0) ? 1 : instanceCount + 1;

            // Fetch only the records of the given instrument
            var previousData = (await GetRecordDataByIdAsync(record.Id))
                .Where(r => r.ContainsKey("redcap_repeat_instrument") && r["redcap_repeat_instrument"].ToString() == instrument)
                .ToList();

            if (!recordExists && !previousData.Any())
            {
                // Create only if there is no previous data
                bool created = await ImportRecordsAsync(new Dictionary<string, object>
                {
                    { "record_id", record.Id },
                    { "redcap_repeat_instrument", instrument }
                });

                if (!created)
                {
                    return JsonConvert.SerializeObject(new { error = "Failed to create record in REDCap" });
                }
            }

            if (previousData.Any())
            {
                var latestInstanceData = new Dictionary<string, object>(previousData.Last());

                // Remove old instance number before adding the new one
                latestInstanceData.Remove("redcap_repeat_instance");

                latestInstanceData["redcap_repeat_instance"] = newInstance;
                latestInstanceData["redcap_repeat_instrument"] = instrument;

                bool importSuccess = await ImportRecordsAsync(latestInstanceData);
                if (!importSuccess)
                {
                    return JsonConvert.SerializeObject(new { error = "Failed to copy previous instance data" });
                }
            }

            var requestData = new Dictionary<string, string>
            {
                { "token", _apiToken },
                { "content", "surveyLink" },
                { "record", record.Id },
                { "instrument", instrument },
                { "repeat_instance", newInstance.ToString() },
                { "format", "json" }
            };

            var response = await _httpClient.PostAsync(_apiUrl, new FormUrlEncodedContent(requestData));
            string responseString = await response.Content.ReadAsStringAsync();

            // Debugging: Log response if needed
            if (string.IsNullOrEmpty(responseString))
            {
                return JsonConvert.SerializeObject(new { error = "Empty response from REDCap API" });
            }

            return responseString;
        }


        public async Task<bool> ImportRecordsAsync(Dictionary<string, object> recordData)
        {
            var recordsList = new List<Dictionary<string, object>> { recordData };

            var response = await _redcapApi.ImportRecordsAsync(
                token: _apiToken,
                content: Content.Record,
                format: RedcapFormat.json,
                redcapDataType: RedcapDataType.flat,
                overwriteBehavior: OverwriteBehavior.normal,
                forceAutoNumber: false,
                backgroundProcess: false,
                data: recordsList,
                returnContent: ReturnContent.count,
                returnFormat: RedcapReturnFormat.json,
                cancellationToken: CancellationToken.None,
                timeOutSeconds: 100
            );
            return response.Contains("1");
        }
    }
}
