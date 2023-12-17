using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishSubtitle : Subtitle
{
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player").GetComponent<PlayerController_new>();
        talkManager = GameObject.FindWithTag("UIManager").GetComponent<TalkManager>();
        generator = subtitleArea.GetComponent<SubtitleGenerator>();
    }

    // Update is called once per frame
    void Update()
    {
        Talk();
    }

    public override void Talk()
    {
        if (IsPlayerInRange(talkRange)) {
            if (talkManager.currentSubtitle == 1) {
                if (!generator.isUsingSubtitle) {
                    StartCoroutine(ShowSubtitle(talkManager.subtitles[talkManager.currentSubtitle]));
                    talkManager.currentSubtitle += 1;
                }
            }
        }
    }

    public override IEnumerator ShowSubtitle(List<string> subtitles)
    {
        return base.ShowSubtitle(subtitles);
    }
}