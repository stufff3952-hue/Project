using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Doodlik : MonoBehaviour
{
    public new Rigidbody2D rigidbody;
    public float jumpForce;
    public float speed;

    private Keyboard keyboard;
    private bool canJump = true;
    private float jumpCooldown = 0.2f;
    private bool facingRight = true; // true - смотрит вправо, false - влево

    private void Start()
    {
        keyboard = Keyboard.current;
    }

    private void Update()
    {
        if (keyboard.aKey.isPressed) // A - влево
        {
            transform.Translate(Vector2.left * speed * Time.deltaTime);
            if (facingRight) // Если смотрел вправо, поворачиваем влево
            {
                Flip(false);
            }
        }

        if (keyboard.dKey.isPressed) // D - вправо
        {
            transform.Translate(Vector2.right * speed * Time.deltaTime);
            if (!facingRight) // Если смотрел влево, поворачиваем вправо
            {
                Flip(true);
            }
        }
    }

    // Метод для поворота персонажа
    private void Flip(bool faceRight)
    {
        // Меняем направление взгляда
        facingRight = faceRight;

        // Создаем временный вектор масштаба
        Vector3 theScale = transform.localScale;

        // Меняем scale по X на противоположный (отражаем по вертикали)
        if (faceRight)
        {
            theScale.x = Mathf.Abs(theScale.x); // Положительный scale (вправо)
        }
        else
        {
            theScale.x = -Mathf.Abs(theScale.x); // Отрицательный scale (влево)
        }

        // Применяем новый scale
        transform.localScale = theScale;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Прыгаем только если можем прыгать и столкнулись с чем-то снизу
        if (canJump && IsCollisionFromBelow(collision))
        {
            StartCoroutine(JumpWithCooldown());
        }
    }

    // Проверяем, что столкновение произошло снизу
    private bool IsCollisionFromBelow(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                return true;
            }
        }
        return false;
    }

    // Прыжок с задержкой
    private System.Collections.IEnumerator JumpWithCooldown()
    {
        canJump = false;

        // Обнуляем вертикальную скорость перед прыжком
        rigidbody.linearVelocity = new Vector2(rigidbody.linearVelocity.x, 0);

        // Применяем силу прыжка
        rigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        // Ждем перед следующим прыжком
        yield return new WaitForSeconds(jumpCooldown);

        canJump = true;
    }
}