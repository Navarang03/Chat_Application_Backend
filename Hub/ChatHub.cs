

//using Microsoft.AspNetCore.SignalR;
//using EmployeeManagementAPI.Data;
//using EmployeeManagementAPI.Models;
//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//namespace EmployeeManagementAPI.Hubs
//{
//    public class ChatHub : Hub
//    {
//        private readonly ApplicationDbContext _context;

//        // ?? Static collections to track user status
//        private static readonly HashSet<string> OnlineUsers = new();
//        private static readonly Dictionary<string, DateTime> UserLastSeen = new();
//        private static readonly Dictionary<string, int> UserConnectionCounts = new();


//        public ChatHub(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        public async Task SendMessage(string fromUser, string toUser, string message)
//        {
//            var timestamp = DateTime.Now;

//            // Save to DB
//            var chatMessage = new ChatMessage
//            {
//                FromUser = fromUser,
//                ToUser = toUser,
//                Message = message,
//                Timestamp = timestamp
//            };

//            _context.ChatMessages.Add(chatMessage);
//            await _context.SaveChangesAsync();

//            // Add timestamp to message sent to clients
//            await Clients.Group(toUser).SendAsync("ReceiveMessage", fromUser, message, timestamp);
//            // After sending, mark as delivered
//            chatMessage.IsDelivered = true;
//            await _context.SaveChangesAsync();

//            await Clients.Group(fromUser).SendAsync("ReceiveMessage", fromUser, message, timestamp);
//        }

//        public override async Task OnConnectedAsync()
//        {
//            var httpContext = Context.GetHttpContext();
//            var username = httpContext?.Request.Query["username"].ToString();

//            Console.WriteLine($"[CONNECTED] ConnectionId: {Context.ConnectionId}, Username: {username}");

//            if (!string.IsNullOrEmpty(username))
//            {
//                await Groups.AddToGroupAsync(Context.ConnectionId, username);

//                if (UserConnectionCounts.ContainsKey(username))
//                    UserConnectionCounts[username]++;
//                else
//                    UserConnectionCounts[username] = 1;

//                OnlineUsers.Add(username);
//                Console.WriteLine($"[ONLINE] {username} now online. Total connections: {UserConnectionCounts[username]}");

//                await Clients.All.SendAsync("UserStatusChanged", username, true);
//            }
//            else
//            {
//                Console.WriteLine("[WARNING] Username is null or empty in OnConnectedAsync.");
//            }

//            await base.OnConnectedAsync();
//        }


//        public override async Task OnDisconnectedAsync(Exception? exception)
//        {
//            var httpContext = Context.GetHttpContext();
//            var username = httpContext?.Request.Query["username"].ToString();
//            if (!string.IsNullOrEmpty(username))
//            {
//                await Groups.RemoveFromGroupAsync(Context.ConnectionId, username);

//                if (UserConnectionCounts.ContainsKey(username))
//                {
//                    UserConnectionCounts[username]--;
//                    if (UserConnectionCounts[username] <= 0)
//                    {
//                        OnlineUsers.Remove(username);
//                        UserLastSeen[username] = DateTime.UtcNow;
//                        await Clients.All.SendAsync("UserStatusChanged", username, false);
//                    }
//                }
//            }


//            await base.OnDisconnectedAsync(exception);
//        }

//        // ? Expose last seen (you can call this from your controller)
//        public static DateTime? GetLastSeen(string username)
//        {
//            return UserLastSeen.TryGetValue(username, out var lastSeen) ? lastSeen : null;
//        }

//        public static bool IsUserOnline(string username)
//        {
//            return OnlineUsers.Contains(username);
//        }

//        public async Task SendTypingNotification(string fromUser, string toUser)
//        {
//            await Clients.Group(toUser).SendAsync("UserTyping", fromUser);
//        }

//    }
//}

using Microsoft.AspNetCore.SignalR;
using EmployeeManagementAPI.Data;
using EmployeeManagementAPI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EmployeeManagementAPI.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;

        // ?? Static collections to track user status
        private static readonly HashSet<string> OnlineUsers = new();
        private static readonly Dictionary<string, DateTime> UserLastSeen = new();
        private static readonly Dictionary<string, int> UserConnectionCounts = new();

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SendMessage(string fromUser, string toUser, string message)
        {
            var timestamp = DateTime.Now;

            // Save to DB
            var chatMessage = new ChatMessage
            {
                FromUser = fromUser,
                ToUser = toUser,
                Message = message,
                Timestamp = timestamp
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            // Add timestamp to message sent to clients
            chatMessage.IsDelivered = true;
            await _context.SaveChangesAsync();

            await Clients.Group(toUser).SendAsync("ReceiveMessage", fromUser, message, timestamp, true, false);
            await Clients.Group(fromUser).SendAsync("ReceiveMessage", fromUser, message, timestamp, true, false);

        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var username = httpContext?.Request.Query["username"].ToString();

            Console.WriteLine($"[CONNECTED] ConnectionId: {Context.ConnectionId}, Username: {username}");

            if (!string.IsNullOrEmpty(username))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, username);

                if (UserConnectionCounts.ContainsKey(username))
                    UserConnectionCounts[username]++;
                else
                    UserConnectionCounts[username] = 1;

                OnlineUsers.Add(username);
                Console.WriteLine($"[ONLINE] {username} now online. Total connections: {UserConnectionCounts[username]}");

                await Clients.All.SendAsync("UserStatusChanged", username, true);
            }
            else
            {
                Console.WriteLine("[WARNING] Username is null or empty in OnConnectedAsync.");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var httpContext = Context.GetHttpContext();
            var username = httpContext?.Request.Query["username"].ToString();
            if (!string.IsNullOrEmpty(username))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, username);

                if (UserConnectionCounts.ContainsKey(username))
                {
                    UserConnectionCounts[username]--;
                    if (UserConnectionCounts[username] <= 0)
                    {
                        OnlineUsers.Remove(username);
                        UserLastSeen[username] = DateTime.UtcNow;
                        await Clients.All.SendAsync("UserStatusChanged", username, false);
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        // ? Expose last seen (you can call this from your controller)
        public static DateTime? GetLastSeen(string username)
        {
            return UserLastSeen.TryGetValue(username, out var lastSeen) ? lastSeen : null;
        }

        public static bool IsUserOnline(string username)
        {
            return OnlineUsers.Contains(username);
        }

        public async Task SendTypingNotification(string fromUser, string toUser)
        {
            await Clients.Group(toUser).SendAsync("UserTyping", fromUser);
        }

        public async Task NotifyMessagesRead(string fromUser, string toUser)
        {
            // Notifies 'fromUser' that messages they sent to 'toUser' were read
            await Clients.Group(fromUser).SendAsync("MessagesRead", toUser);
        }


    }
}
