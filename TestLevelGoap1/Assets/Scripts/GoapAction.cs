using System.Collections.Generic;

public class GoapAction
{
    public string Name { get; }
    public float Cost { get; }

    public Dictionary<string, bool> Preconditions { get; }
    public Dictionary<string, bool> Effects { get; }

    public GoapAction(
        string name,
        float cost,
        Dictionary<string, bool> preconditions,
        Dictionary<string, bool> effects)
    {
        Name = name;
        Cost = cost;
        Preconditions = preconditions;
        Effects = effects;
    }

    public bool IsPossible(WorldState state)
    {
        return state.Matches(Preconditions);
    }

    public WorldState Predict(WorldState state)
    {
        WorldState nextState = state.Clone();
        nextState.Apply(Effects);

        return nextState;
    }
}