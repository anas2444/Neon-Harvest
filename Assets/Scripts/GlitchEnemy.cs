using UnityEngine;
using System.Collections;

public class GlitchEnemy : MonoBehaviour
{
    public float moveDelay = 1f;

    private Vector2Int gridPosition;
    private GridManager gridManager;

    private void Start()
    {
        gridManager = GridManager.Instance;

        gridPosition = gridManager.WorldToGrid(transform.position);

        transform.position = GridToWorld(gridPosition);

        StartCoroutine(MoveRoutine());
    }

    IEnumerator MoveRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(moveDelay);

            TryRandomMove();
        }
    }

    void TryRandomMove()
    {
        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        Vector2Int randomDir =
            directions[Random.Range(0, directions.Length)];

        Vector2Int targetPos = gridPosition + randomDir;

        if (gridManager.GetTile(targetPos.x, targetPos.y) != null)
        {
            gridPosition = targetPos;
            transform.position = GridToWorld(gridPosition);

            CorruptCurrentTile();
        }
    }

    void CorruptCurrentTile()
    {
        GridTile tile = gridManager.GetTile(
            gridPosition.x,
            gridPosition.y
        );

        if (tile != null)
        {
            tile.SetState(GridTile.TileState.Corrupt);
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CheckAllTilesCorrupt();
            }
        }
    }

    Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(
            gridPos.x - 3.5f,
            gridPos.y - 3.5f,
            -0.4f
        );
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TakeDamage(1);
            }

            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                StartCoroutine(player.Blink());
            }
        }
    }
}
