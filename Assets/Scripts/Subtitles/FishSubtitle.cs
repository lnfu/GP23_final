using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FishSubtitle : Subtitle
{
    public PreviewPlanet preview;
    private StageManager _stageManager;

    // Start is called before the first frame update
    void Start()
    {
        GameObject m = GameObject.FindWithTag("UIManager");
        _stageManager = m.GetComponent<StageManager>();

        player = GameObject.Find("Player").GetComponent<PlayerController_new>();
        talkManager = GameObject.FindWithTag("UIManager").GetComponent<TalkManager>();
        
        canvasGroup = canvas.GetComponent<CanvasGroup>();
        textArea = canvas.transform.Find("SubtitleText").GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        Talk();
    }

    public override void Talk()
    {
        base.Talk();
    }

    public override IEnumerator ShowSubtitle(List<string> subtitles)
    {
        player.Lock();
        player.Freeze();
        _stageManager.UpdateStage();

        gameObject.GetComponent<Fish>().Lock();
        canvas.isLockingSubtitle = true;
        canvas.isTalking = true;

        float showCharTime = 1f / talkManager.charPerSec;
        for (int i = 0; i < subtitles.Count; i++)
        {
            string[] nameAndWord = subtitles[i].Split(": ");
            string dispText = nameAndWord[0] + ": ";

            isEnterDown = false;
            StartCoroutine(WaitForSkip());

            foreach (char c in nameAndWord[1])
            {
                if (isEnterDown) {
                    isEnterDown = false;
                    dispText = subtitles[i];
                    textArea.text = dispText;
                    break;
                }
                
                dispText += c;
                textArea.text = dispText;
                yield return new WaitForSeconds(showCharTime);
            }

            // yield return new WaitForSeconds(talkManager.delayTime);
            yield return null;
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return));
            yield return null;
            textArea.text = "";
        }

        preview.PlayMazePlanetPreview();

        canvas.isTalking = false;
        canvas.isLockingSubtitle = false;
        player.Unlock();
        player.Unfreeze();
        gameObject.GetComponent<Fish>().Unlock();
    }
}
