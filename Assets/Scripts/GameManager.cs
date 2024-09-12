using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance => instance;
    private static GameManager instance;

    [Header("Game Area")]
    [SerializeField] List<Transform> areaMarker = new List<Transform>();

    [Header("Game Start Setting")]
    [SerializeField] String startHero = "WarriorClass";
    [SerializeField] Transform startPosition;
    [SerializeField] int startEnemyCount;
    [SerializeField] int startRecruitHeroCount;
    [SerializeField] List<UnitSpawnChance> enemySpawnChance = new List<UnitSpawnChance>();
    [SerializeField] List<UnitSpawnChance> heroSpawnChance = new List<UnitSpawnChance>();

    [Header("Max Spawn Setting")]
    [SerializeField] int maxHero;
    [SerializeField] int maxEnemy;

    [Header("Wave Setting")]
    [SerializeField, Range(0,100)] int heroChanceWhenKilled;
    [SerializeField] float waveToAddMoreEnemy;

    private int currentWave;
    [SerializeField] private List<EnemyController> enemyOnMapList = new List<EnemyController>();
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
        SpawnEnemyUnit(startEnemyCount);
        SpawnHeroUnit(startRecruitHeroCount);
    }
    public void ResetGame()
    {
        currentWave = 1;
        enemyOnMapList.Clear();
        heroOnMapList.Clear();
    }
    public void EndGame()
    {
        Debug.Log("Game Over");
    }

    public void EnemyAction()
    {
        foreach(EnemyController enemy in enemyOnMapList)
        {

        }
    }
    public void DeadEnemyRemove(EnemyController enemy)
    {
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
        SpawnEnemyUnit(Mathf.Clamp(startEnemyCount + (int)(wave / waveToAddMoreEnemy), 0, maxEnemy));
    }
    public void SpawnEnemyUnit(int value)
    {
        for (int i = 0; i < value; i++) 
        {
            var unit = RandomUnitType(enemySpawnChance);
            if (unit != null)
            {
                enemyOnMapList.Add(SpawnInArea<EnemyController>(unit));
            }
        }
    }
    public void SpawnHeroUnit(int value)
    {
        for (int i = 0; i < value; i++) 
        {
            var unit = RandomUnitType(heroSpawnChance);
            if (unit != null)
            {
                SpawnInArea<HeroController>(unit);
            }
        }
    }
    private T SpawnInArea<T>(String unit)
    {
        var obj = ObjectPooling.Instance.GetFromPool(unit, RandomSpawnPointFromMarker(), Quaternion.identity);
        return obj.GetComponent<T>();
    }
    
    private String RandomUnitType(List<UnitSpawnChance> unitChances)
    {
        List<String> randomList = new List<String>();
        foreach(var u in unitChances)
        {
            for(int i = 0; i < u.spawnChance; i++)
            {
                randomList.Add(u.unit);
            }
        }
        var obj = randomList[UnityEngine.Random.Range(0, randomList.Count)];
        return obj;
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
            isSafePosition = PlayerManager.Instance.IsPositionNotUsed(checkPosition)
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
