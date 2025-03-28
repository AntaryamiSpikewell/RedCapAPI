using RedcapApiProject.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RedcapApiProject.Services
{
    public interface IRedcapService
    {
        Task<string> GetSurveyLinkAsync(string instrument, RedcapRecord record);
        Task<List<Dictionary<string, object>>> GetAllRecordDataAsync();
        Task<List<Dictionary<string, object>>> GetRecordDataByIdAsync(string recordId);
        Task<int> GetInstanceCountAsync(string recordId, string instrument);
    }
}
