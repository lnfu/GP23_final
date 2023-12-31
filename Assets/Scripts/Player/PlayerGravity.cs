using UnityEngine;

public class PlayerGravity : MonoBehaviour
{
    public PlanetGravity planetGravity; // see PlanetGravity.cs

    private PlayerController _playerController;

    void Start()
    {
        _playerController = GetComponent<PlayerController>();
    }

    void FixedUpdate()
    {
        if (_playerController.stage == "OnPlanet" || _playerController.stage == "Landing")
            planetGravity.AddGravity(transform);
    }
}