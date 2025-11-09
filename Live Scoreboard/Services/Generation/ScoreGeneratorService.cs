using Live_Scoreboard.Data;
using Live_Scoreboard.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Live_Scoreboard.Services.Generation
{
    public class ScoreGeneratorService : IScoreGeneratorService, IDisposable
    {
        private readonly IHubContext<ScoreHub> _hubContext;
        private readonly ILogger<ScoreGeneratorService> _logger;
        private Timer? _timer;
        private readonly Random _random = new();
        private readonly List<string> _teamNames = new()
        {
            "Lions", "Tigers", "Bears", "Eagles", "Hawks", "Wolves", "Sharks", "Dragons",
            "Knights", "Royals", "Giants", "Patriots", "Warriors", "Vikings", "Spartans"
        };
        private List<Score> scores = new List<Score>();
        public bool IsRunning => _timer != null;

        public ScoreGeneratorService(ILogger<ScoreGeneratorService> logger, IHubContext<ScoreHub> hubContext)
        {
            _logger = logger;
            _hubContext = hubContext;
        }

        public Task StartGeneratingScoresAsync()
        {
            if (_timer != null) return Task.CompletedTask;

            _timer = new Timer(GenerateAndSendScore, null, TimeSpan.Zero, TimeSpan.FromSeconds(10)); // Every 10 seconds
            _logger.LogInformation("Score generator started");

            return Task.CompletedTask;
        }

        public Task StopGeneratingScoresAsync()
        {
            _timer?.Dispose();
            _timer = null;
            _logger.LogInformation("Score generator stopped");

            return Task.CompletedTask;
        }

        private async void GenerateAndSendScore(object? state)
        {
            try
            {
                var newScore = GenerateRandomScore();
                scores.Add(newScore);
                await _hubContext.Clients.All.SendAsync("ReceiveScores", scores);
                _logger.LogInformation("Generated score: {Team1} {Score1} - {Score2} {Team2}",
                    newScore.TeamOneName, newScore.TeamOneScore, newScore.TeamTwoScore, newScore.TeamTwoName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating and sending score");
            }
        }

        private Score GenerateRandomScore()
        {
            // Get two different random teams
            var team1Index = _random.Next(_teamNames.Count);
            var team2Index = _random.Next(_teamNames.Count);
            while (team2Index == team1Index)
            {
                team2Index = _random.Next(_teamNames.Count);
            }

            return new Score
            {
                Id = _random.Next(1000, 9999),
                TeamOneName = _teamNames[team1Index],
                TeamTwoName = _teamNames[team2Index],
                TeamOneScore = _random.Next(0, 10), // Scores from 0-9
                TeamTwoScore = _random.Next(0, 10)
            };
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}