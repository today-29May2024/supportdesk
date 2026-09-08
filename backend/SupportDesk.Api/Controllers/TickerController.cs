using Microsoft.AspNetCore.Mvc;
using SupportDesk.Core.Dtos;
using SupportDesk.Core.Services;

namespace SupportDesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<TicketListDto>>> GetTickets([FromQuery] TicketFilterParams filter)
    {
        return Ok(await _ticketService.GetTicketsAsync(filter));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TicketDetailDto>> GetTicketById(int id)
    {
        return Ok(await _ticketService.GetTicketByIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<TicketDetailDto>> CreateTicket([FromBody] CreateTicketDto dto)
    {
        var result = await _ticketService.CreateTicketAsync(dto);
        return CreatedAtAction(nameof(GetTicketById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TicketDetailDto>> UpdateTicket(int id, [FromBody] UpdateTicketDto dto)
    {
        return Ok(await _ticketService.UpdateTicketAsync(id, dto));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTicket(int id)
    {
        await _ticketService.DeleteTicketAsync(id);
        return NoContent();
    }

    // Dedicated endpoint for assignment
    [HttpPut("{id:int}/assign")]
    public async Task<ActionResult<TicketDetailDto>> AssignAgent(int id, [FromBody] AssignAgentDto dto)
    {
        return Ok(await _ticketService.AssignAgentAsync(id, dto));
    }

    [HttpPut("{id:int}/unassign")]
    public async Task<ActionResult<TicketDetailDto>> UnassignAgent(int id)
    {
        return Ok(await _ticketService.UnassignAgentAsync(id));
    }

    // Dedicated status transition endpoint (Rule #2 requirement)
    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<TicketDetailDto>> ChangeStatus(int id, [FromBody] ChangeStatusDto dto)
    {
        return Ok(await _ticketService.ChangeStatusAsync(id, dto));
    }

    [HttpPost("{id:int}/comments")]
    public async Task<ActionResult<CommentDto>> AddComment(int id, [FromBody] CreateCommentDto dto)
    {
        var result = await _ticketService.AddCommentAsync(id, dto);
        return CreatedAtAction(nameof(GetTicketById), new { id }, result);
    }
}