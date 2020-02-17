namespace StoryChart.Model.Rewards
{
    public interface INodeReward
    {
        string Description { get; set; }
        string Id { get; set; }
        RewardsTypesEnum Type { get; set; }
    }
}