using UnityEngine;

public class Ticker : MonoBehaviour
{
    public static float tickTime = 0.5f; // do not change (influences character and camera movement)

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
