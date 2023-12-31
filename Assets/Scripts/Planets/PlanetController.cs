using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetController : MonoBehaviour
{
    private StageManager _stageManager;

    public GameObject originPlanet; // O
    public GameObject catPlanet; // cat
    public GameObject waterPlanet;
    public GameObject mazePlanet;
    public GameObject dragonPlanet;


    void Start()
    {
        _stageManager = GameObject.FindWithTag("UIManager").GetComponent<StageManager>();
    }

    private void Update()
    {

        if (_stageManager != null)
        {
            if (_stageManager.stage == Stage.Dragon) {
                originPlanet.SetActive(false);
                catPlanet.SetActive(false);
                waterPlanet.SetActive(false);
                mazePlanet.SetActive(false);
            }
            else
            {
                if (_stageManager.stage >= Stage.Stele)
                {
                    catPlanet.SetActive(true);
                }
                else
                {
                    catPlanet.SetActive(false);
                }

                if (_stageManager.stage >= Stage.ToWaterPlanet)
                {
                    waterPlanet.SetActive(true);
                }
                else
                {
                    waterPlanet.SetActive(false);
                }

                if (_stageManager.stage >= Stage.Water)
                {
                    mazePlanet.SetActive(true);
                }
                else
                {
                    mazePlanet.SetActive(false);
                }

                if (_stageManager.stage >= Stage.ToDragonPlanet)
                {
                    dragonPlanet.SetActive(true);
                }
                else
                {
                    dragonPlanet.SetActive(false);
                }
            }


            
        }

    }

}