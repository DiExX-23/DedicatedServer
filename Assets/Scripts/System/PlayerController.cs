using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public int playerId;
    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody rb;
    private Vector3 movement;

    public int PlayerId
    {
        get => playerId;
        set => playerId = value;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Evita rotaciones no deseadas
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // Mejora la detección de colisiones a alta velocidad
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // Desactiva gravedad si no la necesitas
        rb.useGravity = false;
    }

    private void Update()
    {
        HandleMovementInput();
    }

    private void FixedUpdate()
    {
        // Movimiento estable sin atravesar paredes
        rb.velocity = movement * moveSpeed;
    }

    private void HandleMovementInput()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        movement = new Vector3(moveX, moveY, 0f).normalized;
    }

    public void MovePlayer(Vector3 position)
    {
        rb.position = position;
    }

    public Vector3 GetPosition()
    {
        return rb.position;
    }
}
