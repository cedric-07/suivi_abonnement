using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class NotificationHub : Hub
{
    private static Dictionary<int, string> ConnectedUsers = new Dictionary<int, string>();

    public override async Task OnConnectedAsync()
    {
        try
        {
            var httpContext = Context.GetHttpContext();
            if (httpContext != null)
            {
                var userId = httpContext.Session.GetInt32("UserId");

                if (userId.HasValue)
                {
                    if (!ConnectedUsers.ContainsKey(userId.Value))
                    {
                        ConnectedUsers[userId.Value] = Context.ConnectionId;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error in OnConnectedAsync: " + ex.Message);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var user = ConnectedUsers.FirstOrDefault(x => x.Value == Context.ConnectionId);
        if (user.Key != 0)
        {
            ConnectedUsers.Remove(user.Key);
        }

        await base.OnDisconnectedAsync(exception);
    }
}
