using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float movementSpeed;

    [SerializeField]
    private float rotationSensitivity;

    [SerializeField]
    private float cueTime;

    private PlayerInput playerInput;
    private Rigidbody2D body;

    void Awake()
    {
        playerInput = new PlayerInput();
        body = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        Cursor.visible = false;
    }

    void OnEnable()
    {
        playerInput.Player.Enable();
    }

    void OnDisable()
    {
        playerInput.Player.Disable();
    }

    void FixedUpdate()
    {
        var move = playerInput.Player.Move.ReadValue<Vector2>();
        Vector2 movement = movementSpeed * Time.fixedDeltaTime * (body.transform.right * move.x + body.transform.up * move.y);
        body.MovePosition(body.position + movement);
    }

    void Update()
    {
        var look = playerInput.Player.Look.ReadValue<Vector2>();
        var rotation = -look.x * rotationSensitivity;
        body.SetRotation(body.rotation + rotation);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contactCount > 0)
        {
            var normal = collision.contacts[0].normal;
            var angle = Vector2.SignedAngle(-normal, body.transform.up);
            var sin = Mathf.Sin(angle * Mathf.Deg2Rad);
            var cue = new Vector2(0.5f - 0.5f * sin, 0.5f + 0.5f * sin);
            StartCoroutine(TriggerCue(cue, cueTime));
        }
    }

    private IEnumerator TriggerCue(Vector2 cue, float time)
    {
        Debug.Log("Collision cue: " + cue);
        Gamepad.current.SetMotorSpeeds(cue[0], cue[1]);
        yield return new WaitForSeconds(time);
        Gamepad.current.SetMotorSpeeds(0.0f, 0.0f);
    }
}
