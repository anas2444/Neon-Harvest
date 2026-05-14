using UnityEngine;

public class GridTile : MonoBehaviour
{
    public enum TileState
    {
        Empty,
        Growing,
        Ripe,
        Corrupt,
        Dead
    }

    public TileState currentState = TileState.Empty;

    private SpriteRenderer sr;

    public int growSteps = 2; // how many grow phases needed

    public Sprite emptySprite;
    public Sprite growingSprite;
    public Sprite ripeSprite;
    public Sprite corruptSprite;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        UpdateVisual();
    }

    public void SetState(TileState newState)
    {
        currentState = newState;
        UpdateVisual();
    }

    void UpdateVisual()
    {
        switch (currentState)
        {
            case TileState.Empty:
                sr.sprite = emptySprite;
                break;

            case TileState.Growing:
                sr.sprite = growingSprite;
                break;

            case TileState.Ripe:
                sr.sprite = ripeSprite;
                break;

            case TileState.Corrupt:
                sr.sprite = corruptSprite;
                break;
        }
    }

    // 🌱 PLANT
    public bool PlantCrystal()
    {
        if (currentState != TileState.Empty)
            return false;

        growSteps = 2; // reset growth
        SetState(TileState.Growing);

        return true;
    }

    // ⚡ GROW (called by GridManager)
    public void Grow()
    {
        if (currentState != TileState.Growing)
            return;

        growSteps--;

        if (growSteps <= 0)
        {
            SetState(TileState.Ripe);
        }
    }

    // 💰 HARVEST
    public bool HarvestCrystal()
    {
        if (currentState != TileState.Ripe)
            return false;

        SetState(TileState.Empty);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(10);
            GameManager.Instance.RestoreEnergy(10);
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayHarvest();
        }

        return true;
    }
}