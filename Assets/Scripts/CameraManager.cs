using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public GameObject mainCamera;
    public GameObject mazeCamera;

    GameObject player;

    bool showWalls = false;

    void Start()
    {
        mainCamera.SetActive(true);
        mazeCamera.SetActive(false);
        player = GameObject.Find("Player");
        Cursor.visible = false;
        ShowWalls(showWalls);
    }

    void Update()
    {
        var cameraPosition = mainCamera.transform.position;
        var playerPosition = player.transform.position;
        mainCamera.transform.position = new Vector3(playerPosition.x, playerPosition.y, cameraPosition.z);
    }

    void ShowWalls(bool inShowWalls)
    {
        showWalls = inShowWalls;

        foreach (GameObject wall in GameObject.FindGameObjectsWithTag("Wall"))
        {
            wall.GetComponent<Renderer>().enabled = showWalls;
        }
        GameObject.FindGameObjectWithTag("Finish").GetComponent<Renderer>().enabled = showWalls;
    }

    public void ToggleWalls()
    {
        ShowWalls(!showWalls);
    }

    public void OnMazeGenerated(float edgeSize, float mazeWorldSize)
    {
        mazeCamera.transform.position = new Vector3(mazeWorldSize / 2.0f, mazeWorldSize / 2.0f, -1.0f);
        mazeCamera.GetComponent<Camera>().orthographicSize = edgeSize;
    }

    public void OnGameWon()
    {
        mainCamera.SetActive(false);
        mazeCamera.SetActive(true);
        ShowWalls(true);
    }
}
