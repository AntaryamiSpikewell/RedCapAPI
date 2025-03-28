using RedcapApiProject.Models;
using RedcapApiProject.Repositories;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Threading.Tasks;

namespace RedcapApiProject.Services
{
    public class RedcapService : IRedcapService
    {
        private readonly IRedcapRepository _repository;

        public RedcapService(IRedcapRepository repository)
        {
            _repository = repository;
        }

        public Task<string> GetSurveyLinkAsync(string instrument, RedcapRecord record)
        {
            return _repository.GetSurveyLinkByInstrumentAsync(instrument, record);
        }

        public Task<List<Dictionary<string, object>>> GetAllRecordDataAsync()
        {
            return _repository.GetAllRecordDataAsync();
        }

        public Task<List<Dictionary<string, object>>> GetRecordDataByIdAsync(string recordId)
        {
            return _repository.GetRecordDataByIdAsync(recordId);
        }

        public Task<int> GetInstanceCountAsync(string recordId, string instrument)
        {
            return _repository.GetInstanceCountAsync(recordId,instrument);
        }
    }
}
