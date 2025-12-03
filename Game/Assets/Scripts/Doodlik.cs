using UnityEngine;
using UnityEngine.InputSystem;

public class Doodlik : MonoBehaviour
{
    [Header("НАСТРОЙКИ")]
    public float moveSpeed = 5f;
    public float jumpForce = 14f;
    public float jumpCooldown = 0.1f;

    [Header("КОМПОНЕНТЫ")]
    public Rigidbody2D rb;
    private Keyboard keyboard;
    private float lastJumpTime;
    private bool canJump = true;
    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 3f;
            rb.freezeRotation = true;
        }
        // Клавиатура
        keyboard = Keyboard.current;
    }
    void Update()
    {
        // Движение
        float moveInput = 0f;
        if (keyboard.aKey.isPressed) moveInput = -1f;
        if (keyboard.dKey.isPressed) moveInput = 1f;
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        if (moveInput > 0.1f)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (moveInput < -0.1f)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        if (Time.time - lastJumpTime > jumpCooldown)
        {
            canJump = true;
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform") && canJump)
        {
            bool isFromBelow = false;
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    isFromBelow = true;
                    break;
                }
            }

            if (isFromBelow)
            {
                Jump();
                lastJumpTime = Time.time;
                canJump = false;
            }
        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);

        // Прыжок
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        // Добавляем очки
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(10);
        }
    }
}