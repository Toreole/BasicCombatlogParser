namespace CombatlogParser.Data.Events
{
    public interface IAdvancedParamEvent
    {
        AdvancedParams AdvancedParams { get; }
        string SourceGUID { get; }
        string TargetGUID { get; }
    }
}
