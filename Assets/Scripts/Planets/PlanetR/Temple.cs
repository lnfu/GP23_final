using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Temple : MonoBehaviour
{
    public Temple otherTemple;
    private Transform playerTF;

    [DisplayOnly] public bool canTeleport;
    [DisplayOnly] public bool startCounting;
    public float teleportTime = 1.5f;
    [DisplayOnly] public float currentTime;
    public float teleportCD = 8f;
    [DisplayOnly] public float currentCD;

    public ParticleSystem teleportParticle;
    public GameObject teleportLight;
    private StageManager _stageManager;
    private TalkManager talkManager;

    // Start is called before the first frame update
    void Start()
    {
        GameObject m = GameObject.FindWithTag("UIManager");
        _stageManager = m.GetComponent<StageManager>();
        talkManager = m.GetComponent<TalkManager>();

        playerTF = GameObject.Find("Player").transform;

        canTeleport = false;
        startCounting = false;
        currentTime = 0f;
        currentCD = teleportCD;
        StopParticle();
    }

    // Update is called once per frame
    void Update()
    {
        if (_stageManager.stage == Stage.Maze) {
            if (canTeleport)
            {
                teleportLight.SetActive(true);
                if (startCounting)
                {
                    currentTime += Time.deltaTime;
                    if (currentTime >= teleportTime)
                    {
                        Teleport();
                        _stageManager.UpdateStage();
                    }
                }
                else {
                    StopParticle();
                }
            }
            else
            {
                teleportLight.SetActive(false);
                StopParticle();
                // currentCD -= Time.deltaTime;
                // if (currentCD <= 0f)
                // {
                //     canTeleport = true;
                //     currentTime = 0f;
                // }
            }
        }
        else {
            teleportLight.SetActive(false);
            StopParticle();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.name == "Player")
        {
            startCounting = true;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.name == "Player")
        {
            if (canTeleport && startCounting)
            {
                GenerateParticle();
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.name == "Player")
        {
            startCounting = false;
            currentTime = 0f;
        }
    }

    private void Teleport()
    {
        currentCD = teleportCD;
        otherTemple.currentCD = teleportCD;
        canTeleport = false;
        otherTemple.canTeleport = false;
        playerTF.position = otherTemple.transform.position;
        playerTF.GetComponent<PlayerController_new>().Lock();
    }

    private void GenerateParticle()
    {
        teleportParticle.Play();
    }

    private void StopParticle()
    {
        teleportParticle.Stop();
    }
}
