using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

/// <summary>
/// One-time script to configure attack animation in animator
/// Run this from Tools menu
/// </summary>
public class SetupAttackAnimation : MonoBehaviour
{
    [MenuItem("Tools/Setup Attack Animation")]
    static void SetupAttack()
    {
        // Find the animator controller
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
        
        // Remove any existing attack states
        var statesToRemove = new System.Collections.Generic.List<AnimatorState>();
        foreach (var state in stateMachine.states)
        {
            if (state.state == null) continue;
            if (state.state.name.ToLower().Contains("attack") || 
                state.state.name.ToLower().Contains("punch") ||
                state.state.name.ToLower().Contains("melee"))
            {
                statesToRemove.Add(state.state);
            }
        }
        foreach (var state in statesToRemove)
        {
            stateMachine.RemoveState(state);
        }
        
        // Add Attack trigger parameter if it doesn't exist
        bool hasAttack = false;
        foreach (var param in controller.parameters)
        {
            if (param.name == "Attack")
            {
                hasAttack = true;
                break;
            }
        }
        if (!hasAttack)
        {
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
            Debug.Log("Added Attack trigger parameter");
        }
        
        // Create Attack state
        AnimatorState attackState = stateMachine.AddState("Attack", new Vector3(300, 200, 0));
        
        // Load attack animation
        AnimationClip attackClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Vampire A Lusth@Standing Melee Punch.fbx");
        if (attackClip != null)
        {
            attackState.motion = attackClip;
            attackState.speed = 1.2f;  // Slightly faster for snappier combat
            Debug.Log("Using Standing Melee Punch animation");
        }
        else
        {
            Debug.LogWarning("Could not find Standing Melee Punch animation");
        }
        
        // Find return state (idle/locomotion)
        AnimatorState returnState = null;
        foreach (var state in stateMachine.states)
        {
            if (state.state == null) continue;
            string stateName = state.state.name.ToLower();
            if (stateName.Contains("idle") || stateName.Contains("blend") || stateName.Contains("locomotion"))
            {
                returnState = state.state;
                Debug.Log($"Found return state: {state.state.name}");
                break;
            }
        }
        
        // Use first state if nothing found
        if (returnState == null && stateMachine.states.Length > 0)
        {
            returnState = stateMachine.states[0].state;
            Debug.Log($"Using first state as return: {returnState.name}");
        }
        
        // Create transition from Any State to Attack
        AnimatorStateTransition anyToAttack = stateMachine.AddAnyStateTransition(attackState);
        anyToAttack.AddCondition(AnimatorConditionMode.If, 0, "Attack");
        anyToAttack.canTransitionToSelf = false;
        anyToAttack.duration = 0.1f;  // Quick entry
        Debug.Log("Created Any State → Attack transition");
        
        // Create transition from Attack back to locomotion
        if (returnState != null)
        {
            AnimatorStateTransition attackToReturn = attackState.AddTransition(returnState);
            attackToReturn.hasExitTime = true;
            attackToReturn.exitTime = 0.9f;  // Near end of animation
            attackToReturn.duration = 0.15f;  // Quick return
            attackToReturn.hasFixedDuration = true;
            Debug.Log($"Created Attack → {returnState.name} transition");
        }
        
        // Save changes
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        
        Debug.Log("✓ Attack animation setup complete!");
        Debug.Log("- Created Attack state with Standing Melee Punch");
        Debug.Log("- Any State → Attack (trigger)");
        Debug.Log("- Attack → Return (exit time)");
    }
}
