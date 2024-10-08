using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerManager : MonoBehaviour
{
    [Header("Tilemap Settings")]
    [SerializeField] Tilemap floorTile;

    [Header("Move Stat")]
    [SerializeField] float moveDuration = 0.1f;

    [Header("Hero Line")]
    [SerializeField] List<HeroController> heroList = new List<HeroController>();
    [SerializeField] List<Vector2> positionHistory = new List<Vector2>();
    [SerializeField] GameObject playerMarker;

    private bool isMoving = false;
    private PlayerControl playercontrol;
    private Vector2 lastestMove = new Vector2();
    //public event Action OnMoveEnd;
    public event Action OnGameEnd;

    private void Awake() 
    {
        playercontrol = new PlayerControl();
        playercontrol.Player.Movement.performed += ctx => MovePlay(ctx.ReadValue<Vector2>());
    }
    private void OnEnable() 
    {
        playercontrol.Enable();
    }
    private void OnDisable() 
    {
        playercontrol.Disable();
    }

    public void StartSetup(HeroController hero,Vector2 position)
    {
        ResetValue();
        SetMoveable(true);
        positionHistory.Add(position);
        transform.position = position;
        GetNewHero(hero);
        SetMarker();
    }

    private void ResetValue()
    {
        foreach(var hero in heroList)
        {
            hero.gameObject.SetActive(false);
        }
        heroList.Clear();
        positionHistory.Clear();

        isMoving = false;
        lastestMove = new Vector2();
    }

    public void SetMoveable(bool canMove)
    {
        if (canMove)
            playercontrol.Enable();
        else
            playercontrol.Disable();
    }
    private void OnTriggerEnter2D(Collider2D col) 
    {
        if (col.gameObject.tag == GlobalTag.RECRUITHERO)
        {
            GetNewHero(col.GetComponent<HeroController>());
        }
        else if (col.gameObject.tag == GlobalTag.LINE_HERO)
        {
            if (col.GetComponent<HeroController>() != heroList[0])
                OnGameEnd.Invoke();
        }
    }

    private async void GetNewHero(HeroController hero)
    {
        if (heroList.Contains(hero))
            return;
        heroList.Add(hero);
        await AddHeroToLine(hero.gameObject);

        hero.gameObject.tag = GlobalTag.LINE_HERO;
        hero.gameObject.GetComponent<HeroController>().ActiveUI(true);
    }

#region Moving Function
    private void MovePlay(Vector2 direction)
    {
        if (MoveCheck(direction) && !isMoving)
        {
            MoveHead(direction);
            //OnMoveEnd.Invoke();
        }
    }
    public async void MoveHead(Vector2 direction)
    {
        positionHistory.Add(transform.position + (Vector3)direction);

        transform.position += (Vector3)direction;
        lastestMove = direction;

        isMoving = true;
        List<Task> tasks = new List<Task>();
        for (int i = 0; i < heroList.Count; i++)
        {
            tasks.Add(MoveHero(heroList[i].gameObject,positionHistory[heroList.Count - i]));
        } 
        await Task.WhenAll(tasks);
        isMoving = false;

        ClearOldHistory();
    }

    private async Task MoveHero(GameObject hero,Vector2 direction)
    {
        var col = hero.GetComponent<BoxCollider2D>();
        col.enabled = false;

        float elapsedTime = 0;
        var startPosition = hero.transform.position;
        var endPosition = direction;

        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float percent = elapsedTime / moveDuration;
            hero.transform.position = Vector2.Lerp(startPosition, endPosition, percent);
            await Task.Yield();
        }

        hero.transform.position = endPosition;
        col.enabled = true;
    }

    private bool MoveCheck(Vector2 direction)
    {
        Vector3Int grid = floorTile.WorldToCell(transform.position + (Vector3)direction);
        if (!floorTile.HasTile(grid) || direction == -lastestMove)
            return false;
        return true;
    }
#endregion

#region ADD/REMOVE HERO
    private async Task AddHeroToLine(GameObject hero)
    {
        float elapsedTime = 0;
        var startPosition = hero.transform.position;
        var endPosition = positionHistory[0];

        while (elapsedTime < moveDuration)
        {
            isMoving = true;

            elapsedTime += Time.deltaTime;
            float percent = elapsedTime / moveDuration;
            hero.transform.position = Vector2.Lerp(startPosition, endPosition, percent);
            await Task.Yield();
        }
        hero.transform.position = endPosition;
        isMoving = false;
    }

    public async Task RemoveHeroFromLine(HeroController hero)
    {
        isMoving = true;
        List<Task> tasks = new List<Task>();
        for(int i = heroList.IndexOf(hero) + 1; i < heroList.Count; i++)
        {
            tasks.Add(MoveHero(heroList[i].gameObject, positionHistory[heroList.Count - i]));
        }
        await Task.WhenAll(tasks);
        isMoving = false;

        heroList.Remove(hero);
        if(heroList.Count <= 0)
        {
            OnGameEnd.Invoke();
            return;
        }
        ClearOldHistory();
        SetMarker();
    }

    public async void RemoveHero(HeroController hero)
    {
        await RemoveHeroFromLine(hero);
    }
#endregion

    private void SetMarker()
    {
        playerMarker.transform.SetParent(heroList[0].transform);
        playerMarker.transform.localPosition = Vector2.zero;
    }
    private void ClearOldHistory()
    {
        if (positionHistory.Count > heroList.Count)
        {
            while (positionHistory.Count > heroList.Count)
            {
                positionHistory.RemoveAt(0);
            }
        }
    }
    public bool IsPositionNotUsed(Vector2 position)
    {
        return !positionHistory.Contains(position);
    }

    public int GetHeroCount()
    {
        return heroList.Count;
    }
}
