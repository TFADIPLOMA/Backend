using Microsoft.AspNetCore.SignalR;

namespace TwoFactorAuth.API.SocketHubs
{
    public class QrLoginHub:Hub
    {
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var guid = httpContext.Request.Query["guid"];

            if (!string.IsNullOrEmpty(guid))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, guid);
                Console.WriteLine($"➡ Клиент {Context.ConnectionId} присоединился к группе {guid}");
            }

            await base.OnConnectedAsync();
        }
    }
}
