using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D : MonoBehaviour
{
    public bool canMove = true;
    [Header("Ссылки")]
    [Tooltip("Ссылка на Inventory, чтобы брать текущий вес")]
    public Inventory inventory;
    [Tooltip("Ссылка на camera")]
    public Camera cam;
    //[Tooltip("Ссылка на body")]
    //public Rigidbody2D rb;


    [Header("Скорость и инерция")]
    [Tooltip("Максимальная скорость (единиц/сек) при весе ≤ weightNoPenaltyKg")]
    public float maxSpeed = 5f;
    [Tooltip("Минимальная скорость (единиц/сек) при весе ≥ weightMaxDebuffKg")]
    public float minSpeed = 0.5f;

    [Tooltip("Максимальное ускорение (единиц/сек²) при весе ≤ weightNoPenaltyKg")]
    public float maxAcceleration = 20f;
    [Tooltip("Минимальное ускорение (единиц/сек²) при весе ≥ weightMaxDebuffKg")]
    public float minAcceleration = 2f;

    [Tooltip("Порог веса (в кг), до которого нет штрафа")]
    public float weightNoPenaltyKg = 30f;
    [Tooltip("Порог веса (в кг), при котором скорость сводится к minSpeed")]
    public float weightMaxDebuffKg = 80f;

    private Rigidbody2D rb;

    public float CurrentSpeed => rb.linearVelocity.magnitude;
    public float MaxSpeed => currentSpeed;
    public float RelativeSpeed => Mathf.Clamp01(CurrentSpeed / Mathf.Max(MaxSpeed, 0.01f));
    public bool IsMoving => CurrentSpeed > 0.1f;
    // Направление ввода (нормализованный вектор)
    public Vector2 inputDirection;

    // Текущее «целевое» значение скорости (будет пересчитываться в Update())
    public float currentSpeed;
    // Текущее «целевое» ускорение (будет пересчитываться в Update())
    public float currentAccel;



    // mouse
    Vector2 mousePos;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (inventory == null)
            Debug.LogError("PlayerMovement2D: не задан Inventory-компонент в инспекторе.");
    }

    public void SetCanMove(bool canMove)
    {
        this.canMove = canMove;
    }

    void Update()
    {
        // Если инвентарь открыт → обнуляем ввод, чтобы персонаж начал постепенно тормозить
        if (inventory.backGround.activeSelf)
        {
            inputDirection = Vector2.zero;
        }
        else
        {
            // Считываем ввод от игрока
            float h = Input.GetAxisRaw("Horizontal"); // A/D, ←/→
            float v = Input.GetAxisRaw("Vertical");   // W/S, ↑/↓
            inputDirection = new Vector2(h, v).normalized;
        }

        // Каждую секунду (каждый кадр в Update) пересчитываем, 
        // какую скорость и ускорение должен иметь персонаж, исходя из веса
        float weightKg = inventory.GetTotalWeightInKg();

        if (weightKg <= weightNoPenaltyKg)
        {
            currentSpeed = maxSpeed;
            currentAccel = maxAcceleration;
        }
        else if (weightKg >= weightMaxDebuffKg)
        {
            currentSpeed = minSpeed;
            currentAccel = minAcceleration;
        }
        else
        {
            // Линейно интерполируем между (maxSpeed, maxAcceleration) и (minSpeed, minAcceleration)
            float t = (weightKg - weightNoPenaltyKg) / (weightMaxDebuffKg - weightNoPenaltyKg);
            currentSpeed = Mathf.Lerp(maxSpeed, minSpeed, t);
            currentAccel = Mathf.Lerp(maxAcceleration, minAcceleration, t);
        }


        // mouse
        //Input.mousePosition();
        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

    }

    void FixedUpdate()
    {
        Vector2 lookDir = mousePos - rb.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        rb.rotation = angle;
        /*
        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero; // мгновенно останавливаем движение
            return;
        }
        */
        if (!canMove)
        {
            currentSpeed = minSpeed;
        }



        // Применяем движение через Rigidbody2D:
        //   targetVelocity — желаемая скорость (Vector2)
        //   rb.velocity    — текущая скорость (Vector2)
        //
        // Если inputDirection == Vector2.zero, то targetVelocity = Vector2.zero,
        // и MoveTowards плавно уменьшит rb.velocity до нуля с шагом currentAccel * Time.fixedDeltaTime.

            Vector2 targetVelocity = inputDirection * currentSpeed;
        Vector2 newVelocity = Vector2.MoveTowards(rb.linearVelocity,
                                                  targetVelocity,
                                                  currentAccel * Time.fixedDeltaTime);
        ///Debug.Log($"newVelocity = {newVelocity}");
        rb.linearVelocity = newVelocity;
    }
}
