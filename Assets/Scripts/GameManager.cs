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
    [SerializeField] MenuUIManager menuUIManager;

    [Header("Game Area")]
    [SerializeField] List<Transform> areaMarker = new List<Transform>();

    [Header("Game Start Setting")]
    [SerializeField] String startHero = "WarriorClass";
    [SerializeField] Transform startPosition;
    [SerializeField] int startEnemyCount;
    [SerializeField] int startRecruitHeroCount;
    [SerializeField] List<UnitSpawnChance> enemyTypeSpawnChance = new List<UnitSpawnChance>();
    [SerializeField] List<UnitSpawnChance> heroTypeSpawnChance = new List<UnitSpawnChance>();

    [Header("Max Spawn Setting")]
    [SerializeField] int maxHero;
    [SerializeField] int maxEnemy;

    [Header("Wave Setting")]
    [SerializeField] List<UnitSpawnChance> heroSpawnChance = new List<UnitSpawnChance>();
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
        PlayerManager.Instance.StartSetup(startHero, startPosition.position);
        SpawnEnemyUnit(startEnemyCount);
        SpawnHeroUnit(startRecruitHeroCount);
    }
    public void ResetGame()
    {
        currentWave = 1;
        RemoveHeroFromMap();
        RemoveEnemyFromMap();
    }
    public void EndGame()
    {
        PlayerManager.Instance.SetMoveable(false);
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
        SpawnEnemyUnit(Mathf.Clamp(startEnemyCount + (int)(wave / waveToAddMoreEnemy), 0, maxEnemy));
        if (PlayerManager.Instance.GetHeroCount() + heroOnMapList.Count < maxHero)
            SpawnHeroUnit(int.Parse(RandomUnitType(heroSpawnChance)));
    }
    public void SpawnEnemyUnit(int value)
    {
        for (int i = 0; i < value; i++) 
        {
            var unit = RandomUnitType(enemyTypeSpawnChance);
            if (unit != null)
            {
                var spawnObj = SpawnInArea<EnemyController>(unit);
                enemyOnMapList.Add(spawnObj);
                spawnObj.ResetValue();
            }
        }
    }
    public void SpawnHeroUnit(int value)
    {
        for (int i = 0; i < value; i++) 
        {
            var unit = RandomUnitType(heroTypeSpawnChance);
            if (unit != null)
            {
                var spawnObj = SpawnInArea<HeroController>(unit);
                heroOnMapList.Add(spawnObj);
                spawnObj.ResetValue();
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
