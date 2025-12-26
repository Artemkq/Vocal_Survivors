using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    // Ссылки
    Animator am;
    PlayerMovement pm;
    SpriteRenderer sr;

    // Кэшируем ID параметра (это работает быстрее, чем текст "Move")
    static readonly int MoveParam = Animator.StringToHash("Move");

    void Start()
    {
        am = GetComponent<Animator>();
        pm = GetComponent<PlayerMovement>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Проверка состояния игры (оптимизировано)
        if (GameManager.instance == null || GameManager.instance.isGameOver || GameManager.instance.isPaused)
        {
            am.SetBool(MoveParam, false);
            return;
        }

        // Проверяем, движется ли игрок
        bool isMoving = pm.moveDir.sqrMagnitude > 0.01f;
        am.SetBool(MoveParam, isMoving);

        if (isMoving)
        {
            SpriteDirectionChecker();
        }
    }

    void SpriteDirectionChecker()
    {
        // Упрощенная логика поворота спрайта
        if (pm.lastHorizontalVector < 0) sr.flipX = true;
        else if (pm.lastHorizontalVector > 0) sr.flipX = false;
    }

    public void SetAnimatorController(RuntimeAnimatorController c)
    {
        if (am == null) am = GetComponent<Animator>();
        am.runtimeAnimatorController = c;
    }
}
