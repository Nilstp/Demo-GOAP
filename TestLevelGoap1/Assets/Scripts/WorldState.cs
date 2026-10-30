using System.Collections.Generic;

public class WorldState
{
    private readonly Dictionary<string, bool> facts;

    public WorldState()
    {
        facts = new Dictionary<string, bool>();
    }

    private WorldState(Dictionary<string, bool> source)
    {
        facts = new Dictionary<string, bool>(source);
    }

    public bool Get(string key)
    {
        return facts.TryGetValue(key, out bool value) && value;
    }

    public void Set(string key, bool value)
    {
        facts[key] = value;
    }

    public WorldState Clone()
    {
        return new WorldState(facts);
    }

    public void Apply(Dictionary<string, bool> effects)
    {
        foreach (var effect in effects)
        {
            Set(effect.Key, effect.Value);
        }
    }

    public bool Matches(Dictionary<string, bool> conditions)
    {
        foreach (var condition in conditions)
        {
            if (Get(condition.Key) != condition.Value)
                return false;
        }

        return true;
    }

    public string ToKey()
    {
        var keys = new List<string>(facts.Keys);
        keys.Sort();

        string result = "";

        foreach (string key in keys)
        {
            result += key + "=" + Get(key) + ";";
        }

        return result;
    }
}