

//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.SignalR; // ? This is required for IHubContext
//using Microsoft.EntityFrameworkCore;
//using EmployeeManagementAPI.Data;
//using EmployeeManagementAPI.Hubs;
//using System.Linq;
//using System.Threading.Tasks;


//[ApiController]
//    [Route("api/[controller]")]
//    public class ChatController : ControllerBase
//    {
//        private readonly ApplicationDbContext _context;

//    private readonly IHubContext<ChatHub> _hubContext;

//    public ChatController(ApplicationDbContext context, IHubContext<ChatHub> hubContext)
//    {
//        _context = context;
//        _hubContext = hubContext;
//    }

//    [HttpGet("history")]
//        public async Task<IActionResult> GetMessages(string user1, string user2)
//        {
//            var messages = await _context.ChatMessages
//                .Where(m =>
//                    (m.FromUser == user1 && m.ToUser == user2) ||
//                    (m.FromUser == user2 && m.ToUser == user1))
//                .OrderBy(m => m.Timestamp)
//                .ToListAsync();

//            return Ok(messages);
//        }

//        // ? Check if user is online
//        [HttpGet("isonline")]
//        public IActionResult IsUserOnline([FromQuery] string user)
//        {
//            bool isOnline = ChatHub.IsUserOnline(user);
//            return Ok(isOnline);
//        }

//        // ? Get last seen timestamp
//        [HttpGet("lastseen")]
//        public IActionResult GetLastSeen([FromQuery] string user)
//        {
//            var lastSeen = ChatHub.GetLastSeen(user);
//            if (lastSeen.HasValue)
//                return Ok(lastSeen.Value);
//            else
//                return NotFound("User has never been online.");
//        }

//    [HttpGet("unreadcount")]
//    public IActionResult GetUnreadCounts(string forUser)
//    {
//        var unreadCounts = _context.ChatMessages
//            .Where(m => m.ToUser == forUser && !m.IsRead)
//            .GroupBy(m => m.FromUser)
//            .Select(g => new {
//                From = g.Key,
//                Count = g.Count()
//            })
//            .ToList();

//        return Ok(unreadCounts);
//    }

//    [HttpPost("markasread")]
//    public async Task<IActionResult> MarkAsRead([FromQuery] string user1, [FromQuery] string user2)
//    {
//        var messages = await _context.ChatMessages
//            .Where(m => m.FromUser == user2 && m.ToUser == user1 && !m.IsRead)
//            .ToListAsync();

//        foreach (var msg in messages)
//        {
//            msg.IsRead = true;

//            await _hubContext.Clients.Group(msg.FromUser).SendAsync("MessageRead", user1, msg.Id);
//        }

//        await _context.SaveChangesAsync();
//        return Ok();
//    }







//}

using Microsoft.AspNetCore.Mvc;
using EmployeeManagementAPI.Data;
using Microsoft.EntityFrameworkCore;
using EmployeeManagementAPI.Hubs; // ?? Needed to access ChatHub static methods
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.SignalR;


[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<ChatHub> _hubContext;


    public ChatController(ApplicationDbContext context, IHubContext<ChatHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetMessages(string user1, string user2)
    {
        var messages = await _context.ChatMessages
            .Where(m =>
                (m.FromUser == user1 && m.ToUser == user2) ||
                (m.FromUser == user2 && m.ToUser == user1))
            .OrderBy(m => m.Timestamp)
            .ToListAsync();

        return Ok(messages);
    }

    // ? Check if user is online
    [HttpGet("isonline")]
    public IActionResult IsUserOnline([FromQuery] string user)
    {
        bool isOnline = ChatHub.IsUserOnline(user);
        return Ok(isOnline);
    }

    // ? Get last seen timestamp
    [HttpGet("lastseen")]
    public IActionResult GetLastSeen([FromQuery] string user)
    {
        var lastSeen = ChatHub.GetLastSeen(user);
        if (lastSeen.HasValue)
            return Ok(lastSeen.Value);
        else
            return NotFound("User has never been online.");
    }

    [HttpGet("unreadcount")]
    public IActionResult GetUnreadCounts(string forUser)
    {
        var unreadCounts = _context.ChatMessages
            .Where(m => m.ToUser == forUser && !m.IsRead)
            .GroupBy(m => m.FromUser)
            .Select(g => new {
                From = g.Key,
                Count = g.Count()
            })
            .ToList();

        return Ok(unreadCounts);
    }

    [HttpPost("markasread")]
    public async Task<IActionResult> MarkAsRead([FromQuery] string user1, [FromQuery] string user2)
    {
        var messages = await _context.ChatMessages
            .Where(m => m.FromUser == user2 && m.ToUser == user1 && !m.IsRead)
            .ToListAsync();

        foreach (var msg in messages)
        {
            msg.IsRead = true;
        }

        await _context.SaveChangesAsync();

        // Real-time update to the sender
        await _hubContext.Clients.User(user2).SendAsync("MessagesRead", user1);

        return Ok();
    }
}
