using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

class Player : MonoBehaviour
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
    float movementSpeed;

    [SerializeField]
    float rotationSensitivity;

    [SerializeField]
    float cueTime;

    [SerializeField]
    float restartTime;

    [SerializeField]
    float wallTouchScale;

    CameraManager cameraManager;
    GameObject winArea;

    PlayerInput playerInput;
    Rigidbody2D body;
    AudioSource audioSource;

    List<(GameObject, ContactPoint2D)> currentCollisions;

    bool isPlayingCue;
    bool isPlayingTimedCue;

    void Awake()
    {
        playerInput = new PlayerInput();
        body = GetComponent<Rigidbody2D>();
        audioSource = CreateCueAudioSource();
        currentCollisions = new List<(GameObject, ContactPoint2D)>();
    }

    void Start()
    {
        cameraManager = GameObject.Find("CameraManager").GetComponent<CameraManager>();
        winArea = GameObject.Find("WinArea");
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
            var cue = CalcCue(currentCollisions.LastOrDefault().Item2, wallTouchScale);
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
            currentCollisions.Add((collision.gameObject, collision.GetContact(0)));
            var playCue = currentCollisions.Count() == 1;
            if (!playCue)
            {
                var currentCollision = currentCollisions.Last().Item1.transform.position;
                var previousCollision = currentCollisions[currentCollisions.Count() - 2].Item1.transform.position;
                // Avoid triggering cues when transitioning over block boundaries along a straight wall
                playCue = currentCollision.x != previousCollision.x && currentCollision.y != previousCollision.y;
            }
            if (playCue)
            {
                var cue = CalcCue(currentCollisions.Last().Item2, 1.0f);
                StartCoroutine(TriggerCue(cue, cueTime));
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        currentCollisions.Remove(currentCollisions.Single(x => x.Item1 == collision.gameObject));
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject == winArea)
        {
            Debug.Log("Win!");
            StartCoroutine(TriggerRestart(restartTime));
        }
    }

    void OnToggleWalls()
    {
        cameraManager.ToggleWalls();
    }

    bool IsDraggingAlongWall()
    {
        return currentCollisions.Count > 0 && body.velocity.magnitude >= 0.01f;
    }

    Cue CalcCue(ContactPoint2D contactPoint, float intensity)
    {
        var normal = contactPoint.normal;
        var angle = Vector2.SignedAngle(-normal, body.transform.up);
        var sin = Mathf.Sin(angle * Mathf.Deg2Rad);

        Cue cue;
        cue.orientation = new Vector2(0.5f - 0.5f * sin, 0.5f + 0.5f * sin);
        cue.intensity = cue.orientation * intensity;

        return cue;
    }

    void PlayCue(Cue cue)
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

    void StopCue()
    {
        Gamepad.current?.SetMotorSpeeds(0.0f, 0.0f);
        audioSource.Stop();

        isPlayingCue = false;
        Debug.Log("Stopping cue");
    }

    IEnumerator TriggerCue(Cue cue, float time)
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

    IEnumerator TriggerRestart(float time)
    {
        cameraManager.OnGameWon();
        yield return new WaitForSeconds(time);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    AudioSource CreateCueAudioSource()
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
