using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    public List<GameObject> terrainChunks;
    public GameObject player;
    public float checkerRadius;
    public LayerMask terrainMask;
    public GameObject currentChunk;
    Vector3 playerLastPosition;
    

    [Header("Optimization")]
    public List<GameObject> spawnedChunks;
    public GameObject latestChunk;
    public float maxOpDist;
    float opDist;
    float optimizerCooldown;
    public float optimizerCooldepownDur;

    void Start()
    {
        playerLastPosition = player.transform.position;
    }


    void Update()
    {
        ChunkChecker();
        ChunkOptimizer();
    }

    void ChunkChecker()
    {
        if (!currentChunk)
        {
            return;
        }

        Vector3 moveDir = player.transform.position - playerLastPosition;
        playerLastPosition = player.transform.position;

        string directionName = GetDirectionName(moveDir);

        CheckAndSpawnChunk(directionName);

        //Check additional adjacent directions for diagonal chunks
        if (directionName.Contains("Up"))
        {
            CheckAndSpawnChunk("Up");
        }

        if (directionName.Contains("Down"))
        {
            CheckAndSpawnChunk("Down");
        }

        if (directionName.Contains("Right"))
        {
            CheckAndSpawnChunk("Right");
        }

        if (directionName.Contains("Left"))
        {
            CheckAndSpawnChunk("Left");
        }
    }
    
    #region Archive diagonal chunk generator

    //if (!Physics2D.OverlapCircle(currentChunk.transform.Find(directionName).position, checkerRadius, terrainMask))
    //{
    //    SpawnChunk(currentChunk.transform.Find(directionName).position);

    //    //Check additional adjacent directions for diagonal chunks
    //    if (directionName.Contains("Up") && directionName.Contains("Right"))
    //    {
    //        if (!Physics2D.OverlapCircle(currentChunk.transform.Find("Up").position, checkerRadius, terrainMask))
    //        {
    //            SpawnChunk(currentChunk.transform.Find("Up").position);
    //        }

    //        if (!Physics2D.OverlapCircle(currentChunk.transform.Find("Right").position, checkerRadius, terrainMask))
    //        {
    //            SpawnChunk(currentChunk.transform.Find("Right").position);
    //        }
    //    }
    //    else if (directionName.Contains("Up") && directionName.Contains("Left"))
    //    {
    //        if (!Physics2D.OverlapCircle(currentChunk.transform.Find("Up").position, checkerRadius, terrainMask))
    //        {
    //            SpawnChunk(currentChunk.transform.Find("Up").position);
    //        }

    //        if (!Physics2D.OverlapCircle(currentChunk.transform.Find("Left").position, checkerRadius, terrainMask))
    //        {
    //            SpawnChunk(currentChunk.transform.Find("Left").position);
    //        }
    //    }
    //    else if (directionName.Contains("Down") && directionName.Contains("Right"))
    //    {
    //        if (!Physics2D.OverlapCircle(currentChunk.transform.Find("Down").position, checkerRadius, terrainMask))
    //        {
    //            SpawnChunk(currentChunk.transform.Find("Down").position);
    //        }

    //        if (!Physics2D.OverlapCircle(currentChunk.transform.Find("Right").position, checkerRadius, terrainMask))
    //        {
    //            SpawnChunk(currentChunk.transform.Find("Right").position);
    //        }
    //    }
    //    else if (directionName.Contains("Down") && directionName.Contains("Left"))
    //    {
    //        if (!Physics2D.OverlapCircle(currentChunk.transform.Find("Down").position, checkerRadius, terrainMask))
    //        {
    //            SpawnChunk(currentChunk.transform.Find("Down").position);
    //        }

    //        if (!Physics2D.OverlapCircle(currentChunk.transform.Find("Left").position, checkerRadius, terrainMask))
    //        {
    //            SpawnChunk(currentChunk.transform.Find("Left").position);
    //        }
    //    }
    //}

    #endregion

    void CheckAndSpawnChunk (string direction)
    {
        if (!Physics2D.OverlapCircle(currentChunk.transform.Find(direction).position, checkerRadius, terrainMask))
        {
            SpawnChunk(currentChunk.transform.Find(direction).position);
        }
    }

    string GetDirectionName(Vector3 direction)
    {
        direction = direction.normalized;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            //Moving horizontally more than vertically
            if (direction.y > 0.5f)
            {
                //Also moving upwards
                return direction.x > 0 ? "Right Up" : "Left Up";
            }
            else if (direction.y < -0.5f)
            {
                //Also moving downwards
                return direction.x > 0 ? "Right Down" : "Left Down";
            }
            else
            {
                //Moving straight horizontally
                return direction.x > 0 ? "Right" : "Left";
            }
        }
        else
        {
            //Moving vertically more than horizontally
            if (direction.x > 0.5f)
            {
                //Also moving right
                return direction.y > 0 ? "Right Up" : "Right Down";
            }
            else if (direction.x < -0.5f)
            {
                //Also moving left
                return direction.y > 0 ? "Left Up" : "Left Down";
            }
            else
            {
                //Moving straight vertically
                return direction.y > 0 ? "Up" : "Down";
            }
        }
    }

    #region Archive map generator

    //if (pm.moveDir.x > 0 && pm.moveDir.y == 0) //right
    //{
    //    if (!Physics2D.OverlapCircle(currentChunk.transform.Find("Right").position, checkerRadius, terrainMask))
    //    {
    //        noTerrainPosition = currentChunk.transform.Find("Right").position;
    //        SpawnChunk();
    //    }
    //}

    //else if (pm.moveDir.x < 0 && pm.moveDir.y == 0) //left
    //{
    //    if (!Physics2D.OverlapCircle(currentChunk.transform.Find("Left").position, checkerRadius, terrainMask))
    //    {
    //        noTerrainPosition = currentChunk.transform.Find("Left").position;
    //        SpawnChunk();
    //    }
    //}

    //else if (pm.moveDir.x == 0 && pm.moveDir.y > 0) //up
    //{
    //    if (!Physics2D.OverlapCircle(currentChunk.transform.Find("Up").position, checkerRadius, terrainMask))
    //    {
    //        noTerrainPosition = currentChunk.transform.Find("Up").position;
    //        SpawnChunk();
    //    }
    //}

    //else if (pm.moveDir.x == 0 && pm.moveDir.y < 0) //down
    //{
    //    if (!Physics2D.OverlapCircle(currentChunk.transform.Find("Down").position, checkerRadius, terrainMask))
    //    {
    //        noTerrainPosition = currentChunk.transform.Find("Down").position;
    //        SpawnChunk();
    //    }
    //}

    //else if (pm.moveDir.x > 0 && pm.moveDir.y > 0) //right up
    //{
    //    if (!Physics2D.OverlapCircle(currentChunk.transform.Find("Right Up").position, checkerRadius, terrainMask))
    //    {
    //        noTerrainPosition = currentChunk.transform.Find("Right Up").position;
    //        SpawnChunk();
    //    }
    //}

    //else if (pm.moveDir.x > 0 && pm.moveDir.y < 0) //right down
    //{
    //    if (!Physics2D.OverlapCircle(currentChunk.transform.Find("Right Down").position, checkerRadius, terrainMask))
    //    {
    //        noTerrainPosition = currentChunk.transform.Find("Right Down").position;
    //        SpawnChunk();
    //    }
    //}

    //else if (pm.moveDir.x < 0 && pm.moveDir.y > 0) //left up
    //{
    //    if (!Physics2D.OverlapCircle(currentChunk.transform.Find("Left Up").position, checkerRadius, terrainMask))
    //    {
    //        noTerrainPosition = currentChunk.transform.Find("Left Up").position;
    //        SpawnChunk();
    //    }
    //}

    //else if (pm.moveDir.x < 0 && pm.moveDir.y < 0) //left down
    //{
    //    if (!Physics2D.OverlapCircle(currentChunk.transform.Find("Left Down").position, checkerRadius, terrainMask))
    //    {
    //        noTerrainPosition = currentChunk.transform.Find("Left Down").position;
    //        SpawnChunk();
    //    }
    //}

    #endregion

    void SpawnChunk(Vector3 spawnPosition)
    {
        int rand = Random.Range(0, terrainChunks.Count);
        latestChunk = Instantiate(terrainChunks[rand], spawnPosition, Quaternion.identity);
        spawnedChunks.Add(latestChunk);
    }

    void ChunkOptimizer()
    {
       
        optimizerCooldown -= Time.deltaTime;

        if (optimizerCooldown <= 0f)
        {
            optimizerCooldown = optimizerCooldepownDur;
        }
        else
        {
            return;
        }

        foreach (GameObject chunk in spawnedChunks)
        {
            opDist = Vector3.Distance(player.transform.position, chunk.transform.position);

            if (opDist > maxOpDist)
            {
                chunk.SetActive(false);
            }
            else
            {
                chunk.SetActive(true);
            }
        }
    }
}
