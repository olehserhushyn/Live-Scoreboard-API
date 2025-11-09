namespace Live_Scoreboard.Services.Generation
{
    public interface IScoreGeneratorService
    {
        Task StartGeneratingScoresAsync();
        Task StopGeneratingScoresAsync();
        bool IsRunning { get; }
    }
}
