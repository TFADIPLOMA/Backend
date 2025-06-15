using Microsoft.AspNetCore.SignalR;

namespace TwoFactorAuth.API.SocketHubs
{
    public class AuthHub:Hub
    {
        public async Task SendMessage(string email, string json)
        {
            await Clients.All.SendAsync("ReceiveMessage", email, json);
        }
    }
}
