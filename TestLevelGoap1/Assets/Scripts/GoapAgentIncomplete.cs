using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoapAgentIncomplete : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private Transform woodTarget;
    [SerializeField] private Transform torchTarget;
    [SerializeField] private Transform fireTarget;
    [SerializeField] private Transform bucketTarget;
    [SerializeField] private Transform waterTarget;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float interactionTime = 1f;

    [Header("Visuals")]
    [SerializeField] private Renderer woodRenderer;
    [SerializeField] private Renderer torchRenderer;
    [SerializeField] private Renderer fireRenderer;
    [SerializeField] private Renderer bucketRenderer;
    [SerializeField] private Renderer waterRenderer;

    [Header("Action Costs - Lighting the Fire wood")]
    [SerializeField] private float moveToWoodCost = 2f;
    [SerializeField] private float collectWoodCost = 1f;
    [SerializeField] private float moveToFireWithWoodCost = 2f;
    [SerializeField] private float lightFireWithWoodCost = 1f;

    [Header("Action costs - Lighting the Fire with a torch")]
    [SerializeField] private float moveToTorchCost = 1f;
    [SerializeField] private float collectTorchCost = 1f;
    [SerializeField] private float moveToFireWithTorchCost = 2f;
    [SerializeField] private float lightFireWithTorchCost = 1f;

    [Header("Action Costs - Dousing the Fire")]
    [SerializeField] private float moveToBucketCost = 1f;
    [SerializeField] private float grabBucketCost = 1f;
    [SerializeField] private float moveToWaterCost = 1f;
    [SerializeField] private float fillBucketCost = 1f;
    [SerializeField] private float moveToDouseFireCost = 1f;
    [SerializeField] private float douseFireCost = 1f;

    private WorldState world;
    private GoapPlanner planner;
    private List<GoapAction> actions;

    private List<GoapAction> woodLightingActions;
    private List<GoapAction> torchLightingActions;
    private List<GoapAction> fireDousingActions;

    private float woodRouteCost;
    private float torchRouteCost;

    private string currentAction = "Initializing...";
    private string chosenLightingRoute = "Not selected";
    private bool finished;

    private void Start()
    {
        if (woodTarget == null || fireTarget == null)
        {
            Debug.LogError(
                "Assign Wood and Fire targets in the inspector.",
                this);
            enabled = false;
            return;
        }

        // Initial state of the world.
        world = new WorldState();
        // TODO: 1. Add the initial world-state facts (HasTorch, AtTorch, HasWood & AtWood).

        // Define the actions that can light the fire using wood.
        
        // TODO: 2. Fill in the moveToWood action and create actions for the other steps (collectWood, moveToFireWithWood & lightFireWithWood).
        // The name of the action should be moveToFire for moveToFireWithWood and lightFire for lightFireWithWood since these actions will be reused
        GoapAction moveToWood = new GoapAction(
            "",
            0f,
            new Dictionary<string, bool>
            {
                { "", false },
            },
            new Dictionary<string, bool>
            {
                { "", true }
            });

        // TODO: 3. Add all actions to the List.
        woodLightingActions = new List<GoapAction>
        {
            moveToWood,
        };

        // Define alternative actions that use a torch.
        // TODO: 6. Define the four torch actions (Same as by 2 but now for the torch) and put them in their own actions list.
        
        torchLightingActions = new List<GoapAction>();

        // Define the bucket and water actions used to extinguish the fire.
        // TODO: 9. Define the bucket and water actions used to extinguish the fire.

        // Combine both lighting routes and the extinguishing actions.
        actions = new List<GoapAction>();
        actions.AddRange(woodLightingActions);
        // TODO: 7. Add a range for lighting the fire with a torch.
        // TODO: 10. Add a range for dousing the fire.
        
        planner = new GoapPlanner();

        var fireLitGoal = new Dictionary<string, bool>
        {
            // TODO: 11. Change the goal to DouseFire.
            { "FireLit", true }
        };

        // TODO: 4. Finish the fullPlan. uncomment the section below when fullPlan is finished. Use fireLitGoal.
        List<GoapAction> fullPlan;

        // if (fullPlan == null)
        // {
        //     Debug.LogError("GOAP could not find a plan.", this);
        //     currentAction = "No plan found";
        //     return;
        // }
        //
        // Debug.Log("===== COMPLETE GOAP PLAN =====");
        // foreach (GoapAction action in fullPlan)
        // {
        //     Debug.Log("- " + action.Name);
        // }
        //
        // StartCoroutine(ExecutePlan(fullPlan));
    }

    private float GetPlanCost(List<GoapAction> plan)
    {
        if (plan == null)
            return float.PositiveInfinity;

        float total = 0f;
        foreach (GoapAction action in plan)
            total += action.Cost;

        return total;
    }

    private string DescribePlan(List<GoapAction> plan)
    {
        if (plan == null)
            return "No valid plan";

        List<string> names = new List<string>();
        foreach (GoapAction action in plan)
            names.Add(action.Name);

        return string.Join(" -> ", names);
    }

    private IEnumerator ExecutePlan(List<GoapAction> plan)
    {
        foreach (GoapAction action in plan)
        {
            if (!action.IsPossible(world))
            {
                Debug.LogError(
                    "Action is no longer possible: " + action.Name,
                    this);
                currentAction = "Action failed";
                yield break;
            }

            currentAction = action.Name;
            Debug.Log("Executing: " + action.Name);

            switch (action.Name)
            {
                case "MoveToWood":
                    yield return 0;
                    break;

                case "CollectWood":
                    yield return 0;
                    break;

                // TODO: 5. Add execution cases for each action for the wood route.
                // MoveToWood should move the agent to the woodTarget. Use the MoveTo() function.
                // CollectWood should wait for interactionTime and hide the woodRenderer.
                // Add a case for MoveToFire.
                // Add a case for LightFire. Use the SetFireColor() function. Set the color to orange or red.
                
                // TODO: 8. Do the same for the torch path.
                
                // TODO: 12. Do the same for dousing fire. Set the color of the fire to grey.
            }

            // Update the simulated world only after the action completes.
            world.Apply(action.Effects);
        }

        finished = world.Get("FireDoused");
        currentAction = finished ? "Goal reached!" : "Plan ended";
        Debug.Log(currentAction);
    }

    private void SetFireColor(Color color)
    {
        if (fireRenderer != null)
            fireRenderer.material.color = color;
    }

    private IEnumerator MoveTo(Vector3 destination)
    {
        destination.y = transform.position.y;

        while (Vector3.Distance(transform.position, destination) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                destination,
                moveSpeed * Time.deltaTime);

            yield return null;
        }

        transform.position = destination;
    }

    private void OnGUI()
    {
        GUI.Box(new Rect(10, 10, 430, 190), "GOAP Planner");

        GUI.Label(new Rect(20, 35, 410, 25),
            "Goal: FireDoused = true");
        
        GUI.Label(new Rect(20, 135, 410, 25),
            "Current action: " + currentAction);
    }
}
