using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("Ссылки")]
    [Tooltip("Ссылка на Inventory, чтобы брать текущий вес")]
    public Inventory inventory;

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

    // Направление ввода (нормализованный вектор)
    private Vector2 inputDirection;

    // Текущее «целевое» значение скорости (будет пересчитываться в Update())
    private float currentSpeed;
    // Текущее «целевое» ускорение (будет пересчитываться в Update())
    private float currentAccel;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (inventory == null)
            Debug.LogError("PlayerMovement2D: не задан Inventory-компонент в инспекторе.");
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
    }

    void FixedUpdate()
    {
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
        Debug.Log($"newVelocity = {newVelocity}");
        rb.linearVelocity = newVelocity;
    }
}
