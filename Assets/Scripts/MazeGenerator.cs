using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [SerializeField]
    int mazeSize;

    public GameObject wallPrefab;
    public GameObject playerPrefab;
    public GameObject winAreaPrefab;

    CameraManager cameraManager;

    int edgeSize;
    float wallSize;

    bool[,] visited;
    bool[,] edge;
    Stack<(int, int)> stack;

    static readonly (int, int)[] directions = {(1, 0), (-1, 0), (0, 1), (0, -1)};

    void Awake()
    {
        edgeSize = mazeSize * 2 + 1;
        wallSize = wallPrefab.transform.localScale.x;
        visited = new bool[mazeSize, mazeSize];
        edge = new bool[edgeSize, edgeSize];
        stack = new Stack<(int, int)>();

        static bool IsEdge(int x, int edgeSize) => x == 0 || x == edgeSize - 1 || (x % 2 == 0);
        for (var x = 0; x < edgeSize; ++x)
        {
            for (var y = 0; y < edgeSize; ++y)
            {
                edge[x, y] = IsEdge(x, edgeSize) || IsEdge(y, edgeSize);
            }
        }

        var start = (Random.Range(0, mazeSize), Random.Range(0, mazeSize));
        (int sX, int sY) = start;
        stack.Push(start);
        visited[sX, sY] = true;

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            (int aX, int aY) = current;

            var neighbours = new List<(int, int)>();
            foreach (var direction in directions)
            {
                (int x, int y) = direction;
                var next = (aX + x, aY + y);
                (int bX, int bY) = next;

                if (bX >= 0 && bX < mazeSize && bY >= 0 && bY < mazeSize && !visited[bX, bY])
                {
                    neighbours.Add(next);
                }
            }

            if (neighbours.Count > 0)
            {
                stack.Push(current);

                var neighbour = neighbours[Random.Range(0, neighbours.Count)];
                (int bX, int bY) = neighbour;
                edge[aX + bX + 1, aY + bY + 1] = false;
                visited[bX, bY] = true;

                stack.Push(neighbour);
            }
        }

        for (var x = 0; x < edgeSize; ++x)
        {
            for (var y = 0; y < edgeSize; ++y)
            {
                if (edge[x, y])
                {
                    var wall = Instantiate(wallPrefab, new Vector3(x * wallSize, y * wallSize), Quaternion.identity);
                    wall.name = "Wall";
                }
            }
        }

        var player = Instantiate(playerPrefab, new Vector3(wallSize, wallSize), Quaternion.identity);
        player.name = "Player";
        var winArea = Instantiate(winAreaPrefab, new Vector3(wallSize * (edgeSize - 2), wallSize * (edgeSize - 2)), Quaternion.identity);
        winArea.name = "WinArea";
    }

    void Start()
    {
        cameraManager = GameObject.Find("CameraManager").GetComponent<CameraManager>();
        cameraManager.OnMazeGenerated(edgeSize, wallSize * edgeSize);
    }
}
