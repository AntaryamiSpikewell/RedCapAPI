using Microsoft.AspNetCore.Mvc;
using RedcapApiProject.Models;
using RedcapApiProject.Services;
using System.Threading.Tasks;

[ApiController]
[Route("api/redcap")]
public class RedcapController : ControllerBase
{
    private readonly IRedcapService _service;

    public RedcapController(IRedcapService service)
    {
        _service = service;
    }

    [HttpGet("get-survey-link")]
    public async Task<IActionResult> GetSurveyLink([FromQuery] string instrument, [FromQuery] RedcapRecord record)
    {
        // Users can select only one instrument (patient_data or treatment_data).

        if (string.IsNullOrWhiteSpace(instrument))
        {
            return BadRequest(new { error = "Instrument is required. Choose one from 'patient_data' or 'treatment_data'." });
        }
        if (record == null || string.IsNullOrWhiteSpace(record.Id))
        {
            return BadRequest(new { error = "Valid Record ID is required." });
        }

        // Validate instrument selection
        var validInstruments = new List<string> { "patient_data", "treatment_data" };
        if (!validInstruments.Contains(instrument))
        {
            return BadRequest(new { error = "Invalid instrument. Choose only 'patient_data' or 'treatment_data'." });
        }

        // Fetch survey link using GetSurveyLinkByInstrumentAsync
        string surveyLink = await _service.GetSurveyLinkAsync(instrument, record);

        if (string.IsNullOrEmpty(surveyLink))
        {
            return NotFound(new { error = "No survey link found for the provided instrument and record ID." });
        }

        return Ok(new { Instrument = instrument, RecordID = record.Id, Link = surveyLink });
    }



    [HttpGet("get-record-data")]
    public async Task<IActionResult> GetRecordData()
    {
        var response = await _service.GetAllRecordDataAsync();
        return Ok(response);
    }

    [HttpGet("get-record-data-by-id")]
    public async Task<IActionResult> GetRecordDataById([FromQuery] string recordId)
    {
        var response = await _service.GetRecordDataByIdAsync(recordId);
        return Ok(response);
    }

    //[HttpGet("get-instance-count")]
    //public async Task<IActionResult> GetInstanceCount([FromQuery] string recordId, string instrument)
    //{
    //    var response = await _service.GetInstanceCountAsync(recordId);
    //    return Ok(new { recordId, instanceCount = response });
    //}
}
