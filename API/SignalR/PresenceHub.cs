using System.Threading;
using System;
using System.Threading.Tasks;
using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    [Authorize]
    public class PresenceHub : Hub
    {
        private readonly PresenceTracker _tracker;
        public PresenceHub(PresenceTracker tracker)
        {
            _tracker = tracker;
        }
        public override async Task OnConnectedAsync()
        {
            var isOnline = await _tracker.UserConnected(Context.User.GetUsername().ToLower(),
                    Context.ConnectionId);

            if (isOnline)
            {
                // si se ha conectado (generado primer connectionId) envia mensaje a todos los demas
                await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUsername().ToLower());
            }

            var currentUsers = await _tracker.GetOnlineUsers();
            await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var isOffline = await _tracker.UserDisconnected(Context.User.GetUsername().ToLower(),
                Context.ConnectionId);

            if (isOffline)
            {
                // si el usuario ha cerrado todas sus conexiones en diferentes dispositivos, enviar mensaje
                // 'oficial' que se ha salido
                await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUsername().ToLower());
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}