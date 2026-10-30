using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoapAgent : MonoBehaviour
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

    [Header("Action Costs - Lighting the Fire")]
    [SerializeField] private float moveToWoodCost = 2f;
    [SerializeField] private float collectWoodCost = 1f;
    [SerializeField] private float moveToFireWithWoodCost = 2f;
    [SerializeField] private float lightFireWithWoodCost = 1f;

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

    private float woodRouteCost;
    private float torchRouteCost;

    private string currentAction = "Initializing...";
    private string chosenLightingRoute = "Not selected";
    private bool finished;

    private void Start()
    {
        if (woodTarget == null ||
            torchTarget == null ||
            fireTarget == null ||
            bucketTarget == null ||
            waterTarget == null)
        {
            Debug.LogError(
                "Assign Wood, Torch, Fire, Bucket and Water Targets in the Inspector.",
                this);
            enabled = false;
            return;
        }

        // 1. Initial state of the world.
        world = new WorldState();
        world.Set("HasWood", false);
        world.Set("AtWood", false);
        world.Set("HasTorch", false);
        world.Set("AtTorch", false);
        world.Set("AtFire", false);
        world.Set("FireLit", false);
        world.Set("HasBucket", false);
        world.Set("AtBucket", false);
        world.Set("AtWater", false);
        world.Set("BucketFull", false);
        world.Set("FireDoused", false);

        // 2. Define the actions that can light the fire using wood.
        GoapAction moveToWood = new GoapAction(
            "MoveToWood",
            moveToWoodCost,
            new Dictionary<string, bool>
            {
                { "HasWood", false },
                { "HasTorch", false },
                { "AtWood", false },
                { "AtTorch", false },
                { "AtFire", false }
            },
            new Dictionary<string, bool>
            {
                { "AtWood", true }
            });

        GoapAction collectWood = new GoapAction(
            "CollectWood",
            collectWoodCost,
            new Dictionary<string, bool>
            {
                { "AtWood", true },
                { "HasWood", false },
                { "HasTorch", false }
            },
            new Dictionary<string, bool>
            {
                { "HasWood", true }
            });

        GoapAction moveToFireWithWood = new GoapAction(
            "MoveToFire",
            moveToFireWithWoodCost,
            new Dictionary<string, bool>
            {
                { "HasWood", true },
                { "AtFire", false }
            },
            new Dictionary<string, bool>
            {
                { "AtFire", true },
                { "AtWood", false }
            });

        GoapAction lightFireWithWood = new GoapAction(
            "LightFire",
            lightFireWithWoodCost,
            new Dictionary<string, bool>
            {
                { "HasWood", true },
                { "AtFire", true },
                { "FireLit", false }
            },
            new Dictionary<string, bool>
            {
                { "FireLit", true },
                { "HasWood", false }
            });

        woodLightingActions = new List<GoapAction>
        {
            moveToWood,
            collectWood,
            moveToFireWithWood,
            lightFireWithWood
        };

        // 3. Define the alternative actions that use a torch.
        GoapAction moveToTorch = new GoapAction(
            "MoveToTorch",
            moveToTorchCost,
            new Dictionary<string, bool>
            {
                { "HasWood", false },
                { "HasTorch", false },
                { "AtWood", false },
                { "AtTorch", false },
                { "AtFire", false }
            },
            new Dictionary<string, bool>
            {
                { "AtTorch", true }
            });

        GoapAction collectTorch = new GoapAction(
            "CollectTorch",
            collectTorchCost,
            new Dictionary<string, bool>
            {
                { "AtTorch", true },
                { "HasTorch", false },
                { "HasWood", false }
            },
            new Dictionary<string, bool>
            {
                { "HasTorch", true }
            });

        GoapAction moveToFireWithTorch = new GoapAction(
            "MoveToFire",
            moveToFireWithTorchCost,
            new Dictionary<string, bool>
            {
                { "HasTorch", true },
                { "AtFire", false }
            },
            new Dictionary<string, bool>
            {
                { "AtFire", true },
                { "AtTorch", false }
            });

        GoapAction lightFireWithTorch = new GoapAction(
            "LightFire",
            lightFireWithTorchCost,
            new Dictionary<string, bool>
            {
                { "HasTorch", true },
                { "AtFire", true },
                { "FireLit", false }
            },
            new Dictionary<string, bool>
            {
                { "FireLit", true },
                { "HasTorch", false }
            });

        torchLightingActions = new List<GoapAction>
        {
            moveToTorch,
            collectTorch,
            moveToFireWithTorch,
            lightFireWithTorch
        };

        // 4. Define the bucket and water actions used to extinguish the fire.
        GoapAction moveToBucket = new GoapAction(
            "MoveToBucket",
            moveToBucketCost,
            new Dictionary<string, bool>
            {
                { "AtBucket", false },
                { "HasBucket", false },
                { "FireLit", true }
            },
            new Dictionary<string, bool>
            {
                { "AtBucket", true },
                { "AtFire", false }
            });

        GoapAction grabBucket = new GoapAction(
            "GrabBucket",
            grabBucketCost,
            new Dictionary<string, bool>
            {
                { "AtBucket", true },
                { "HasBucket", false },
                { "FireLit", true }
            },
            new Dictionary<string, bool>
            {
                { "HasBucket", true }
            });

        GoapAction moveToWater = new GoapAction(
            "MoveToWater",
            moveToWaterCost,
            new Dictionary<string, bool>
            {
                { "HasBucket", true },
                { "FireLit", true },
                { "AtWater", false }
            },
            new Dictionary<string, bool>
            {
                { "AtWater", true },
                { "AtBucket", false }
            });

        GoapAction fillBucket = new GoapAction(
            "FillBucket",
            fillBucketCost,
            new Dictionary<string, bool>
            {
                { "HasBucket", true },
                { "FireLit", true },
                { "AtWater", true },
                { "BucketFull", false }
            },
            new Dictionary<string, bool>
            {
                { "BucketFull", true }
            });

        GoapAction moveToDouseFire = new GoapAction(
            "MoveToDouseFire",
            moveToDouseFireCost,
            new Dictionary<string, bool>
            {
                { "HasBucket", true },
                { "FireLit", true },
                { "BucketFull", true },
                { "AtFire", false }
            },
            new Dictionary<string, bool>
            {
                { "AtFire", true },
                { "AtWater", false }
            });

        GoapAction douseFire = new GoapAction(
            "DouseFire",
            douseFireCost,
            new Dictionary<string, bool>
            {
                { "HasBucket", true },
                { "FireLit", true },
                { "BucketFull", true },
                { "AtFire", true }
            },
            new Dictionary<string, bool>
            {
                { "FireLit", false },
                { "BucketFull", false },
                { "FireDoused", true }
            });

        // Combine both lighting routes and the extinguishing actions.
        actions = new List<GoapAction>();
        actions.AddRange(woodLightingActions);
        actions.AddRange(torchLightingActions);
        actions.Add(moveToBucket);
        actions.Add(grabBucket);
        actions.Add(moveToWater);
        actions.Add(fillBucket);
        actions.Add(moveToDouseFire);
        actions.Add(douseFire);

        // 5. Compare the two ways of lighting the fire.
        planner = new GoapPlanner();

        var fireLitGoal = new Dictionary<string, bool>
        {
            { "FireLit", true }
        };

        List<GoapAction> woodPlan =
            planner.Plan(world, fireLitGoal, woodLightingActions);

        List<GoapAction> torchPlan =
            planner.Plan(world, fireLitGoal, torchLightingActions);

        woodRouteCost = GetPlanCost(woodPlan);
        torchRouteCost = GetPlanCost(torchPlan);

        Debug.Log("===== GOAP ROUTE COMPARISON =====");
        Debug.Log("Wood route (" + woodRouteCost + "): " + DescribePlan(woodPlan));
        Debug.Log("Torch route (" + torchRouteCost + "): " + DescribePlan(torchPlan));

        chosenLightingRoute = torchRouteCost < woodRouteCost
            ? "Torch"
            : woodRouteCost < torchRouteCost
                ? "Wood"
                : "Tie (planner action order decides)";

        Debug.Log("Cheapest lighting route: " + chosenLightingRoute);

        // 6. The actual goal is to light the fire and then extinguish it.
        var finalGoal = new Dictionary<string, bool>
        {
            { "FireDoused", true }
        };

        List<GoapAction> fullPlan =
            planner.Plan(world, finalGoal, actions);

        if (fullPlan == null)
        {
            Debug.LogError("GOAP could not find a plan.", this);
            currentAction = "No plan found";
            return;
        }

        Debug.Log("===== COMPLETE GOAP PLAN =====");
        foreach (GoapAction action in fullPlan)
        {
            Debug.Log("- " + action.Name);
        }

        StartCoroutine(ExecutePlan(fullPlan));
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
                    yield return MoveTo(woodTarget.position);
                    break;

                case "CollectWood":
                    yield return new WaitForSeconds(interactionTime);
                    if (woodRenderer != null)
                        woodRenderer.enabled = false;
                    break;

                case "MoveToTorch":
                    yield return MoveTo(torchTarget.position);
                    break;

                case "CollectTorch":
                    yield return new WaitForSeconds(interactionTime);
                    if (torchRenderer != null)
                        torchRenderer.enabled = false;
                    break;
                
                case "MoveToFire":
                    yield return MoveTo(fireTarget.position);
                    break;
                
                case "LightFire":
                    yield return new WaitForSeconds(interactionTime);
                    SetFireColor(new Color(1f, 0.35f, 0.05f));
                    break;

                case "MoveToBucket":
                    yield return MoveTo(bucketTarget.position);
                    break;

                case "GrabBucket":
                    yield return new WaitForSeconds(interactionTime);
                    if (bucketRenderer != null)
                        bucketRenderer.enabled = false;
                    break;

                case "MoveToWater":
                    yield return MoveTo(waterTarget.position);
                    break;

                case "FillBucket":
                    yield return new WaitForSeconds(interactionTime);
                    if (waterRenderer != null)
                        waterRenderer.material.color = Color.cyan;
                    break;

                case "MoveToDouseFire":
                    yield return MoveTo(fireTarget.position);
                    break;

                case "DouseFire":
                    yield return new WaitForSeconds(interactionTime);
                    SetFireColor(new Color(0.55f, 0.55f, 0.55f));
                    break;
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

        GUI.Label(new Rect(20, 60, 410, 25),
            "Wood route cost: " + woodRouteCost);

        GUI.Label(new Rect(20, 85, 410, 25),
            "Torch route cost: " + torchRouteCost);

        GUI.Label(new Rect(20, 110, 410, 25),
            "Cheapest lighting route: " + chosenLightingRoute);

        GUI.Label(new Rect(20, 135, 410, 25),
            "Current action: " + currentAction);

        GUI.Label(new Rect(20, 160, 410, 25),
            "Fire lit: " + world?.Get("FireLit") +
            " | Fire doused: " + world?.Get("FireDoused"));
    }
}