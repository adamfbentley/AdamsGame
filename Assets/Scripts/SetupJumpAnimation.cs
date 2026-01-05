using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

/// <summary>
/// One-time script to clean up and properly configure jump animation
/// Run this from Tools menu after attaching to GameObject
/// </summary>
public class SetupJumpAnimation : MonoBehaviour
{
    [MenuItem("Tools/Setup Jump Animation")]
    static void SetupJump()
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
        
        // Remove any existing jump states - collect them first to avoid modification during iteration
        var statesToRemove = new System.Collections.Generic.List<AnimatorState>();
        foreach (var state in stateMachine.states)
        {
            if (state.state.name.ToLower().Contains("jump"))
            {
                statesToRemove.Add(state.state);
            }
        }
        foreach (var state in statesToRemove)
        {
            stateMachine.RemoveState(state);
        }
        
        // Remove JumpSpeed parameter if it exists
        foreach (var param in controller.parameters)
        {
            if (param.name == "JumpSpeed")
            {
                controller.RemoveParameter(param);
            }
        }
        
        // Add Grounded parameter if it doesn't exist
        bool hasGrounded = false;
        foreach (var param in controller.parameters)
        {
            if (param.name == "Grounded")
            {
                hasGrounded = true;
                break;
            }
        }
        if (!hasGrounded)
        {
            controller.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
            Debug.Log("Added Grounded parameter");
        }
        
        // Create a simple Jump state
        AnimatorState jumpState = stateMachine.AddState("Jump", new Vector3(300, 100, 0));
        
        // Try to load Jump (2) - the stationary jump
        AnimationClip jumpClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Vampire A Lusth@Jump (2).fbx");
        if (jumpClip != null)
        {
            jumpState.motion = jumpClip;
            jumpState.speed = 1.0f;  // Normal speed
            Debug.Log("Using Jump (2) animation (normal speed) - stationary jump");
        }
        else
        {
            Debug.LogWarning("Could not find Jump (2) animation clip");
        }
        
        // Find all locomotion states - check for null to avoid destroyed object errors
        AnimatorState idleState = null;
        AnimatorState walkState = null;
        AnimatorState runState = null;
        
        foreach (var state in stateMachine.states)
        {
            if (state.state == null) continue;  // Skip destroyed states
            
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
        
        // Create transition from Any State to Jump
        AnimatorStateTransition anyToJump = stateMachine.AddAnyStateTransition(jumpState);
        anyToJump.AddCondition(AnimatorConditionMode.If, 0, "Jump");
        anyToJump.canTransitionToSelf = false;
        anyToJump.duration = 0.3f;  // Smooth blend into jump
        Debug.Log("Created Any State → Jump transition (0.3s blend)");
        
        // Create transitions from Jump to all movement states
        // Create transition to idle (when Speed is low)
        if (idleState != null)
        {
            AnimatorStateTransition jumpToIdle = jumpState.AddTransition(idleState);
            jumpToIdle.hasExitTime = false;
            jumpToIdle.exitTime = 0f;
            jumpToIdle.duration = 0.05f;  // Near-instant transition
            jumpToIdle.offset = 0f;
            jumpToIdle.AddCondition(AnimatorConditionMode.If, 0, "Grounded");
            jumpToIdle.AddCondition(AnimatorConditionMode.Less, 0.5f, "Speed");
            Debug.Log("Created Jump → Idle transition (when Speed < 0.5, instant)");
        }
        
        // Create transition to walk (when Speed is medium)
        if (walkState != null)
        {
            AnimatorStateTransition jumpToWalk = jumpState.AddTransition(walkState);
            jumpToWalk.hasExitTime = false;
            jumpToWalk.exitTime = 0f;
            jumpToWalk.duration = 0.05f;  // Near-instant transition
            jumpToWalk.offset = 0f;
            jumpToWalk.AddCondition(AnimatorConditionMode.If, 0, "Grounded");
            jumpToWalk.AddCondition(AnimatorConditionMode.Greater, 0.5f, "Speed");
            jumpToWalk.AddCondition(AnimatorConditionMode.Less, 5f, "Speed");
            Debug.Log("Created Jump → Walk transition (when 0.5 < Speed < 5, instant)");
        }
        
        // Create transition to run (when Speed is high)
        if (runState != null)
        {
            AnimatorStateTransition jumpToRun = jumpState.AddTransition(runState);
            jumpToRun.hasExitTime = false;
            jumpToRun.exitTime = 0f;
            jumpToRun.duration = 0.05f;  // Near-instant transition
            jumpToRun.offset = 0f;
            jumpToRun.AddCondition(AnimatorConditionMode.If, 0, "Grounded");
            jumpToRun.AddCondition(AnimatorConditionMode.Greater, 5f, "Speed");
            Debug.Log("Created Jump → Run transition (when Speed > 5, instant)");
        }
        
        // Save changes
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        
        Debug.Log("✓ Jump animation setup complete!");
        Debug.Log("- Created Jump state with Vampire A Lusth@Jump animation");
        Debug.Log("- Any State → Jump (trigger)");
        Debug.Log("- Jump → Idle (exit time 0.9)");
    }
}
