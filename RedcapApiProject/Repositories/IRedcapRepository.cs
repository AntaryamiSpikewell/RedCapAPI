using RedcapApiProject.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RedcapApiProject.Repositories
{
    public interface IRedcapRepository
    {
        Task<List<Dictionary<string, object>>> GetAllRecordDataAsync();
        Task<List<Dictionary<string, object>>> GetRecordDataByIdAsync(string recordId);
        Task<int> GetInstanceCountAsync(string recordId, string instrument);
        Task<string> GetSurveyLinkByInstrumentAsync(string instrument, RedcapRecord record);
        Task<string> GenerateNextRecordNameAsync();
        Task<bool> CheckIfRecordExists(string recordId);
        Task<bool> ImportRecordsAsync(Dictionary<string, object> recordData);
    }
}
