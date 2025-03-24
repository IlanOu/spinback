using UnityEngine;
using UnityEngine.InputSystem;

public class Test : MonoBehaviour
{
    public PlayerActions playerActions;

    private InputAction startGame;
    
    private void Awake()
    {
        playerActions = new PlayerActions();
    }

    private void OnEnable()
    {
        startGame = playerActions.Controls.StartDeck1;
        startGame.Enable();
        startGame.performed += StartTheGame;
    }
    
    private void OnDisable()
    {
        startGame.performed -= StartTheGame;
        startGame.Disable();
    }

    private void StartTheGame(InputAction.CallbackContext callbackContext)
    {
        Debug.Log("Start the game !");
    }
}
