using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BridgeReveal : MonoBehaviour
{
    [Header("Projection Settings")]
    [SerializeField] private float timeBetweenTiles  = 0.04f;  // fast appearance
    [SerializeField] private float projectionDuration = 1.5f;  // how long the visual stays
    [SerializeField] private float fadeOutDuration    = 0.3f;  // quick fade out

    [Header("Flicker On")]
    [SerializeField] private int   flickerCount    = 4;        // flickers before settling
    [SerializeField] private float flickerInterval = 0.06f;    // speed of flicker

    private Tilemap           tilemap;
    private TilemapRenderer   tilemapRenderer;
    private TilemapCollider2D tilemapCollider;

    private Dictionary<Vector3Int, TileBase> _tileData  = new Dictionary<Vector3Int, TileBase>();
    private List<Vector3Int>                 _positions  = new List<Vector3Int>();

    void Awake()
    {
        tilemap         = GetComponent<Tilemap>();
        tilemapRenderer = GetComponent<TilemapRenderer>();
        tilemapCollider = GetComponent<TilemapCollider2D>();

        CollectTiles();

        // Start invisible, no collision
        tilemapRenderer.enabled = false;
        tilemapCollider.enabled = false;

        foreach (Vector3Int pos in _positions)
            tilemap.SetTile(pos, null);
    }

    private void CollectTiles()
    {
        tilemap.CompressBounds();

        foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin)
        {
            TileBase tile = tilemap.GetTile(pos);
            if (tile != null)
            {
                _tileData[pos] = tile;
                _positions.Add(pos);
            }
        }

        _positions.Sort((a, b) => a.x.CompareTo(b.x));
        Debug.Log("[BridgeReveal] Collected " + _positions.Count + " tiles.");
    }

    public void RevealBridge()
    {
        Debug.Log("[BridgeReveal] RevealBridge called.");
        StartCoroutine(ProjectionRoutine());
    }

    private IEnumerator ProjectionRoutine()
    {
        // Phase 1 — flicker on like a drone projection booting up
        yield return StartCoroutine(FlickerOn());

        // Phase 2 — place tiles fast left to right
        tilemapRenderer.enabled = true;
        tilemap.color = new Color(0.5f, 0.85f, 1f, 0.85f); // cyan tint while projected

        foreach (Vector3Int pos in _positions)
        {
            tilemap.SetTile(pos, _tileData[pos]);
            yield return new WaitForSeconds(timeBetweenTiles);
        }

        // Phase 3 — collision goes on permanently, visual stays briefly
        tilemapCollider.enabled = true;
        Debug.Log("[BridgeReveal] Collision enabled.");

        // Phase 4 — hold the visual for a moment
        yield return new WaitForSeconds(projectionDuration);

        // Phase 5 — visual fades out quickly, collision stays
        yield return StartCoroutine(FadeOutVisual());

        Debug.Log("[BridgeReveal] Visual gone, collision remains.");
    }

    private IEnumerator FlickerOn()
    {
        tilemapRenderer.enabled = true;

        for (int i = 0; i < flickerCount; i++)
        {
            // Place all tiles instantly at low alpha for flicker effect
            foreach (Vector3Int pos in _positions)
                tilemap.SetTile(pos, _tileData[pos]);

            tilemap.color = new Color(0.5f, 0.85f, 1f, i % 2 == 0 ? 0.4f : 0f);
            yield return new WaitForSeconds(flickerInterval);

            // Clear tiles on off flicker
            if (i % 2 != 0)
            {
                foreach (Vector3Int pos in _positions)
                    tilemap.SetTile(pos, null);
            }
        }

        // Clear again so BuildRoutine places them fresh
        foreach (Vector3Int pos in _positions)
            tilemap.SetTile(pos, null);

        tilemap.color = Color.white;
    }

    private IEnumerator FadeOutVisual()
    {
        float elapsed = 0f;
        Color startColor = tilemap.color;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0f, elapsed / fadeOutDuration);
            tilemap.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        // Visual completely gone but collision is still active
        tilemapRenderer.enabled = false;
        tilemap.color = Color.white;
    }
}