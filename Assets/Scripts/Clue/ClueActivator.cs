using UnityEngine;

public class ClueActivator : MonoBehaviour
{
    [SerializeField] private InteractableClue clue;
    [SerializeField] private ClueInteractiveIcon interactiveIcon;
    void OnEnable()
    {
        clue.EnableInteractability();
        if (interactiveIcon != null)
            interactiveIcon.EnableInteractive();
    }

    void OnDisable()
    {
        clue.DisableInteractability();
        if (interactiveIcon != null)
            interactiveIcon.DisableInteractive();
    }
}
