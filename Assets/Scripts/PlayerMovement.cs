using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerMovement : MonoBehaviour
{
    [Header("Tilemap Settings")]
    [SerializeField] Tilemap floorTile;
    [SerializeField] Tilemap wallTile;

    [Header("Move Stat")]
    [SerializeField] float moveDuration = 0.1f;

    private bool isMoving = false;
    private PlayerControl playercontrol;
    private Vector2 lastestMove = new Vector2();
    private void Awake() 
    {
        playercontrol = new PlayerControl();
        playercontrol.Player.Movement.performed += async ctx => await Move(ctx.ReadValue<Vector2>());
    }
    private void OnEnable() 
    {
        playercontrol.Enable();
    }
    private void OnDisable() 
    {
        playercontrol.Disable();
    }

    private async Task Move(Vector2 direction)
    {
        if (MoveCheck(direction) && !isMoving)
        {
            isMoving = true;
            //transform.position += (Vector3)direction;

            float elapsedTime = 0;
            var startPosition = transform.position;
            var endPosition = transform.position + (Vector3)direction;

            while (elapsedTime < moveDuration)
            {
                elapsedTime += Time.deltaTime;
                float percent = elapsedTime / moveDuration;
                transform.position = Vector2.Lerp(startPosition, endPosition, percent);
                await Task.Yield();
            }

            lastestMove = direction;
            transform.position = endPosition;
            isMoving = false;
        }
    }

    private bool MoveCheck(Vector2 direction)
    {
        Vector3Int grid = floorTile.WorldToCell(transform.position + (Vector3)direction);
        if (!floorTile.HasTile(grid) || direction == -lastestMove)
            return false;
        return true;
    }
}
