using UnityEngine;

public class ClueActivator : MonoBehaviour
{
    [SerializeField] private InteractableClue clue;
    void OnEnable()
    {
        clue.EnableInteractability();
    }

    void OnDisable()
    {
        clue.DisableInteractability();
    }
}
