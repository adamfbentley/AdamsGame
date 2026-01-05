using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

/// <summary>
/// Setup dodge animation in animator controller
/// </summary>
public class SetupDodgeAnimation : MonoBehaviour
{
    [MenuItem("Tools/Setup Dodge Animation")]
    static void SetupDodge()
    {
        string controllerPath = "Assets/PlayerAnimator.controller";
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        
        if (controller == null)
        {
            Debug.LogError("Could not find PlayerAnimator.controller");
            return;
        }
        
        // Get the base layer
        AnimatorControllerLayer baseLayer = controller.layers[0];
        AnimatorStateMachine stateMachine = baseLayer.stateMachine;
        
        // Check if Dodge state already exists
        AnimatorState existingDodge = null;
        foreach (var state in stateMachine.states)
        {
            if (state.state.name == "Dodge")
            {
                existingDodge = state.state;
                break;
            }
        }
        
        if (existingDodge != null)
        {
            Debug.Log("Dodge state already exists - just updating animation");
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Vampire A Lusth@Stand To Roll.fbx");
            if (clip != null)
            {
                existingDodge.motion = clip;
            }
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            Debug.Log("✓ Updated Dodge animation");
            return;
        }
        
        // Add Dodge parameter if it doesn't exist
        bool hasDodge = false;
        foreach (var param in controller.parameters)
        {
            if (param.name == "Dodge")
            {
                hasDodge = true;
                break;
            }
        }
        if (!hasDodge)
        {
            controller.AddParameter("Dodge", AnimatorControllerParameterType.Trigger);
            Debug.Log("Added Dodge trigger parameter");
        }
        
        // Create Dodge state
        AnimatorState dodgeState = stateMachine.AddState("Dodge", new Vector3(300, 200, 0));
        
        // Use walking animation for dodge - looks more natural than idle
        AnimationClip walkClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Vampire A Lusth@Walking.fbx");
        if (walkClip != null)
        {
            dodgeState.motion = walkClip;
            dodgeState.speed = 2.0f;  // 2x speed
            Debug.Log("Using Walking animation for Dodge (2x speed, code handles movement)");
        }
        else
        {
            Debug.LogWarning("Could not find Walking animation");
        }
        
        // Find locomotion states
        AnimatorState idleState = null;
        AnimatorState walkState = null;
        AnimatorState runState = null;
        
        foreach (var state in stateMachine.states)
        {
            if (state.state == null) continue;
            
            string stateName = state.state.name.ToLower();
            if (stateName.Contains("idle"))
            {
                idleState = state.state;
                Debug.Log($"Found idle state: {state.state.name}");
            }
            else if (stateName.Contains("walk"))
            {
                walkState = state.state;
                Debug.Log($"Found walk state: {state.state.name}");
            }
            else if (stateName.Contains("run"))
            {
                runState = state.state;
                Debug.Log($"Found run state: {state.state.name}");
            }
        }
        
        // Use first state if nothing found
        if (idleState == null && stateMachine.states.Length > 0)
        {
            idleState = stateMachine.states[0].state;
            Debug.Log($"Using first state as return: {idleState.name}");
        }
        
        // Create transition from Any State to Dodge
        AnimatorStateTransition anyToDodge = stateMachine.AddAnyStateTransition(dodgeState);
        anyToDodge.AddCondition(AnimatorConditionMode.If, 0, "Dodge");
        anyToDodge.canTransitionToSelf = false;
        anyToDodge.duration = 0.15f;  // Quick entry
        Debug.Log("Created Any State → Dodge transition (0.15s blend)");
        
        // Create transition from Dodge to Run (no exit time for instant response)
        if (runState != null)
        {
            AnimatorStateTransition dodgeToRun = dodgeState.AddTransition(runState);
            dodgeToRun.hasExitTime = false;  // No exit time - instant transition
            dodgeToRun.duration = 0.02f;  // Near-instant blend
            dodgeToRun.AddCondition(AnimatorConditionMode.Greater, 4f, "Speed");
            Debug.Log($"Created Dodge → Run transition (Speed > 4, no exit time)");
        }
        
        // Save changes
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        
        Debug.Log("✓ Dodge animation setup complete!");
        Debug.Log("- Created Dodge state with Stand To Roll animation");
        Debug.Log("- Any State → Dodge (trigger)");
        Debug.Log("- Dodge → Idle (exit time 0.95)");
    }
}
