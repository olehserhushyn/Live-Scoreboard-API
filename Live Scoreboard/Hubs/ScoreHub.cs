using Live_Scoreboard.Data;
using Microsoft.AspNetCore.SignalR;

namespace Live_Scoreboard.Hubs
{
    public class ScoreHub : Hub
    {
        public async Task SendScores(List<Score> scores)
        {
            await Clients.All.SendCoreAsync("ReceiveScores", new object[] { scores });
        }
    }
}
