using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PopupUI : MonoBehaviour
{
    public static PopupUI Instance { get; private set; }

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float duration = 4f;
    [SerializeField] private float fadeDuration = 0.5f;

    private readonly Queue<string> messageQueue = new Queue<string>();
    private bool isShowing = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void Show(string message)
    {
        messageQueue.Enqueue(message);
        if (!isShowing)
            StartCoroutine(ProcessQueue());
    }

    private IEnumerator ProcessQueue()
    {
        while (messageQueue.Count > 0)
        {
            isShowing = true;
            string nextMessage = messageQueue.Dequeue();
            text.text = nextMessage;

            yield return StartCoroutine(Fade(0, 1, fadeDuration));
            yield return new WaitForSeconds(duration);
            yield return StartCoroutine(Fade(1, 0, fadeDuration));
        }

        isShowing = false;
    }

    private IEnumerator Fade(float start, float end, float time)
    {
        float elapsed = 0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        while (elapsed < time)
        {
            canvasGroup.alpha = Mathf.Lerp(start, end, elapsed / time);
            elapsed += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = end;

        if (end == 0)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}