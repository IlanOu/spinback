using System.Linq;
using NPC.NPCAnimations;
using UnityEngine;

[System.Serializable]
public class NPCAnimationConfig
{
    public NPCAnimationsType animationType; // Ex: Walk, Dance, Talk
    public string animatorParameterName;   // Ex: "Walk", "Dance", "IsTalking"
    public bool useTriggerInsteadOfBool;   // Si true, SetTrigger() au lieu de SetBool()
}

public class NPCAnimationManager : MonoBehaviour
{
    public NPCAnimationConfig[] animationConfigs; // Tableau configurable dans l'Inspector
    
    [SerializeField]
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator manquant !");
        }

        // Vérifie que chaque config a un paramètre valide dans l'Animator
        foreach (var config in animationConfigs)
        {
            bool parameterExists = false;
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == config.animatorParameterName)
                {
                    parameterExists = true;
                    break;
                }
            }
            if (!parameterExists)
            {
                Debug.LogError($"Paramètre Animator manquant : {config.animatorParameterName}");
            }
        }
    }

    public void PlayAnimation(NPCAnimationsType animationType, bool value = true)
    {
        // Trouve la config correspondant à l'animation demandée
        NPCAnimationConfig config = animationConfigs.FirstOrDefault(c => c.animationType == animationType);
        if (config == null)
        {
            Debug.LogError($"Animation {animationType} non configurée dans NPCAnimationManager !");
            return;
        }

        // Joue l'animation
        if (config.useTriggerInsteadOfBool)
        {
            animator.SetTrigger(config.animatorParameterName); // Ex: Dance
        }
        else
        {
            animator.SetBool(config.animatorParameterName, value); // Ex: Walk = true/false
        }
    }

    // Optionnel : méthode rapide pour arrêter proprement
    public void StopAnimation(NPCAnimationsType animationType)
    {
        NPCAnimationConfig config = animationConfigs.FirstOrDefault(c => c.animationType == animationType);
        if (config != null && !config.useTriggerInsteadOfBool)
        {
            animator.SetBool(config.animatorParameterName, false); // Ex: Walk = false
        }
    }
}