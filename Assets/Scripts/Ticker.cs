using UnityEngine;

public class Ticker : MonoBehaviour
{
    public float tickTime = 0.2f; // do not change (influences character and camera movement)

    private float _tickTimer;

    public delegate void TickAction();
    public static event TickAction OnTickAction;

    private void Update()
    {
        _tickTimer += Time.deltaTime;

        if (_tickTimer >= tickTime) {
            _tickTimer = 0;
            TickEvent();
        }
    }

    private void TickEvent()
    {
        OnTickAction?.Invoke();
    }
}
