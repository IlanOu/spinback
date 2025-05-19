using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [Header("Paramètres du curseur")]
    [SerializeField] private bool hideCursorOnStart = true;
    [SerializeField] private KeyCode toggleCursorKey = KeyCode.Tab;
    [SerializeField] private KeyCode showCursorKey = KeyCode.Escape;
    [SerializeField] private bool hideOnClick = true;
    
    public delegate void CursorStateChanged(bool isHidden);
    public event CursorStateChanged OnCursorStateChanged;

    private bool cursorWasVisible;
    private CursorLockMode previousLockMode;
    private bool cursorHidden;
    private bool temporarilyDisableHideOnClick = false;

    void Start()
    {
        cursorWasVisible = Cursor.visible;
        previousLockMode = Cursor.lockState;
        
        cursorHidden = hideCursorOnStart;
        UpdateCursorState();
    }

    void OnEnable()
    {
        UpdateCursorState();
    }

    void OnDisable()
    {
        Cursor.visible = cursorWasVisible;
        Cursor.lockState = previousLockMode;
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleCursorKey))
        {
            cursorHidden = !cursorHidden;
            UpdateCursorState();
        }
        
        if (cursorHidden && Input.GetKeyDown(showCursorKey))
        {
            cursorHidden = false;
            UpdateCursorState();
        }
        
        if (!temporarilyDisableHideOnClick && !cursorHidden && hideOnClick && 
            (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)))
        {
            cursorHidden = true;
            UpdateCursorState();
        }
    }
    
    private void UpdateCursorState()
    {
        if (cursorHidden)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        
        OnCursorStateChanged?.Invoke(cursorHidden);
    }
    
    public void SetCursorHidden(bool hidden)
    {
        cursorHidden = hidden;
        UpdateCursorState();
    }
    
    public bool IsCursorHidden()
    {
        return cursorHidden;
    }
    
    public void SetHideOnClickEnabled(bool enabled)
    {
        temporarilyDisableHideOnClick = !enabled;
    }
}
