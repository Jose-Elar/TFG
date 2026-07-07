using UnityEngine;

public class HpManager : MonoBehaviour
{
    [Header("HP Bar Images")]
    [SerializeField] private GameObject hp1;
    [SerializeField] private GameObject hp2;
    [SerializeField] private GameObject hp3;

    [Header("References")]
    [SerializeField] private DroneHealth droneHealth;

    private int _lastHp = -1;
    int currentHp = 3;

    void Update()
    {
        currentHp = droneHealth.getCurrentHp();

        if (currentHp == _lastHp) return;
        _lastHp = currentHp;

        hp1.SetActive(currentHp >= 1);
        hp2.SetActive(currentHp >= 2);
        hp3.SetActive(currentHp >= 3);
    }
}