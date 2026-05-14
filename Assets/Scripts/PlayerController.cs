using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 8f;

    private bool isMoving = false;
    private Vector2Int gridPosition;
    private Vector2Int lastMoveDir = Vector2Int.down;

    private GridManager gridManager;
    private SpriteRenderer sr;
    private Animator anim;

    private void Start()
    {
        gridManager = GridManager.Instance;
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        gridPosition = new Vector2Int(0, 7);
        transform.position = GridToWorld(gridPosition);

        UpdateAnimator(lastMoveDir, false);
    }

    private void Update()
    {
        if (isMoving) return;

        Vector2Int moveDir = Vector2Int.zero;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            moveDir = Vector2Int.up;
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            moveDir = Vector2Int.down;
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            moveDir = Vector2Int.left;
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            moveDir = Vector2Int.right;

        if (moveDir != Vector2Int.zero)
            TryMove(moveDir);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (anim != null)
                anim.SetTrigger("Harvest");

            SmartAction();
        }
    }

    void TryMove(Vector2Int direction)
    {
        Vector2Int targetPos = gridPosition + direction;

        if (gridManager.GetTile(targetPos.x, targetPos.y) == null)
            return;

        lastMoveDir = direction;

        if (sr != null)
        {
            if (direction.x < 0)
                sr.flipX = true;
            else if (direction.x > 0)
                sr.flipX = false;
        }

        UpdateAnimator(direction, true);

        gridPosition = targetPos;
        StartCoroutine(MoveToPosition(GridToWorld(gridPosition)));
    }

    void SmartAction()
    {
        GridTile currentTile = gridManager.GetTile(gridPosition.x, gridPosition.y);

        if (currentTile == null) return;

        if (currentTile.currentState == GridTile.TileState.Ripe)
        {
            TryHarvest();
        }
        else if (currentTile.currentState == GridTile.TileState.Empty)
        {
            TryPlant();
        }
        else if (currentTile.currentState == GridTile.TileState.Corrupt)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.ShowMessage("Tile is corrupted!");
        }
    }

    void TryPlant()
    {
        if (GameManager.Instance.currentPhase != GameManager.Phase.Plant)
        {
            GameManager.Instance.ShowMessage("Wait for Plant Phase!");
            return;
        }

        GridTile currentTile = gridManager.GetTile(gridPosition.x, gridPosition.y);

        if (currentTile == null) return;

        if (GameManager.Instance != null &&
            !GameManager.Instance.UseEnergy(5))
        {
            GameManager.Instance.ShowMessage("Not enough energy!");
            return;
        }

        bool planted = currentTile.PlantCrystal();

        if (!planted && GameManager.Instance != null)
            GameManager.Instance.RestoreEnergy(5);

        if (planted && GameManager.Instance != null)
            GameManager.Instance.ShowMessage("Crystal planted!");
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayPlant();
    }

    void TryHarvest()
    {
        if (GameManager.Instance.currentPhase != GameManager.Phase.Harvest)
        {
            GameManager.Instance.ShowMessage("Wait for Harvest Phase!");
            return;
        }

        GridTile currentTile = gridManager.GetTile(gridPosition.x, gridPosition.y);

        if (currentTile == null) return;

        bool harvested = currentTile.HarvestCrystal();

        if (harvested && GameManager.Instance != null)
            GameManager.Instance.ShowMessage("+10 Score!");
    }

    IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        isMoving = true;

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = targetPosition;
        isMoving = false;

        UpdateAnimator(lastMoveDir, false);
    }

    void UpdateAnimator(Vector2Int direction, bool moving)
    {
        if (anim == null) return;

        anim.SetFloat("MoveX", direction.x);
        anim.SetFloat("MoveY", direction.y);
        anim.SetBool("IsMoving", moving);
    }

    public IEnumerator Blink()
    {
        for (int i = 0; i < 6; i++)
        {
            sr.enabled = false;
            yield return new WaitForSeconds(0.1f);

            sr.enabled = true;
            yield return new WaitForSeconds(0.1f);
        }
    }

    Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(
            gridPos.x - 3.5f,
            gridPos.y - 3.5f,
            -0.5f
        );
    }
}