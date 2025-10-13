using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public int playerId;
    [SerializeField] private float moveSpeed = 100f;

    // Propiedad pública para obtener o establecer el ID desde fuera
        public int PlayerId
        {
            get => playerId;
            set => playerId = value;
        }

    private void Update()
    {
        HandleMovementInput();
    }

    private void HandleMovementInput()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        Vector3 movement = new Vector3(moveX, moveY, 0f).normalized * moveSpeed * Time.deltaTime;
        transform.Translate(movement, Space.World);
    }

    public void MovePlayer(Vector3 position)
    {
        transform.position = position;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
}
