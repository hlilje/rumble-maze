using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    private List<ContactPoint2D> currentCollisions;
    private bool isPlayingTimedCue;

    void Awake()
    {
        playerInput = new PlayerInput();
        body = GetComponent<Rigidbody2D>();
        currentCollisions = new List<ContactPoint2D>();
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
            var cue = CalcCue(currentCollisions.LastOrDefault()) * wallTouchScale;
            PlayCue(cue);
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
            currentCollisions.Add(collision.GetContact(0));
            var cue = CalcCue(currentCollisions.Last());
            StartCoroutine(TriggerCue(cue, cueTime));
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        // Not entirely correct but good enough
        if (currentCollisions.Count > 0)
        {
            currentCollisions.RemoveAt(0);
        }
    }

    private bool IsDraggingAlongWall()
    {
        return currentCollisions.Count > 0 && body.velocity.magnitude >= 0.01;
    }

    private Vector2 CalcCue(ContactPoint2D contactPoint)
    {
        var normal = contactPoint.normal;
        var angle = Vector2.SignedAngle(-normal, body.transform.up);
        var sin = Mathf.Sin(angle * Mathf.Deg2Rad);
        return new Vector2(0.5f - 0.5f * sin, 0.5f + 0.5f * sin);
    }

    private void PlayCue(Vector2 cue)
    {
        Gamepad.current?.SetMotorSpeeds(cue[0], cue[1]);
        if (cue.magnitude >= 0.01)
        {
            Debug.Log("Playing cue: " + cue);
        }
    }

    private void StopCue()
    {
        Gamepad.current?.SetMotorSpeeds(0.0f, 0.0f);
    }

    private IEnumerator TriggerCue(Vector2 cue, float time)
    {
        PlayCue(cue);
        isPlayingTimedCue = true;

        yield return new WaitForSeconds(time);

        if (!IsDraggingAlongWall())
        {
            StopCue();
        }

        isPlayingTimedCue = false;
    }
}
