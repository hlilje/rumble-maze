using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    static readonly int mazeSize = 10;
    static readonly int edgeSize = mazeSize * 2 + 1;
    static readonly (int, int)[] directions = {(1, 0), (-1, 0), (0, 1), (0, -1)};

    public GameObject wallPrefab;
    public GameObject playerPrefab;
    public GameObject winAreaPrefab;

    bool[,] visited;
    bool[,] edges;
    Stack<(int, int)> stack;

    void Start()
    {
        visited = new bool[mazeSize, mazeSize];
        edges = new bool[edgeSize, edgeSize];
        stack = new Stack<(int, int)>();

        static bool IsEdge(int x) => x == 0 || x == edgeSize - 1 || (x % 2 == 0);
        for (var x = 0; x < edgeSize; ++x)
        {
            for (var y = 0; y < edgeSize; ++y)
            {
                edges[x, y] = IsEdge(x) || IsEdge(y);
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
                edges[aX + bX + 1, aY + bY + 1] = false;
                visited[bX, bY] = true;

                stack.Push(neighbour);
            }
        }

        for (var x = 0; x < edgeSize; ++x)
        {
            for (var y = 0; y < edgeSize; ++y)
            {
                if (edges[x, y])
                {
                    var size = wallPrefab.transform.localScale.x;
                    Instantiate(wallPrefab, new Vector3(x * size, y * size), Quaternion.identity);
                }
            }
        }

        Instantiate(playerPrefab, new Vector3(-2, -2), Quaternion.identity);
        Instantiate(winAreaPrefab, new Vector3(32, 32), Quaternion.identity);
    }
}
