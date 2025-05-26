using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Toggle
{
    public class SwitchHandler : MonoBehaviour
    {
        
        [Header("Switch")]
        [SerializeField] private GameObject switchBtn;
        [SerializeField] private Image background;
        [SerializeField] private Color switchOnColor;
        [SerializeField] private Color switchOffColor;
        
        [Header("Case")]
        [SerializeField] private Image caseBackground;
        [SerializeField] private Color caseOnColor;
        [SerializeField] private Color caseOffColor;

        [Header("Animation")]
        [SerializeField] private float animationDuration = 0.2f;
        private Clue clue;
        private bool isOn => clue.enabled;

        private void Start()
        {
            UpdateVisuals();
        }

        public void SetClue(Clue clue)
        {
            this.clue = clue;
        }

        public void OnSwitchButtonClicked()
        {
            clue.enabled = !clue.enabled;
            switchBtn.transform.DOLocalMoveX(-switchBtn.transform.localPosition.x, animationDuration);
            UpdateVisuals();
        }
        
        private void UpdateVisuals()
        {
            Color targetSwitchColor = !isOn ? switchOffColor : switchOnColor;
            Color targetCaseColor = !isOn ? caseOffColor : caseOnColor;
            
            background.DOColor(targetSwitchColor, animationDuration);
            caseBackground.DOColor(targetCaseColor, animationDuration);
        }
    }
}