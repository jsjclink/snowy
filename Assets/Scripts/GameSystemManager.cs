using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TerrainType { BASE_FLOOR, BASE_WALL, BASE_NEAR_WALL };

public enum ChunkType { BASE_FLOOR, BASE_WALL, BASE_NEAR_WALL };

public class ChunkInfo
{
    public ChunkType chunk_type;
    public int chunk_size;

    public TerrainType[,] grid;

    public ChunkInfo(ChunkType chunk_type, int chunk_size)
    {
        this.chunk_type = chunk_type;
        this.chunk_size = chunk_size;
        this.grid = new TerrainType[chunk_size, chunk_size];
        SetGridAllByTerrainType(chunk_type, chunk_size);
    }
    public ChunkInfo(ChunkInfo src_chunk)
    {
        this.chunk_type = src_chunk.chunk_type;
        this.chunk_size = src_chunk.chunk_size;
        this.grid = new TerrainType[src_chunk.chunk_size, src_chunk.chunk_size];
        for(int dx = 0; dx < src_chunk.chunk_size; dx++)
        {
            for(int dy = 0; dy < src_chunk.chunk_size; dy++)
            {
                this.grid[dx, dy] = src_chunk.grid[dx, dy];
            }
        }
    }
    public void SetGridAllByTerrainType(ChunkType chunk_type, int chunk_size)
    {
        TerrainType terrain_type = TerrainType.BASE_WALL;
        switch (chunk_type)
        {
            case ChunkType.BASE_FLOOR:
                terrain_type = TerrainType.BASE_FLOOR;
                break;
            case ChunkType.BASE_WALL:
                terrain_type = TerrainType.BASE_WALL;
                break;
            case ChunkType.BASE_NEAR_WALL:
                terrain_type = TerrainType.BASE_NEAR_WALL;
                break;
        }

        for (int dx = 0; dx < chunk_size; dx++)
        {
            for (int dy = 0; dy < chunk_size; dy++)
            {
                this.grid[dx, dy] = terrain_type;
            }
        }
    }
}
public class MapInfo
{
    public ChunkInfo[,] chunk_arr;
    public int seed; public int map_size; public int chunk_size; public int noise_density; public int iterations;

    public MapInfo(int seed, int map_size, int chunk_size, int noise_density, int iterations)
    {
        this.seed = seed;
        this.map_size = map_size;
        this.chunk_size = chunk_size;
        this.noise_density = noise_density;
        this.iterations = iterations;

        UnityEngine.Random.InitState(seed);
        InitChunkArr(map_size, chunk_size, noise_density, iterations);
        MarkBoundaryChunks(map_size, chunk_size);
        EroseBoundaryChunkTerrain(map_size, chunk_size);
        EroseBoundaryChunkTerrain(map_size, chunk_size);
        EroseBoundaryChunkTerrain(map_size, chunk_size);
        MarkBoundaryTerrains(map_size, chunk_size);
    }

    private void InitChunkArr(int map_size, int chunk_size, int noise_density, int iterations)
    {
        ChunkInfo[,] new_chunk_arr = new ChunkInfo[map_size, map_size];

        for (int x = 0; x < map_size; x++)
        {
            for (int y = 0; y < map_size; y++)
            {
                if (UnityEngine.Random.Range(0, 101) > noise_density)
                {
                    new_chunk_arr[x, y] = new ChunkInfo(ChunkType.BASE_FLOOR, chunk_size);
                }
                else
                {
                    new_chunk_arr[x, y] = new ChunkInfo(ChunkType.BASE_WALL, chunk_size);
                }
            }
        }

        for (int i = 0; i < iterations; i++)
        {
            ChunkInfo[,] tmp_arr = CopyChunkArr(new_chunk_arr);
            for (int x = 0; x < map_size; x++)
            {
                for (int y = 0; y < map_size; y++)
                {
                    int neighbor_floor_cnt = 0;
                    int[] dx = { 0, 1, 1, 1, 0, -1, -1, -1 };
                    int[] dy = { 1, 1, 0, -1, -1, -1, 0, 1 };

                    for (int di = 0; di < 8; di++)
                    {
                        if (x + dx[di] >= 0 && x + dx[di] < map_size && y + dy[di] >= 0 && y + dy[di] < map_size)
                        {
                            if (tmp_arr[x + dx[di], y + dy[di]].chunk_type == ChunkType.BASE_FLOOR)
                            {
                                neighbor_floor_cnt++;
                            }
                        }
                    }

                    if (neighbor_floor_cnt > 4)
                    {
                        new_chunk_arr[x, y].chunk_type = ChunkType.BASE_FLOOR;
                        new_chunk_arr[x, y].SetGridAllByTerrainType(ChunkType.BASE_FLOOR, chunk_size);
                    }
                    else
                    {
                        new_chunk_arr[x, y].chunk_type = ChunkType.BASE_WALL;
                        new_chunk_arr[x, y].SetGridAllByTerrainType(ChunkType.BASE_WALL, chunk_size);
                    }
                }
            }
        }

        this.chunk_arr = new_chunk_arr;
    }

    private void MarkBoundaryChunks(int map_size, int chunk_size)
    {
        ChunkInfo[,] tmp_arr = CopyChunkArr(this.chunk_arr);
        for (int x = 0; x < map_size; x++)
        {
            for (int y = 0; y < map_size; y++)
            {
                if (tmp_arr[x, y].chunk_type != ChunkType.BASE_FLOOR) continue;
                int[] dx = { 0, 1, 0, -1 };
                int[] dy = { 1, 0, -1, 0 };
                for (int di = 0; di < 4; di++)
                {
                    if (x + dx[di] >= 0 && x + dx[di] < map_size && y + dy[di] >= 0 && y + dy[di] < map_size)
                    {
                        if (tmp_arr[x + dx[di], y + dy[di]].chunk_type == ChunkType.BASE_WALL)
                        {
                            this.chunk_arr[x, y].chunk_type = ChunkType.BASE_NEAR_WALL;
                        }
                    }
                }
            }
        }
    }

    private void EroseBoundaryChunkTerrain(int map_size, int chunk_size)
    {
        ChunkInfo[,] tmp_arr = CopyChunkArr(this.chunk_arr);

        for(int x = 0; x < map_size; x++)
        {
            for(int y = 0; y < map_size; y++)
            {
                if (this.chunk_arr[x, y].chunk_type != ChunkType.BASE_NEAR_WALL) continue; 
                for(int dx = 0; dx < chunk_size; dx++)
                {
                    for(int dy = 0; dy < chunk_size; dy++)
                    {
                        int neighbor_floor_cnt = 0;
                        int[] dx_list = { 0, 1, 1, 1, 0, -1, -1, -1 };
                        int[] dy_list = { 1, 1, 0, -1, -1, -1, 0, 1 };
                        for(int di = 0; di < 8; di++)
                        {
                            if(IsValidChunkArrIndex(x, y, map_size) && IsValidGridArrIndex(dx + dx_list[di], dy + dy_list[di], chunk_size))
                            {
                                if (tmp_arr[x, y].grid[dx + dx_list[di], dy + dy_list[di]] == TerrainType.BASE_FLOOR)
                                {
                                    neighbor_floor_cnt++;
                                }
                            }
                        }

                        if(neighbor_floor_cnt > UnityEngine.Random.Range(0, 8))
                        {
                            this.chunk_arr[x, y].grid[dx, dy] = TerrainType.BASE_FLOOR;
                        }
                        else
                        {
                            this.chunk_arr[x, y].grid[dx, dy] = TerrainType.BASE_WALL;
                        }
                    }
                }
            }
        }
    }


    private TerrainType[,] GetTerrainArr(int map_size, int chunk_size)
    {
        TerrainType[,] terrain_arr = new TerrainType[map_size * chunk_size, map_size * chunk_size];

        for (int x = 0; x < map_size; x++)
        {
            for (int y = 0; y < map_size; y++)
            {
                for (int dx = 0; dx < chunk_size; dx++)
                {
                    for (int dy = 0; dy < chunk_size; dy++)
                    {
                        terrain_arr[x * chunk_size + dx, y * chunk_size + dy] = this.chunk_arr[x, y].grid[dx, dy];
                    }
                }
            }
        }

        return terrain_arr;
    }

    private void MarkBoundaryTerrains(int map_size, int chunk_size)
    {
        TerrainType[,] terrain_arr = GetTerrainArr(map_size, chunk_size);
        TerrainType[,] tmp_terrain_arr = GetTerrainArr(map_size, chunk_size);

        for (int x = 0; x < map_size * chunk_size; x++)
        {
            for(int y = 0; y < map_size * chunk_size; y++)
            {
                if (tmp_terrain_arr[x, y] != TerrainType.BASE_FLOOR) continue;
                int[] dx = { 0, 1, 0, -1 };
                int[] dy = { 1, 0, -1, 0 };
                for(int di = 0; di < 4; di++)
                {
                    if (x + dx[di] >= 0 && x + dx[di] < map_size * chunk_size && y + dy[di] >= 0 && y + dy[di] < map_size * chunk_size)
                    {
                        if (tmp_terrain_arr[x + dx[di], y + dy[di]] == TerrainType.BASE_WALL)
                        {
                            terrain_arr[x, y] = TerrainType.BASE_NEAR_WALL;
                        }
                    }
                }
            }
        }

        for(int x = 0; x < map_size; x++)
        {
            for(int y = 0; y < map_size; y++)
            {
                for(int dx = 0; dx < chunk_size; dx++)
                {
                    for(int dy = 0; dy < chunk_size; dy++)
                    {
                        this.chunk_arr[x, y].grid[dx, dy] = terrain_arr[x * chunk_size + dx, y * chunk_size + dy];
                    }
                }
            }
        }
    }

    public static ChunkInfo[,] CopyChunkArr(ChunkInfo[,] target_arr)
    {
        ChunkInfo[,] ret_arr = new ChunkInfo[target_arr.GetLength(0), target_arr.GetLength(1)];

        for(int x = 0; x < target_arr.GetLength(0); x++)
        {
            for(int y = 0; y < target_arr.GetLength(1); y++)
            {
                ret_arr[x, y] = new ChunkInfo(target_arr[x, y]);
            }
        }

        return ret_arr;
    }
    public static bool IsValidChunkArrIndex(int x, int y, int map_size)
    {
        if (x >= 0 && x < map_size && y >= 0 && y <= map_size) return true;
        else return false;
    }
    public static bool IsValidGridArrIndex(int x, int y, int chunk_size)
    {
        if (x >= 0 && x < chunk_size && y >= 0 && y < chunk_size) return true;
        else return false;
    }
}

public class GameSystemManager : MonoBehaviour
{
    [SerializeField]
    Camera cam;

    [SerializeField]
    GameObject player_obj;

    [SerializeField]
    GameObject tile_base_floor_pref;
    [SerializeField]
    GameObject tile_base_wall_pref;
    [SerializeField]
    GameObject tile_base_near_wall_pref;

    public MapInfo map_info;
    void Start()
    {
        //graphic
        SetResolution();

        //init map
        int seed = 0; int map_size = 50; int chunk_size = 5; int noise_density = 40; int iterations = 3;
        this.map_info = new MapInfo(seed, map_size, chunk_size, noise_density, iterations);
        //draw map
        DrawEntireMap();
        //spawn player
        SpawnPlayer();
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        player_obj.transform.position += new Vector3(x, y, 0) * 1 * Time.deltaTime;
    }

    void SetResolution()
    {
        int set_width = 1920;
        int set_height = 1080;

        int phone_width = Screen.width;
        int phone_height = Screen.height;
        //Debug.Log(phone_width);
        //Debug.Log(phone_height);

        if ((float)set_width / set_height < (float)phone_width / phone_height)
        {
            float newWidth = ((float)set_width / set_height) / ((float)phone_width / phone_height);
            cam.rect = new Rect((1f - newWidth) / 2f, 0f, newWidth, 1f);
        }
        else
        {
            float newHeight = ((float)phone_width / phone_height) / ((float)set_width / set_height);
            cam.rect = new Rect(0f, (1f - newHeight) / 2f, 1f, newHeight);
        }
    }

    void DrawEntireMap()
    {
        MapInfo cur_map_info = this.map_info;
        int map_size = this.map_info.map_size;
        int chunk_size = this.map_info.chunk_size;

        for (int x = 0; x < map_size; x++)
        {
            for (int y = 0; y < map_size; y++)
            {
                for (int dx = 0; dx < chunk_size; dx++)
                {
                    for (int dy = 0; dy < chunk_size; dy++)
                    {
                        switch (this.map_info.chunk_arr[x, y].grid[dx, dy])
                        {
                            case TerrainType.BASE_FLOOR:
                                Instantiate(tile_base_floor_pref, new Vector3(x * chunk_size + dx, y * chunk_size + dy, 0), Quaternion.identity);
                                break;
                            case TerrainType.BASE_NEAR_WALL:
                                Instantiate(tile_base_near_wall_pref, new Vector3(x * chunk_size + dx, y * chunk_size + dy, 0), Quaternion.identity);
                                break;
                            case TerrainType.BASE_WALL:
                                Instantiate(tile_base_wall_pref, new Vector3(x * chunk_size + dx, y * chunk_size + dy, 0), Quaternion.identity);
                                break;
                        }
                    }
                }
            }
        }
    }

    void SpawnPlayer()
    {
        MapInfo cur_map_info = this.map_info;
        int map_size = this.map_info.map_size;
        int chunk_size = this.map_info.chunk_size;

        for(int x = 0; x < map_size; x++)
        {
            for(int y = 0; y < map_size; y++)
            {
                for(int dx = 0; dx < chunk_size; dx++)
                {
                    for(int dy = 0; dy < chunk_size; dy++)
                    {
                        if (cur_map_info.chunk_arr[x, y].grid[dx, dy] == TerrainType.BASE_FLOOR)
                        {
                            player_obj.transform.position = new Vector3(x * chunk_size + dx, y * chunk_size + dy, 0);
                            return;
                        }
                    }
                }
            }
        }
    }
}
