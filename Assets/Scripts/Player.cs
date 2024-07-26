using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    struct Cue
    {
        public Vector2 orientation;
        public Vector2 intensity;

        public override readonly string ToString()
        {
            return "Orientation: " + orientation + " Intensity: " + intensity;
        }
    }

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
    private AudioSource audioSource;

    private List<ContactPoint2D> currentCollisions;
    private bool isPlayingCue;
    private bool isPlayingTimedCue;

    void Awake()
    {
        playerInput = new PlayerInput();
        body = GetComponent<Rigidbody2D>();
        audioSource = CreateCueAudioSource();
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
        var movement = movementSpeed * (body.transform.right * move.x + body.transform.up * move.y);
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
            var cue = CalcCue(currentCollisions.LastOrDefault(), wallTouchScale);
            PlayCue(cue);
        }
        else if (isPlayingCue)
        {
            StopCue();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contactCount > 0)
        {
            currentCollisions.Add(collision.GetContact(0));
            var cue = CalcCue(currentCollisions.Last(), 1.0f);
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
        return currentCollisions.Count > 0 && body.velocity.magnitude >= 0.01f;
    }

    private Cue CalcCue(ContactPoint2D contactPoint, float intensity)
    {
        var normal = contactPoint.normal;
        var angle = Vector2.SignedAngle(-normal, body.transform.up);
        var sin = Mathf.Sin(angle * Mathf.Deg2Rad);

        Cue cue;
        cue.orientation = new Vector2(0.5f - 0.5f * sin, 0.5f + 0.5f * sin);
        cue.intensity = cue.orientation * intensity;

        return cue;
    }

    private void PlayCue(Cue cue)
    {
        Gamepad.current?.SetMotorSpeeds(cue.intensity[0], cue.intensity[1]);
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
        audioSource.volume = cue.intensity[1] + cue.intensity[0];
        audioSource.panStereo = cue.orientation[1] - cue.orientation[0];

        isPlayingCue = true;
        Debug.Log("Playing cue: " + cue);
    }

    private void StopCue()
    {
        Gamepad.current?.SetMotorSpeeds(0.0f, 0.0f);
        audioSource.Stop();

        isPlayingCue = false;
        Debug.Log("Stopping cue");
    }

    private IEnumerator TriggerCue(Cue cue, float time)
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

    private AudioSource CreateCueAudioSource()
    {
        var SAMPLE_FREQUENCY = 44100;
        var SAMPLE_LENGTH = SAMPLE_FREQUENCY;
        var SINE_SAMPLE_LENGTH = 200;
        var audioData = new float[SAMPLE_LENGTH];
        for (int i = 0; i < SAMPLE_LENGTH; ++i)
        {
            audioData[i] = Mathf.Sin((float)(i % SINE_SAMPLE_LENGTH) / (float)SINE_SAMPLE_LENGTH * Mathf.PI * 2);
        }

        var audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = AudioClip.Create("Cue", SAMPLE_LENGTH, 1, SAMPLE_FREQUENCY, false);
        audioSource.clip.SetData(audioData, 0);
        audioSource.loop = true;

        return audioSource;
    }
}
