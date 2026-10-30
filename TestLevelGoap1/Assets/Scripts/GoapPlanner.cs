using System.Collections.Generic;

public class GoapPlanner
{
    private class Node
    {
        public WorldState State;
        public List<GoapAction> Plan;
        public float Cost;

        public Node(
            WorldState state,
            List<GoapAction> plan,
            float cost)
        {
            State = state;
            Plan = plan;
            Cost = cost;
        }
    }

    public List<GoapAction> Plan(
        WorldState start,
        Dictionary<string, bool> goal,
        List<GoapAction> actions)
    {
        var frontier = new List<Node>
        {
            new Node(
                start.Clone(),
                new List<GoapAction>(),
                0f)
        };

        var bestCosts = new Dictionary<string, float>
        {
            { start.ToKey(), 0f }
        };

        while (frontier.Count > 0)
        {
            // Zoek de node met de laagste kosten.
            int bestIndex = 0;

            for (int i = 1; i < frontier.Count; i++)
            {
                if (frontier[i].Cost < frontier[bestIndex].Cost)
                    bestIndex = i;
            }

            Node current = frontier[bestIndex];
            frontier.RemoveAt(bestIndex);

            // Is het doel bereikt in deze voorspelde toestand?
            if (current.State.Matches(goal))
                return current.Plan;

            // Probeer alle acties vanuit deze toestand.
            foreach (GoapAction action in actions)
            {
                if (!action.IsPossible(current.State))
                    continue;

                WorldState nextState = action.Predict(current.State);
                float newCost = current.Cost + action.Cost;
                string key = nextState.ToKey();

                if (bestCosts.TryGetValue(key, out float oldCost)
                    && oldCost <= newCost)
                {
                    continue;
                }

                bestCosts[key] = newCost;

                var nextPlan = new List<GoapAction>(current.Plan);
                nextPlan.Add(action);

                frontier.Add(
                    new Node(nextState, nextPlan, newCost));
            }
        }

        // Geen geldig plan gevonden.
        return null;
    }
}
