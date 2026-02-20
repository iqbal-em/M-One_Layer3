using Microsoft.AspNetCore.SignalR;

namespace M_One_Layer3.Hubs
{
    public class FingerprintHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            //Console.WriteLine("Client connected: " + Context.ConnectionId);

            //await Clients.Caller.SendAsync("PreviewUpdated", "TEST123");

            await base.OnConnectedAsync();
        }
    }
}
