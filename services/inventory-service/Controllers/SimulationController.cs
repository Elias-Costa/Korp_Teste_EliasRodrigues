using InventoryService.Contracts;
using InventoryService.Services;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Controllers;

[ApiController]
[Route("api/simulation")]
public sealed class SimulationController(FailureSimulationState failureSimulationState) : ControllerBase
{
    [HttpGet("failure-mode")]
    public ActionResult<FailureSimulationResponse> GetFailureMode()
    {
        return Ok(new FailureSimulationResponse(failureSimulationState.IsEnabled));
    }

    [HttpPost("failure-mode")]
    public ActionResult<FailureSimulationResponse> UpdateFailureMode(
        [FromBody] FailureSimulationRequest request)
    {
        var enabled = failureSimulationState.Set(request.Enabled);
        return Ok(new FailureSimulationResponse(enabled));
    }
}
