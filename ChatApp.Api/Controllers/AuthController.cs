using ChatApp.Domain.Models;
using ChatApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private ChatAppDbContext _context;
    public AuthController(ChatAppDbContext chatAppDbContext)
    {
        _context = chatAppDbContext;
    }

    [HttpGet("Test")]
    public async Task<IActionResult> Test(string message)
    {
        var newModel = new TestModel();
        newModel.Message = message;
        newModel.Id = Guid.NewGuid().ToString();
        var res = await _context.TestModel.OrderByDescending(m => m.CreatedAt).FirstOrDefaultAsync();
        if(res == null)
        {
            res = newModel;
        }
        _context.TestModel.Add(newModel);
        await _context.SaveChangesAsync();
        return Ok(new {Message = res.Message});
    }
}
