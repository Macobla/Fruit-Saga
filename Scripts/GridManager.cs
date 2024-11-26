using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GridManager : MonoBehaviour
{
    public List<Sprite> Sprites = new List<Sprite>();
    public GameObject TilePrefab;
    public int GridDimension = 8;
    public float Distance = 1.0f;
    private GameObject[,] Grid;

    public int score = 0;
    public int scoreGoal = 100;
    public string nextSceneName = "LevelSelector";
    public AudioClip matchSound; 
    private AudioSource audioSource; 

    public void SwapTiles(Vector2Int tile1Position, Vector2Int tile2Position)
    {
        GameObject tile1 = Grid[tile1Position.x, tile1Position.y];
        SpriteRenderer renderer1 = tile1.GetComponent<SpriteRenderer>();
        GameObject tile2 = Grid[tile2Position.x, tile2Position.y];
        SpriteRenderer renderer2 = tile2.GetComponent<SpriteRenderer>();

        Sprite temp = renderer1.sprite;
        renderer1.sprite = renderer2.sprite;
        renderer2.sprite = temp;

        bool changesOccurred = CheckAndHandleMatches();
        if (!changesOccurred)
        {
            temp = renderer1.sprite;
            renderer1.sprite = renderer2.sprite;
            renderer2.sprite = temp;
        }
        else
        {
            FillHoles();
        }
    }

    void InitGrid()
    {
        Vector3 positionOffset = transform.position - new Vector3(GridDimension * Distance / 2.0f, GridDimension * Distance / 2.0f, 0);
        for (int row = 0; row < GridDimension; row++)
        {
            for (int column = 0; column < GridDimension; column++)
            {
                GameObject newTile = Instantiate(TilePrefab);
                SpriteRenderer renderer = newTile.GetComponent<SpriteRenderer>();
                renderer.sprite = ChooseSpriteForCell(column, row);
                newTile.transform.parent = transform;
                newTile.transform.position = new Vector3(column * Distance, row * Distance, 0) + positionOffset;
                Grid[column, row] = newTile;

                Tile tile = newTile.AddComponent<Tile>();
                tile.Position = new Vector2Int(column, row);
            }
        }
    }

    void Start()
    {
        Grid = new GameObject[GridDimension, GridDimension];
        InitGrid();
        audioSource = GetComponent<AudioSource>(); 
    }

    Sprite GetSpriteAt(int column, int row)
    {
        if (column < 0 || column >= GridDimension || row < 0 || row >= GridDimension)
            return null;
        GameObject tile = Grid[column, row];
        SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
        return renderer.sprite;
    }

    public static GridManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (score >= scoreGoal)
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    Sprite ChooseSpriteForCell(int column, int row)
    {
        List<Sprite> possibleSprites = new List<Sprite>(Sprites);

        Sprite left1 = GetSpriteAt(column - 1, row);
        Sprite left2 = GetSpriteAt(column - 2, row);
        if (left2 != null && left1 == left2)
        {
            possibleSprites.Remove(left1);
        }
        Sprite down1 = GetSpriteAt(column, row - 1);
        Sprite down2 = GetSpriteAt(column, row - 2);
        if (down2 != null && down1 == down2)
        {
            possibleSprites.Remove(down1);
        }

        if (possibleSprites.Count > 0)
        {
            return possibleSprites[Random.Range(0, possibleSprites.Count)];
        }
        else
        {
            return Sprites[Random.Range(0, Sprites.Count)];
        }
    }

    SpriteRenderer GetSpriteRendererAt(int column, int row)
    {
        if (column < 0 || column >= GridDimension || row < 0 || row >= GridDimension)
            return null;
        GameObject tile = Grid[column, row];
        SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
        return renderer;
    }

    bool CheckAndHandleMatches()
    {
        bool anyMatches = false;
        while (true)
        {
            HashSet<SpriteRenderer> matchedTiles = new HashSet<SpriteRenderer>();
            for (int row = 0; row < GridDimension; row++)
            {
                for (int column = 0; column < GridDimension; column++)
                {
                    SpriteRenderer current = GetSpriteRendererAt(column, row);
                    List<SpriteRenderer> horizontalMatches = FindColumnMatchForTile(column, row, current.sprite);
                    if (horizontalMatches.Count >= 2)
                    {
                        matchedTiles.UnionWith(horizontalMatches);
                        matchedTiles.Add(current);
                    }
                    List<SpriteRenderer> verticalMatches = FindRowMatchForTile(column, row, current.sprite);
                    if (verticalMatches.Count >= 2)
                    {
                        matchedTiles.UnionWith(verticalMatches);
                        matchedTiles.Add(current);
                    }
                }
            }
            if (matchedTiles.Count == 0)
            {
                break;
            }
            foreach (SpriteRenderer renderer in matchedTiles)
            {
                renderer.sprite = null;
                score += 10;
                PlayMatchSound(); 
            }
            anyMatches = true;
            FillHoles();
        }
        return anyMatches;
    }

    void PlayMatchSound() 
    {
        if (matchSound != null)
        {
            audioSource.PlayOneShot(matchSound);
        }
    }

    List<SpriteRenderer> FindColumnMatchForTile(int column, int row, Sprite sprite)
    {
        List<SpriteRenderer> matches = new List<SpriteRenderer>();
        for (int i = column + 1; i < GridDimension; i++)
        {
            SpriteRenderer next = GetSpriteRendererAt(i, row);
            if (next != null && next.sprite == sprite)
                matches.Add(next);
            else
                break;
        }
        return matches;
    }

    List<SpriteRenderer> FindRowMatchForTile(int column, int row, Sprite sprite)
    {
        List<SpriteRenderer> matches = new List<SpriteRenderer>();
        for (int i = row + 1; i < GridDimension; i++)
        {
            SpriteRenderer next = GetSpriteRendererAt(column, i);
            if (next != null && next.sprite == sprite)
                matches.Add(next);
            else
                break;
        }
        return matches;
    }

    void FillHoles()
    {
        for (int column = 0; column < GridDimension; column++)
        {
            for (int row = 0; row < GridDimension; row++)
            {
                while (GetSpriteRendererAt(column, row).sprite == null)
                {
                    for (int filler = row; filler < GridDimension - 1; filler++)
                    {
                        SpriteRenderer current = GetSpriteRendererAt(column, filler);
                        SpriteRenderer next = GetSpriteRendererAt(column, filler + 1);
                        current.sprite = next.sprite;
                    }
                    SpriteRenderer last = GetSpriteRendererAt(column, GridDimension - 1);
                    last.sprite = Sprites[Random.Range(0, Sprites.Count)];
                }
            }
        }
    }

    public Vector2Int GetGridPosition(Vector2 worldPosition)
    {
        Vector3 positionOffset = transform.position - new Vector3(GridDimension * Distance / 2.0f, GridDimension * Distance / 2.0f, 0);
        int column = Mathf.FloorToInt((worldPosition.x - positionOffset.x) / Distance);
        int row = Mathf.FloorToInt((worldPosition.y - positionOffset.y) / Distance);
        return new Vector2Int(column, row);
    }

    public bool IsValidPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < GridDimension && position.y >= 0 && position.y < GridDimension;
    }
}
