using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance => instance;
    private static GameManager instance;
    [SerializeField] MenuUIManager menuUIManager;
    [SerializeField] PlayerManager player;

    [Header("Game Area")]
    [SerializeField] List<Transform> areaMarker = new List<Transform>();

    [Header("Game Start Setting")]
    [SerializeField] String startHero = "WarriorClass";
    [SerializeField] Transform startPosition;
    [SerializeField] int startEnemyCount;
    [SerializeField] int startRecruitHeroCount;
    [SerializeField] List<UnitSpawnChance> enemyTypeSpawnChance = new List<UnitSpawnChance>();
    private List<String> enemyTypePool = new List<String>();
    [SerializeField] List<UnitSpawnChance> heroTypeSpawnChance = new List<UnitSpawnChance>();
    private List<String> heroTypePool = new List<String>();

    [Header("Max Spawn Setting")]
    [SerializeField] int maxHero;
    [SerializeField] int maxEnemy;

    [Header("Wave Setting")]
    [SerializeField] List<UnitSpawnChance> heroSpawnChance = new List<UnitSpawnChance>();
    private List<String> heroSpawnChancePool = new List<String>();
    [SerializeField] float waveToAddMoreEnemy;

    private int currentWave;
    private bool isStart = false;
    private List<EnemyController> enemyOnMapList = new List<EnemyController>();
    private List<HeroController> heroOnMapList = new List<HeroController>();
    private void Awake() 
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(Instance);
        }

        StartGame();
    }

    public async void StartGame()
    {
        await Task.Delay(1);

        ResetGame();
        SetupPlayer();
        SpawnEnemyUnit(startEnemyCount);
        SpawnHeroUnit(startRecruitHeroCount);
    }
    private void SetupPlayer()
    {
        SetUpChancePool(heroSpawnChancePool, heroSpawnChance);
        SetUpChancePool(enemyTypePool, enemyTypeSpawnChance);
        SetUpChancePool(heroTypePool, heroTypeSpawnChance);

        var obj = ObjectPooling.Instance.GetFromPool(startHero, startPosition.position, Quaternion.identity);
        var hero = obj.GetComponent<HeroController>();
        hero.OnHeroDead += player.RemoveHero;
        player.StartSetup(hero,startPosition.position);

        //player.OnMoveEnd += EnemyAction;
        player.OnGameEnd += EndGame;
    }

    public void ResetGame()
    {
        currentWave = 1;
        RemoveHeroFromMap();
        RemoveEnemyFromMap();
    }
    public void EndGame()
    {
        player.SetMoveable(false);
        menuUIManager.ShowDialog();
        Debug.Log("Game Over");
    }

    public void EnemyAction()
    {
        foreach(EnemyController enemy in enemyOnMapList)
        {
            
        }
    }
    private void RemoveHeroFromMap()
    {
        foreach (HeroController hero in heroOnMapList)
        {
            hero.gameObject.SetActive(false);
        }
        heroOnMapList.Clear();
    }
    private void RemoveEnemyFromMap()
    {
        foreach (EnemyController enemy in enemyOnMapList)
        {
            enemy.gameObject.SetActive(false);
        }
        enemyOnMapList.Clear();
    }
    public void DeadEnemyRemove(EnemyController enemy)
    {
        if(enemyOnMapList.Contains(enemy))
            enemyOnMapList.Remove(enemy);

        if(enemyOnMapList.Count <= 0)
        {
            NewEnemyWave(currentWave++);
        }
    }
    public void RecruitHeroPick(HeroController hero)
    {
        heroOnMapList.Remove(hero);
    }

#region SPAWNER
    private void NewEnemyWave(int wave)
    {
        if (waveToAddMoreEnemy == 0)
            return;
    
        SpawnEnemyUnit(Mathf.Clamp(startEnemyCount + (int)(wave / waveToAddMoreEnemy), 0, maxEnemy));
        if (player.GetHeroCount() + heroOnMapList.Count < maxHero)
            SpawnHeroUnit(int.Parse(RandomUnitType(heroSpawnChancePool)));
    }
    public void SpawnEnemyUnit(int value)
    {
        for (int i = 0; i < value; i++) 
        {
            var unit = RandomUnitType(enemyTypePool);
            if (unit != null)
            {
                var spawnObj = SpawnInArea<EnemyController>(unit);
                spawnObj.OnEnemyDead += DeadEnemyRemove;
                enemyOnMapList.Add(spawnObj);
                spawnObj.RestoreHP();
            }
        }
    }
    public void SpawnHeroUnit(int value)
    {
        for (int i = 0; i < value; i++) 
        {
            var unit = RandomUnitType(heroTypePool);
            if (unit != null)
            {
                var spawnObj = SpawnInArea<HeroController>(unit);
                spawnObj.OnHeroDead += player.RemoveHero;
                heroOnMapList.Add(spawnObj);
                spawnObj.RestoreHP();
            }
        }
    }
    private T SpawnInArea<T>(String unit)
    {
        var obj = ObjectPooling.Instance.GetFromPool(unit, RandomSpawnPointFromMarker(), Quaternion.identity);
        return obj.GetComponent<T>();
    }
    
    private void SetUpChancePool(List<String> pool,List<UnitSpawnChance> unitChances)
    {
        pool.Clear();
        foreach(var u in unitChances)
        {
            for(int i = 0; i < u.spawnChance; i++)
            {
                pool.Add(u.unit);
            }
        }
    }
    private String RandomUnitType(List<String> pool)
    {
        return pool[UnityEngine.Random.Range(0, pool.Count)];
    }
    private Vector2 RandomSpawnPointFromMarker()
    {
        bool isSafePosition = false;
        int xPosition = 0;
        int yPosition = 0;
        while (!isSafePosition)
        {
            xPosition = UnityEngine.Random.Range(GetPoint(Point.minX), GetPoint(Point.maxX));
            yPosition = UnityEngine.Random.Range(GetPoint(Point.minY), GetPoint(Point.maxY));

            var checkPosition = new Vector2(xPosition, yPosition);
            isSafePosition = player.IsPositionNotUsed(checkPosition)
            && IsPositionNotUsed(checkPosition, enemyOnMapList.Select(t => (Vector2)t.transform.position).ToList())
            && IsPositionNotUsed(checkPosition, heroOnMapList.Select(t => (Vector2)t.transform.position).ToList());
        }
        Vector2 position = new Vector2(xPosition, yPosition);
        return position;
    }
    private int GetPoint(Point point)
    {
        switch (point)
        {
            case Point.minX: return (int)areaMarker.Min(x => x.transform.position.x); 
            case Point.maxX: return (int)areaMarker.Max(x => x.transform.position.x);
            case Point.minY: return (int)areaMarker.Min(y => y.transform.position.y);
            case Point.maxY: return (int)areaMarker.Max(y => y.transform.position.y);
            default: return 0;
        }
    }
    private enum Point
    {
        minX, maxX, minY, maxY
    }

    public bool IsPositionNotUsed(Vector2 position,List<Vector2> positionList)
    {
        if(positionList == null || positionList.Count == 0)
        {
            return true;
        }
        return !positionList.Contains(position);
    }
#endregion
}
