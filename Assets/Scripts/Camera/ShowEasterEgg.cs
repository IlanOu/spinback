using System;
using DG.Tweening;
using UnityEngine;

public class ShowEasterEgg: MonoBehaviour
{
    [SerializeField] private GameObject _easterEgg;
    private bool _isEasterEggActive;
    private GameObject easterEggGO;
    [SerializeField] private GameObject parent;
    
    private bool _loadPressed = false;
    private bool _vinylPressed = false;
    
    void Start()
    {
        // MidiBinding.Instance.Subscribe(MidiBind.SHIFTED_BUTTON_4_CUE_1, DisplayEasterEgg);
        // MidiBinding.Instance.Subscribe(MidiBind.SHIFTED_BUTTON_4_ROLL_1, DisplayEasterEgg);
        MidiBinding.Instance.Subscribe(MidiBind.LOAD_BUTTON_LONG_PRESS_1, f => {_loadPressed = f == 1f;});
        MidiBinding.Instance.Subscribe(MidiBind.VINYL_BUTTON_1, f => {_vinylPressed = f == 1f;});
    }

    private void Update()
    {
        // on keyboard press
        if (Input.GetKeyDown(KeyCode.O))
        {
            DisplayEasterEgg();
        }
        
        if (_loadPressed && _vinylPressed)
        {
            DisplayEasterEgg();
            _loadPressed = false;
            _vinylPressed = false;
        }
    }

    void DisplayEasterEgg()
    {
        
        if (_isEasterEggActive)
        {
            if (easterEggGO != null)
            {
                // Animation de retour avant destruction
                easterEggGO.transform.DOLocalRotate(new Vector3(90, 0, 0), 0.5f).OnComplete(() => {
                    Destroy(easterEggGO);
                    _isEasterEggActive = false;
                });
            }
            else
            {
                _isEasterEggActive = false;
            }
            return;
        }
        
        // instantiate _easterEgg prefab
        easterEggGO = Instantiate(_easterEgg, parent.transform);
        // Réinitialiser la rotation locale à zéro d'abord
        easterEggGO.transform.localRotation = Quaternion.identity;
        // Puis appliquer la rotation souhaitée (en local puisque c'est un enfant)
        easterEggGO.transform.localRotation = Quaternion.Euler(90, 0, 0);
        easterEggGO.SetActive(true);
        
        AnimateEasterEgg();
        _isEasterEggActive = true;
    }
    
    void AnimateEasterEgg()
    {
        // Animer l'instance créée, pas le prefab original
        easterEggGO.transform.DOLocalRotate(Vector3.zero, 0.5f);
    }
}