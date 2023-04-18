using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

enum Directions
{
    LEFT = 0, UP = 1, RIGHT = 2, DOWN = 3
}



public class Generator : MonoBehaviour
{
    public GameObject[] tiles;
    private GameObject[] options;
    private float tileSize = 0.1f;
    public const int numberOfRooms = 10;
    const int mapMaxSize = numberOfRooms * 2;
    int currNumberOfRooms = 0;
    bool canNextOneAdd = true;
    bool firstOne = true;
    private GameObject[,] finishedMap = new GameObject[mapMaxSize + 1, mapMaxSize + 1];

    void Start()
    {
        for (int i = 0; i <= mapMaxSize; i++)
        {
            for (int j = 0; j <= mapMaxSize; j++)
            {
                finishedMap[i, j] = null;
            }
        }

        StartCoroutine(AddRooms(mapMaxSize / 2, mapMaxSize / 2, 5));


    }

    IEnumerator AddRooms(int x, int y, int previousDir)
    {
        while (!canNextOneAdd)
        {
            yield return null;
        }
        canNextOneAdd = false;
        if (currNumberOfRooms < numberOfRooms)
        {
            currNumberOfRooms++;
            FindCorrectTiles(x, y, false);
            if (finishedMap[x, y].name[(int)Directions.LEFT] == '1' && previousDir != (int)Directions.LEFT && finishedMap[x - 1, y] == null)
            {
                StartCoroutine((AddRooms(x - 1, y, (int)Directions.RIGHT)));
            }
            if (finishedMap[x, y].name[(int)Directions.UP] == '1' && previousDir != (int)Directions.UP && finishedMap[x, y + 1] == null)
            {
                StartCoroutine((AddRooms(x, y + 1, (int)Directions.DOWN)));
            }
            if (finishedMap[x, y].name[(int)Directions.RIGHT] == '1' && previousDir != (int)Directions.RIGHT && finishedMap[x + 1, y] == null)
            {
                StartCoroutine((AddRooms(x + 1, y, (int)Directions.LEFT)));
            }
            if (finishedMap[x, y].name[(int)Directions.DOWN] == '1' && previousDir != (int)Directions.DOWN && finishedMap[x, y - 1] == null)
            {
                StartCoroutine((AddRooms(x, y - 1, (int)Directions.UP)));
            }
        }
        else
        {

            FindCorrectTiles(x, y, true);

        }
        canNextOneAdd = true;
    }


    private void FindCorrectTiles(int x, int y, bool deadEnd)
    {
        options = tiles;
        if (finishedMap[x, y] != null) { return; }

        if (finishedMap[x - 1, y] != null)
        {
            if (finishedMap[x - 1, y].name[(int)Directions.RIGHT] == '1')
            {
                RemoveFromOptions((int)Directions.LEFT, '0');
            }
            else
            {
                RemoveFromOptions((int)Directions.LEFT, '1');
            }
        }
        if (finishedMap[x + 1, y] != null)
        {
            if (finishedMap[x + 1, y].name[(int)Directions.LEFT] == '1')
            {
                RemoveFromOptions((int)Directions.RIGHT, '0');
            }
            else
            {
                RemoveFromOptions((int)Directions.RIGHT, '1');
            }
        }
        if (finishedMap[x, y - 1] != null)
        {
            if (finishedMap[x, y - 1].name[(int)Directions.UP] == '1')
            {
                RemoveFromOptions((int)Directions.DOWN, '0');
            }
            else
            {
                RemoveFromOptions((int)Directions.DOWN, '1');
            }
        }
        if (finishedMap[x, y + 1] != null)
        {
            if (finishedMap[x, y + 1].name[(int)Directions.DOWN] == '1')
            {
                RemoveFromOptions((int)Directions.UP, '0');
            }
            else
            {
                RemoveFromOptions((int)Directions.UP, '1');
            }
        }

        if (firstOne)
        {
            RemoveFromOptions((int)Directions.LEFT, '0');
            RemoveFromOptions((int)Directions.RIGHT, '0');
            firstOne = false;
        }

        // end of max map
        if (x == 1)
            RemoveFromOptions((int)Directions.LEFT, '1');
        if (x == mapMaxSize - 1)
            RemoveFromOptions((int)Directions.RIGHT, '1');
        if (y == 1)
            RemoveFromOptions((int)Directions.DOWN, '1');
        if (y == mapMaxSize - 1)
            RemoveFromOptions((int)Directions.UP, '1');

        //Dead Ends
        if (deadEnd)
        {

            if (finishedMap[x, y + 1] == null)
            {
                RemoveFromOptions((int)Directions.UP, '1');
            }
            if (finishedMap[x, y - 1] == null)
            {
                RemoveFromOptions((int)Directions.DOWN, '1');
            }
            if (finishedMap[x - 1, y] == null)
            {
                RemoveFromOptions((int)Directions.LEFT, '1');
            }
            if (finishedMap[x + 1, y] == null)
            {
                RemoveFromOptions((int)Directions.RIGHT, '1');
            }
        }

        AddRandomTile(x, y);

    }


    private void AddRandomTile(int x, int y)
    {
        if (options.Length == 0)
        {
            return;
        }
        int rand = UnityEngine.Random.Range(0, options.Length);
        string tileName = options[rand].name;
        AddTile(tileName, x, y);
    }

    private void RemoveFromOptions(int dir, char hasCorridor)
    {

        for (int i = 0; i < options.Length; i++)
        {
            GameObject[] newArray = new GameObject[options.Length - 1];
            if (options[i].name[dir] == hasCorridor)
            {
                Array.Copy(options, 0, newArray, 0, i);
                Array.Copy(options, i + 1, newArray, i, options.Length - i - 1);
                options = newArray;
                i--;
            }
        }
    }

    private void AddTile(string name, int x, int y)
    {
        finishedMap[x, y] = FindTileFromName(name);

        Instantiate(finishedMap[x, y], new Vector3(tileSize * x, tileSize * y, 0), Quaternion.identity);

    }

    private GameObject FindTileFromName(string name)
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i].name == name) return tiles[i];
        }
        return null;
    }
}
