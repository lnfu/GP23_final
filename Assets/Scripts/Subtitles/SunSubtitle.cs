using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SunSubtitle : Subtitle
{
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player").GetComponent<PlayerController_new>();
        talkManager = GameObject.FindWithTag("UIManager").GetComponent<TalkManager>();
        
        canvasGroup = canvas.GetComponent<CanvasGroup>();
        textArea = canvas.transform.Find("SubtitleText").GetComponent<TextMeshProUGUI>();
        enterSkipHint = canvas.transform.Find("Images").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.name == "Player") {
            Talk();
        }
    }

    public override void Talk()
    {
        if (!talkManager.sunBool) {
            talkManager.sunBool = true;
            if (!canvas.isLockingSubtitle) {
                StartCoroutine(ShowSubtitle(talkManager.sunSubtitle));
            }
        }
    }

    public override IEnumerator ShowSubtitle(List<string> subtitles)
    {
        canvas.isLockingSubtitle = true;

        canvas.isTalking = true;
        textArea.text = "";

        string dispText = "";
        string richText = "";
        bool recording = false;

        enterSkipHint.SetActive(false);
        yield return FadeSubtitleCanvas(0, 1f, 1f);

        float showCharTime = 1f / talkManager.charPerSec;
        for (int i = 0; i < subtitles.Count; i++) 
        {
            dispText = "";
            textArea.text = "";
            isEnterDown = false;
            StartCoroutine(WaitForSkip());
            enterSkipHint.SetActive(false);

            foreach (char c in subtitles[i]) {
                if (isEnterDown) {
                    dispText = subtitles[i];
                    textArea.text = dispText;
                    break;
                }
                
                if (c == '<') {
                    recording = true;
                }
                else if (c == '>') {
                    recording = false;
                }

                if (recording) {
                    richText += c;
                    continue;
                }
                else {
                    if (c == '>') {
                        dispText += richText + c;
                        richText = "";
                        textArea.text = dispText;
                    }
                    else {
                        dispText += c;
                        textArea.text = dispText;
                        if (c != ' ') yield return new WaitForSeconds(showCharTime);
                    }
                }
            }

            yield return new WaitForSeconds(talkManager.delayTime);
            // yield return null;
            // yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return));
            // yield return null;
        }

        yield return FadeSubtitleCanvas(1f, 0, 1f);

        canvas.isTalking = false;
        canvas.isLockingSubtitle = false;
    }
}
