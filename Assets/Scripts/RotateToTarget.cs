using UnityEngine;

public class RotateToTarget : MonoBehaviour
{
    public float rotationSpeed;
    private Vector2 direction;

    public float moveSpeed;

    // Update is called once per frame
    void Update()
    {
        direction = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        if (direction != Vector2.zero) {
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
        }

        Vector2 cursorPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = Vector2.MoveTowards(transform.position, cursorPosition, moveSpeed * Time.deltaTime);
    }
}
