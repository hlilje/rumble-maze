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

    [SerializeField]
    private float wallTouchScale;

    private PlayerInput playerInput;
    private Rigidbody2D body;

    private Collision2D currentCollision;
    private bool isPlayingTimedCue;

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

        StopCue();
    }

    void FixedUpdate()
    {
        var move = playerInput.Player.Move.ReadValue<Vector2>();
        Vector2 movement = movementSpeed * (body.transform.right * move.x + body.transform.up * move.y);
        body.AddForce(movement);
    }

    void Update()
    {
        var look = playerInput.Player.Look.ReadValue<Vector2>();
        var rotation = -look.x * rotationSensitivity;
        body.SetRotation(body.rotation + rotation);

        if (isPlayingTimedCue)
        {
            // Let the cue play
        }
        else if (IsDraggingAlongWall())
        {
            var cue = CalcCue(currentCollision) * wallTouchScale;
            StartCue(cue);
        }
        else
        {
            StopCue();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contactCount > 0)
        {
            currentCollision = collision;
            var cue = CalcCue(currentCollision);
            StartCoroutine(TriggerCue(cue, cueTime));
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        currentCollision = null;
    }

    private bool IsDraggingAlongWall()
    {
        return currentCollision != null && body.velocity.magnitude >= 0.01;
    }

    private Vector2 CalcCue(Collision2D collision)
    {
        var normal = collision.contacts[0].normal;
        var angle = Vector2.SignedAngle(-normal, body.transform.up);
        var sin = Mathf.Sin(angle * Mathf.Deg2Rad);
        return new Vector2(0.5f - 0.5f * sin, 0.5f + 0.5f * sin);
    }

    private void StartCue(Vector2 cue)
    {
        Gamepad.current.SetMotorSpeeds(cue[0], cue[1]);
    }

    private void StopCue()
    {
        Gamepad.current.SetMotorSpeeds(0.0f, 0.0f);
    }

    private IEnumerator TriggerCue(Vector2 cue, float time)
    {
        Debug.Log("Collision cue: " + cue);

        StartCue(cue);
        isPlayingTimedCue = true;

        yield return new WaitForSeconds(time);

        if (!IsDraggingAlongWall())
        {
            StopCue();
        }

        isPlayingTimedCue = false;
    }
}
