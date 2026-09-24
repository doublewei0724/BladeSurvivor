using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BladeSurvivor {
[Serializable] public class BattleTelemetry {
    public int stage,hits,deaths,revives,dodges,swipes,charges,chargeInterruptions,kills,hpPickups,spPickups;
    public bool cleared;public float time,spLowSeconds,bossSpawnTime,bossKillTime;public int bossSpawnRemaining;public int[] waveRemaining=new int[10];
    public int stars,jade;public int[] stones=new int[4];public string equipmentRarity;
}
public class BattleController:MonoBehaviour {
    public GameBalance Balance;public ProgressionService Progression;public GameRoot Root;
    public PlayerFighter Player;public BattleCamera Camera;public CombatEffects Effects;public GameAudio Audio;
    public readonly List<EnemyActor> Enemies=new List<EnemyActor>();public readonly BattleRewards Pending=new BattleRewards();
    public bool Running,Paused,AwaitingRevive,Finished;public int StageIndex,Wave,Kills,Hits,Deaths,Revives,ResultStars,RewardExp,RewardGold;public float Elapsed;
    public EquipmentItem RewardEquipment;public BattleTelemetry Telemetry;public string BattleId;
    public int AliveCount {get{int n=0;foreach(var e in Enemies)if(e.Alive)n++;return n;}}
    public EnemyActor Boss {get{foreach(var e in Enemies)if(e.Alive&&e.Kind==EnemyKind.Boss)return e;return null;}}
    public class Pickup {public GameObject go;public PickupKind kind;public bool active;public Vector3 origin;public float phase;}
    class Shot {public GameObject go;public bool active,player;public Vector3 direction;public float speed,distance,damage,healed;public readonly HashSet<EnemyActor> hit=new HashSet<EnemyActor>();}
    public readonly List<Pickup> Pickups=new List<Pickup>();readonly List<Shot> shots=new List<Shot>();GameObject world;float lowSpTime;
    public void Initialize(GameRoot root){Root=root;Balance=root.Balance;Progression=root.Progression;Camera=root.BattleCamera;Effects=root.Effects;Audio=root.Audio;world=ArenaBuilder.Build(Balance.arenaHalfSize);world.SetActive(false);}
    public void StartBattle(int index){ResetWorld();StageIndex=index;Wave=Kills=Hits=Deaths=Revives=0;Elapsed=lowSpTime=0;ResultStars=0;Pending.Clear();RewardEquipment=null;BattleId=Guid.NewGuid().ToString("N");Telemetry=new BattleTelemetry{stage=index};
        Running=true;Paused=false;Finished=false;AwaitingRevive=false;world.SetActive(true);
        var go=new GameObject("Player");go.transform.SetParent(transform);Art.Humanoid(-1-(int)Progression.Weapon.weapon,go.transform);Player=go.AddComponent<PlayerFighter>();Player.Initialize(this);Camera.target=Player.transform;Camera.SnapToTarget();
    // 可調數值【預熱物件池】衛兵 75、其餘普通類各 30、Boss 1；拾取物 64、投射物 40；不足會擴充。
        if(Enemies.Count==0)for(int k=0;k<5;k++)for(int i=0;i<(k==4?1:k==0?75:30);i++)CreateEnemy((EnemyKind)k);
        if(Pickups.Count==0)for(int i=0;i<64;i++){Drop(PickupKind.SmallStone,new Vector3(0,-100,0));Pickups[Pickups.Count-1].active=true;}
        foreach(var pickup in Pickups){pickup.active=false;pickup.go.SetActive(false);}Effects.Clear();
        if(shots.Count==0)for(int i=0;i<40;i++)Projectile(new Vector3(0,-100,0),Vector3.forward,1,1,0,false);
        foreach(var shot in shots){shot.active=false;shot.go.SetActive(false);}
        SpawnWave();Audio.Volume(Progression.Save.volume);
    }
    void ResetWorld(){Time.timeScale=1;if(Player!=null)Destroy(Player.gameObject);foreach(var e in Enemies){e.State=EnemyState.Dead;e.gameObject.SetActive(false);}foreach(var p in Pickups){p.active=false;p.go.SetActive(false);}foreach(var s in shots){s.active=false;s.go.SetActive(false);}Effects.Clear();}
    public void Leave(){Running=false;Paused=false;AwaitingRevive=false;ResetWorld();world.SetActive(false);Camera.target=null;}
    EnemyActor CreateEnemy(EnemyKind kind){var go=new GameObject("Pooled "+kind);go.transform.SetParent(transform);Art.Humanoid((int)kind,go.transform);var e=go.AddComponent<EnemyActor>();e.Kind=kind;e.State=EnemyState.Dead;go.SetActive(false);Enemies.Add(e);return e;}
    // 可調數值【Boss 替換位置】最後一波第 10 位（索引 9）換 Boss；變更每波數量時需同步這個位置與公告的第 10 波。
    void SpawnWave(){if(Wave>0)Telemetry.waveRemaining[Wave-1]=AliveCount;Wave++;
        for(int i=0;i<Balance.enemiesPerWave;i++){
            EnemyKind kind=Balance.waves[Wave-1].At(i);
            if(Wave==Balance.waveCount&&Balance.stages[StageIndex].boss&&i==9)kind=EnemyKind.Boss;
            EnemyActor actor=null;foreach(var e in Enemies)if(e.Kind==kind&&!e.gameObject.activeSelf){actor=e;break;}if(actor==null)actor=CreateEnemy(kind);
            Vector3 p=SpawnPosition(i);
            actor.transform.localScale=Vector3.one;actor.Initialize(this,kind,Balance.stages[StageIndex].level,p);Effects.Ring(p,1,new Color(.65f,.15f,.1f),.6f);
            if(kind==EnemyKind.Boss){Telemetry.bossSpawnTime=Elapsed;Telemetry.bossSpawnRemaining=AliveCount;}
        }
        Root.UI.Announce(Wave==10&&Balance.stages[StageIndex].boss?"巨斧亡將 · 已降臨":$"第 {Wave:00} 波 · 敵人來襲");
    }
    public Vector3 SpawnPosition(int index){
        var camera=Camera.GetComponent<UnityEngine.Camera>();var plane=new Plane(Vector3.up,Vector3.zero);
        // Project an expanded viewport perimeter onto the ground, rather than using map edges.
    // 可調數值【畫面外出生】最多嘗試 40 次；均勻分散角 137.5°，初始間隔 36°、波次偏轉 47°。
        for(int attempt=0;attempt<40;attempt++){
            float angle=(index*36+Wave*47+attempt*137.5f)*Mathf.Deg2Rad;
    // 可調數值【出生離畫面距離】.68 為螢幕中心到外框距離，.5 剛好螢幕邊；出生需離地圖邊 1m。
            Vector2 dir=new Vector2(Mathf.Sin(angle),Mathf.Cos(angle));dir*=.68f/Mathf.Max(Mathf.Abs(dir.x),Mathf.Abs(dir.y));
            var ray=camera.ViewportPointToRay(new Vector3(.5f+dir.x,.5f+dir.y,0));
            if(!plane.Raycast(ray,out float distance))continue;Vector3 point=ray.GetPoint(distance);point.y=0;
            if(Mathf.Abs(point.x)<=Balance.arenaHalfSize.x-1&&Mathf.Abs(point.z)<=Balance.arenaHalfSize.y-1)return point;
        }
        // Valid for unusually narrow custom maps as well.
    // 可調數值【出生備援】極小自訂地圖找不到外框時，取玩家前方 20m 並限制在地圖內 1m。
        Vector3 fallback=Player.transform.position+Vector3.forward*20;
        fallback.x=Mathf.Clamp(fallback.x,-Balance.arenaHalfSize.x+1,Balance.arenaHalfSize.x-1);
        fallback.z=Mathf.Clamp(fallback.z,-Balance.arenaHalfSize.y+1,Balance.arenaHalfSize.y-1);return fallback;
    }
    void Update(){if(!Running||Paused||AwaitingRevive||Finished||Player==null)return;float dt=Time.deltaTime;Elapsed+=dt;if(Player.SP<Player.Stats.maxSP*.2f)lowSpTime+=dt;
        if(Wave<Balance.waveCount&&Elapsed>=Wave*Balance.waveInterval)SpawnWave();
        Player.Tick(dt);if(!Running||AwaitingRevive)return;
        for(int i=0;i<Enemies.Count;i++)if(Enemies[i].gameObject.activeSelf)Enemies[i].Tick(dt);
        ResolveEnemySpacing();
        if(AwaitingRevive||!Running)return;
        TickShots(dt);TickPickups(dt);
        if(Kills>=Balance.waveCount*Balance.enemiesPerWave&&Wave==Balance.waveCount&&!Finished)Complete();
    }
    // 可調數值【防重疊效能】預配 256 敵人位置，可自動擴充；最多迭代 24 次。
    readonly List<EnemyActor> spacingActors=new List<EnemyActor>(256);
    Vector3[] spacingPositions=new Vector3[256];
    public void ResolveEnemySpacing(){
        spacingActors.Clear();foreach(var enemy in Enemies)if(enemy.Alive)spacingActors.Add(enemy);
        int count=spacingActors.Count;if(spacingPositions.Length<count)Array.Resize(ref spacingPositions,count*2);
        for(int i=0;i<count;i++)spacingPositions[i]=spacingActors[i].transform.position;
        // Resolve bodies in every living state; weapons and attack arcs do not block movement.
        for(int iteration=0;iteration<24;iteration++){
            float worst=0;
            for(int i=0;i<count;i++)for(int j=i+1;j<count;j++){
                Vector3 delta=spacingPositions[j]-spacingPositions[i];delta.y=0;
    // 可調數值【敵人間距】身體半徑和之外保留 .06m；每次雙方各推一半，加 .0005m 誤差補償。
                float minimum=spacingActors[i].BodyRadius+spacingActors[j].BodyRadius+.06f;
                float squared=delta.sqrMagnitude;if(squared>=minimum*minimum)continue;
                float distance=Mathf.Sqrt(squared),penetration=minimum-distance;worst=Mathf.Max(worst,penetration);
                Vector3 direction=distance>.0001f?delta/distance:new Vector3(Mathf.Sin((i*37+j*61)*2.39996f),0,Mathf.Cos((i*37+j*61)*2.39996f));
                Vector3 correction=direction*(penetration*.5f+.0005f);
                spacingPositions[i]-=correction;spacingPositions[j]+=correction;
            }
            for(int i=0;i<count;i++){var p=spacingPositions[i];p.x=Mathf.Clamp(p.x,-Balance.arenaHalfSize.x,Balance.arenaHalfSize.x);p.z=Mathf.Clamp(p.z,-Balance.arenaHalfSize.y,Balance.arenaHalfSize.y);p.y=0;if(Player!=null&&Player.KnockedDown){Vector3 away=p-Player.transform.position;float minimum=Player.RecoveryRadius+spacingActors[i].BodyRadius;if(away.sqrMagnitude<minimum*minimum){if(away.sqrMagnitude<.0001f)away=new Vector3(Mathf.Sin(i*2.4f),0,Mathf.Cos(i*2.4f));p=Player.transform.position+away.normalized*minimum;}}spacingPositions[i]=p;}
    // 可調數值【間距求解精度】最壞重疊小於 .002m 提前停止，避免不必要迭代。
            if(worst<.002f)break;
        }
        for(int i=0;i<count;i++)spacingActors[i].transform.position=spacingPositions[i];
    }
    public EnemyActor Nearest(Vector3 p,float range){EnemyActor result=null;float best=range*range;foreach(var e in Enemies)if(e.Alive){float distance=(e.transform.position-p).sqrMagnitude;if(distance<best){best=distance;result=e;}}return result;}
    // 可調數值【重型掉石加成】一般強化石機率 ×1.5；基礎掉率在 GameBalance，Boss 石頭採累積機率抽一種。
    public void EnemyDied(EnemyActor enemy){Kills++;bool boss=enemy.Kind==EnemyKind.Boss;Vector3 p=enemy.transform.position;
        if(boss){float r=UnityEngine.Random.value,total=0;for(int i=0;i<4;i++){total+=Balance.bossStoneChances[i];if(r<=total){Drop((PickupKind)(i+2),p);break;}}Telemetry.bossKillTime=Elapsed-Telemetry.bossSpawnTime;}
        else {if(UnityEngine.Random.value<Balance.hpPickupChance)Drop(PickupKind.Health,p);if(UnityEngine.Random.value<Balance.spPickupChance)Drop(PickupKind.Spirit,p);
            for(int i=0;i<4;i++)if(UnityEngine.Random.value<Balance.stoneChances[i]*(enemy.Kind==EnemyKind.Heavy?1.5f:1))Drop((PickupKind)(i+2),p);}
        if(UnityEngine.Random.value<(boss?Balance.bossJadeChance:Balance.jadeChance))Drop(PickupKind.Jade,p);
    }
    // 可調數值【掉落外觀】尺寸 .3×.42×.3m，落點隨機偏移 ±.3m、高 .32m；幾何 5 邊 3 環。
    public void Drop(PickupKind kind,Vector3 p){Pickup item=Pickups.Find(x=>!x.active);if(item==null){var geo=new Geometry();geo.Ellipsoid(Vector3.zero,new Vector3(.3f,.42f,.3f),Color.white,5,3);var go=geo.Object("Pooled pickup",transform,Art.Material);item=new Pickup{go=go};Pickups.Add(item);}
        item.kind=kind;item.active=true;item.origin=p+new Vector3(UnityEngine.Random.Range(-.3f,.3f),.32f,UnityEngine.Random.Range(-.3f,.3f));item.go.transform.position=item.origin;item.go.SetActive(true);item.phase=UnityEngine.Random.value*6;
        var block=new MaterialPropertyBlock();block.SetColor("_Color",PickupColor(kind));item.go.GetComponent<Renderer>().SetPropertyBlock(block);Effects.Ring(p,.3f,PickupColor(kind),.35f);
    }
    public static Color PickupColor(PickupKind kind)=>kind==PickupKind.Health?new Color(1,.14f,.18f):kind==PickupKind.Spirit?new Color(.15f,.6f,1):kind==PickupKind.Jade?new Color(.12f,1,.65f):kind==PickupKind.GiantStone?new Color(1,.55f,.1f):kind==PickupKind.LargeStone?new Color(.8f,.2f,1):kind==PickupKind.MediumStone?new Color(.3f,.8f,1):new Color(.4f,.4f,1);
    // 可調數值【拾取吸附】吸附速度 9m/s、拾取距離 .65m；上下漂浮振幅 .075m、每秒旋轉 90°。
    void TickPickups(float dt){foreach(var p in Pickups){if(!p.active)continue;float distance=Vector3.Distance(Player.transform.position,p.go.transform.position);float radius=(int)p.kind>=4?Balance.valuablePickupRadius:Balance.pickupRadius;
        if(distance<radius)p.origin=Vector3.MoveTowards(p.origin,Player.transform.position+Vector3.up*.25f,dt*9);
        p.phase+=dt*3;p.go.transform.position=p.origin+Vector3.up*Mathf.Sin(p.phase)*.075f;p.go.transform.Rotate(0,dt*90,0);
        if(distance<.65f){Collect(p.kind);p.active=false;p.go.SetActive(false);}}}
    void Collect(PickupKind kind){switch(kind){case PickupKind.Health:Player.HP=Mathf.Min(Player.Stats.maxHP,Player.HP+Player.Stats.maxHP*Balance.hpRestore*Player.Stats.pickupMultiplier);Telemetry.hpPickups++;break;
        case PickupKind.Spirit:Player.SP=Mathf.Min(Player.Stats.maxSP,Player.SP+Player.Stats.maxSP*Balance.spRestore*Player.Stats.pickupMultiplier);Telemetry.spPickups++;break;
        case PickupKind.Jade:Pending.jade++;break;default:Pending.stones[(int)kind-2]++;break;}Audio.Play(2);}
    // 可調數值【投射物外觀】箭長 .7m、寬 .06m；玩家劍氣模型縮放 6／1／1.8；與命中寬度分開。
    public void Projectile(Vector3 origin,Vector3 direction,float speed,float distance,float damage,bool player){var s=shots.Find(x=>!x.active);if(s==null){var g=new Geometry();g.Blade(new Vector3(0,0,-.25f),.7f,.06f,Color.white);s=new Shot{go=g.Object("Pooled projectile",transform,Art.Material)};shots.Add(s);}
        s.active=true;s.player=player;s.direction=direction.normalized;s.speed=speed;s.distance=distance;s.damage=damage;s.healed=0;s.hit.Clear();s.go.transform.position=origin;s.go.transform.forward=s.direction;s.go.SetActive(true);
        s.go.transform.localScale=player?new Vector3(6,1,1.8f):Vector3.one;var block=new MaterialPropertyBlock();block.SetColor("_Color",player?new Color(.3f,.85f,1):new Color(1,.4f,.1f));s.go.GetComponent<Renderer>().SetPropertyBlock(block);}
    // 可調數值【投射物判定】玩家劍氣命中額外半寬 .5m、擊退 .3m；敵人箭對玩家命中半徑 .45m。
    void TickShots(float dt){foreach(var s in shots){if(!s.active)continue;Vector3 before=s.go.transform.position;s.go.transform.position+=s.direction*s.speed*dt;s.distance-=s.speed*dt;Vector3 p=s.go.transform.position;p.y=0;before.y=0;
        if(s.player){foreach(var e in Enemies)if(e.Alive&&!s.hit.Contains(e)&&SegmentDistance(e.transform.position,before,p)<e.Radius+.5f){s.hit.Add(e);s.healed+=Player.ApplyLifeSteal(Player.Hit(e,s.damage,.3f),s.healed);}}
        else if(SegmentDistance(Player.transform.position,before,p)<.45f){Player.Receive(s.damage,before-s.direction);s.distance=0;}
        if(s.distance<=0){s.active=false;s.go.SetActive(false);}}}
    static float SegmentDistance(Vector3 point,Vector3 a,Vector3 b){var ab=b-a;return Vector3.Distance(point,a+ab*Mathf.Clamp01(Vector3.Dot(point-a,ab)/Mathf.Max(.0001f,ab.sqrMagnitude)));}
    // 可調數值【死亡次數】第三次死亡失敗；第二次復活花費勾玉，第一次走模擬廣告。
    public void PlayerDied(){Deaths++;if(Deaths>=3){Fail();return;}AwaitingRevive=true;Time.timeScale=0;Root.UI.OpenRevive();}
    public bool Revive(){if(!AwaitingRevive)return false;if(Deaths==2&&!Progression.SpendJade())return false;Revives++;AwaitingRevive=false;Time.timeScale=1;Player.Revive();return true;}
    public void Pause(bool paused){if(!Running||Finished||AwaitingRevive)return;Paused=paused;Time.timeScale=paused?0:1;if(paused)Player.CancelCharge();}
    void Complete(){Finished=true;Running=false;Time.timeScale=1;ResultStars=ProgressionRules.Stars(Elapsed,Hits,Balance);RewardEquipment=Progression.Commit(BattleId,StageIndex,ResultStars,Pending,out RewardExp,out RewardGold);WriteTelemetry(true);Audio.Play(3);Root.ShowResult(true);}
    public void Fail(){Finished=true;Running=false;AwaitingRevive=false;Time.timeScale=1;Pending.Clear();WriteTelemetry(false);Root.ShowResult(false);}
    void WriteTelemetry(bool cleared){Telemetry.cleared=cleared;Telemetry.time=Elapsed;Telemetry.hits=Hits;Telemetry.deaths=Deaths;Telemetry.revives=Revives;Telemetry.kills=Kills;Telemetry.dodges=Player.DodgeCount;Telemetry.swipes=Player.SwipeCount;Telemetry.charges=Player.ChargeCount;Telemetry.chargeInterruptions=Player.InterruptedCharges;Telemetry.spLowSeconds=lowSpTime;
        Telemetry.waveRemaining[Mathf.Clamp(Wave-1,0,9)]=AliveCount;Telemetry.stars=ResultStars;Telemetry.jade=Pending.jade;Array.Copy(Pending.stones,Telemetry.stones,4);Telemetry.equipmentRarity=RewardEquipment?.rarity.ToString();string json=JsonUtility.ToJson(Telemetry,true);Debug.Log("BATTLE_TELEMETRY "+json);try{File.WriteAllText(Path.Combine(Application.persistentDataPath,"last-battle.json"),json);}catch(Exception e){Debug.LogWarning(e.Message);}}
}
}
