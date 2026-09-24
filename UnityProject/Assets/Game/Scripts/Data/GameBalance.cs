using System;
using UnityEngine;

namespace BladeSurvivor {
public enum EnemyKind { Soldier, Assassin, Archer, Heavy, Boss }
public enum EquipmentKind { Weapon, Suit }
public enum Rarity { Common, Uncommon, Rare, Epic, Legendary }
public enum WeaponKind { Sword, DualBlades, Greatsword }
public enum PickupKind { Health, Spirit, SmallStone, MediumStone, LargeStone, GiantStone, Jade }

[Serializable] public class EnemySpec {
    public EnemyKind kind; public string title;
    // 可調數值【舊版基礎生命描述；實際生命讀取 GameBalance 等級曲線】
    [Tooltip("舊版基礎生命描述；實際生命讀取 GameBalance 等級曲線")] public float hp;
    // 可調數值【舊版基礎傷害描述；實際傷害讀取等級曲線】
    [Tooltip("舊版基礎傷害描述；實際傷害讀取等級曲線")] public float damage;
    // 可調數值【移動速度（公尺／秒）】
    [Tooltip("移動速度（公尺／秒）")] public float speed;
    // 可調數值【攻擊距離／技能範圍（公尺）】
    [Tooltip("攻擊距離／技能範圍（公尺）")] public float range;
    // 可調數值【攻擊前搖（秒）】
    [Tooltip("攻擊前搖（秒）")] public float windup;
    // 可調數值【攻擊後搖（秒）】
    [Tooltip("攻擊後搖（秒）")] public float recovery;
    // 可調數值【攻擊或技能冷卻（秒）】
    [Tooltip("攻擊或技能冷卻（秒）")] public float cooldown;
    // 可調數值【受擊退倍率；0 免疫普通擊退，1 正常，仍可被特殊招式擊倒】
    [Tooltip("受擊退倍率；0 免疫普通擊退，1 正常，仍可被特殊招式擊倒")] public float knockback;
    public EnemySpec(EnemyKind k,string n,float h,float d,float s,float r,float w,float rec,float cd,float kb) {
        kind=k; title=n; hp=h; damage=d; speed=s; range=r; windup=w; recovery=rec; cooldown=cd; knockback=kb;
    }
}
[Serializable] public class SkillSpec {
    public string title, description;     // 可調數值【技能購買所需等級】
    [Tooltip("技能購買所需等級")] public int unlock;
    // 可調數值【技能購買金幣】
    [Tooltip("技能購買金幣")] public int cost;     // 可調數值【技能基礎傷害倍率】
    [Tooltip("技能基礎傷害倍率")] public float multiplier;
    // 可調數值【攻擊距離／技能範圍（公尺）】
    [Tooltip("攻擊距離／技能範圍（公尺）")] public float range;
    // 可調數值【技能角度（度）；特殊技能仍有 PlayerFighter 的行為設定】
    [Tooltip("技能角度（度）；特殊技能仍有 PlayerFighter 的行為設定")] public float arc;
    // 可調數值【攻擊或技能冷卻（秒）】
    [Tooltip("攻擊或技能冷卻（秒）")] public float cooldown;
    // 可調數值【技能持續時間（秒）】
    [Tooltip("技能持續時間（秒）")] public float duration;
    public SkillSpec(string n,string desc,int level,int price,float mult,float r,float a,float cd,float dur=0) {
        title=n; description=desc; unlock=level; cost=price; multiplier=mult; range=r; arc=a; cooldown=cd; duration=dur;
    }
}
[Serializable] public class StageSpec {
    public string title, subtitle, story;     // 可調數值【此關敵人等級】
    [Tooltip("此關敵人等級")] public int level; public bool boss;
}
[Serializable] public class WaveSpec {
    // 可調數值【每波敵人數】依序：一般衛兵、刺客、弩手、重型；四者合計等於 enemiesPerWave。
    [Tooltip("一般衛兵數")] public int normal;
    [Tooltip("刺客數")] public int speed;
    [Tooltip("弩手數")] public int ranged;
    [Tooltip("重型數")] public int heavy;
    public WaveSpec(int n,int s,int r,int h){normal=n;speed=s;ranged=r;heavy=h;}
    public EnemyKind At(int index)=>index<normal?EnemyKind.Soldier:index<normal+speed?EnemyKind.Assassin:index<normal+speed+ranged?EnemyKind.Archer:EnemyKind.Heavy;
}

[CreateAssetMenu(menuName="Blade Survivor/Game Balance")]
public class GameBalance : ScriptableObject {
    // 調整入口：Assets/Game/Resources/GameBalance.asset（Unity Inspector）。
    // 下列初始化數字只用於新資產／缺少欄位；已有資產中的數值優先，不會因改 .cs 預設值自動更新。
    // 距離單位為世界公尺、時間為秒、比例 0.01 = 1%。搜尋「可調數值」可定位中文說明。
    [Header("玩家基礎")]
    
    // 可調數值【玩家初始生命上限（HP）】搜尋：baseHP
    [Tooltip("玩家初始生命上限（HP）")]
    public float baseHP=100;

    // 可調數值【玩家初始精力上限（SP）】搜尋：baseSP
    [Tooltip("玩家初始精力上限（SP）")]
    public float baseSP=100;

    // 可調數值【正常移動速度（公尺／秒）】搜尋：moveSpeed
    [Tooltip("正常移動速度（公尺／秒）")]
    public float moveSpeed=5;

    // 可調數值【基礎暴擊機率，0.05 代表 5%】搜尋：critChance
    [Tooltip("基礎暴擊機率，0.05 代表 5%")]
    public float critChance=.05f;

    // 可調數值【暴擊傷害倍率，1.5 代表 150%】搜尋：critMultiplier
    [Tooltip("暴擊傷害倍率，1.5 代表 150%")]
    public float critMultiplier=1.5f;

    
    // 可調數值【自動尋敵半徑（公尺）】搜尋：scanRange
    [Tooltip("自動尋敵半徑（公尺）")]
    public float scanRange=5;

    // 可調數值【重新尋敵間隔（秒）】搜尋：scanInterval
    [Tooltip("重新尋敵間隔（秒）")]
    public float scanInterval=.1f;

    [Header("武器／連擊／倒地")]
    
    // 可調數值【武器傷害倍率；索引依序為長劍、雙刀、重劍】搜尋：weaponMultipliers
    [Tooltip("武器傷害倍率；索引依序為長劍、雙刀、重劍")]
    public float[] weaponMultipliers={1,.75f,1.45f};

    // 可調數值【普攻起手間隔（秒）；依序長劍、雙刀、重劍；連招接續沿用此間隔】搜尋：attackIntervals
    [Tooltip("普攻起手間隔（秒）；依序長劍、雙刀、重劍；連招接續沿用此間隔")]
    public float[] attackIntervals={.6f,.55f,1.25f};

    // 可調數值【普攻半徑（公尺）；依序長劍、雙刀、重劍】搜尋：weaponRanges
    [Tooltip("普攻半徑（公尺）；依序長劍、雙刀、重劍")]
    public float[] weaponRanges={3f,1.55f,2.4f};

    // 可調數值【普攻總角度（度）；120 代表左右各 60 度；依序長劍、雙刀、重劍】搜尋：weaponArcs
    [Tooltip("普攻總角度（度）；120 代表左右各 60 度；依序長劍、雙刀、重劍")]
    public float[] weaponArcs={120,100,160};

    // 可調數值【普攻基礎擊退（公尺）；依序長劍、雙刀、重劍；實際位移受抗性與衰減影響】搜尋：weaponKnockbacks
    [Tooltip("普攻基礎擊退（公尺）；依序長劍、雙刀、重劍；實際位移受抗性與衰減影響")]
    public float[] weaponKnockbacks={.55f,.25f,.7f};

    
    // 可調數值【連擊第四下最高跳躍高度（公尺）】搜尋：comboJumpHeight
    [Tooltip("連擊第四下最高跳躍高度（公尺）")]
    public float comboJumpHeight=1.1f;

    // 可調數值【連擊第四下向前位移（公尺）】搜尋：comboJumpDistance
    [Tooltip("連擊第四下向前位移（公尺）")]
    public float comboJumpDistance=1.1f;

    // 可調數值【連擊第四下起跳至落地時間（秒）；全程無敵】搜尋：comboJumpDuration
    [Tooltip("連擊第四下起跳至落地時間（秒）；全程無敵")]
    public float comboJumpDuration=.4f;

    // 可調數值【蓄力矩形向前長度（公尺），三段共用】搜尋：chargeLength
    [Tooltip("蓄力矩形向前長度（公尺），三段共用")]
    public float chargeLength=6f;

    // 可調數值【蓄力矩形總寬度（公尺），不是單側寬度】搜尋：chargeWidth
    [Tooltip("蓄力矩形總寬度（公尺），不是單側寬度")]
    public float chargeWidth=2.5f;

    // 可調數值【敵人被擊倒至站起時間（秒），蓄力／突進／第四擊共用】搜尋：enemyKnockdownDuration
    [Tooltip("敵人被擊倒至站起時間（秒），蓄力／突進／第四擊共用")]
    public float enemyKnockdownDuration=2f;

    
    // 可調數值【玩家受擊後退到站起總時間（秒），期間完全無敵】搜尋：knockdownDuration
    [Tooltip("玩家受擊後退到站起總時間（秒），期間完全無敵")]
    public float knockdownDuration=2f;

    // 可調數值【玩家有效受擊時向後位移（公尺）】搜尋：knockdownDistance
    [Tooltip("玩家有效受擊時向後位移（公尺）")]
    public float knockdownDistance=1f;

    // 可調數值【玩家倒地起身期間敵人隔離半徑（公尺），另加敵人身體半徑】搜尋：recoveryRadius
    [Tooltip("玩家倒地起身期間敵人隔離半徑（公尺），另加敵人身體半徑")]
    public float recoveryRadius=2f;

    
    // 可調數值【普通攻擊起手至傷害判定的前搖（秒）】搜尋：attackWindup
    [Tooltip("普通攻擊起手至傷害判定的前搖（秒）")]
    public float attackWindup=.15f;

    // 可調數值【舊版預留命中窗（秒）；目前普攻採前搖結束單次判定，調此值不生效】搜尋：attackActive
    [Tooltip("舊版預留命中窗（秒）；目前普攻採前搖結束單次判定，調此值不生效")]
    public float attackActive=.12f;

    // 可調數值【普攻動作期間保留的移速比例；0.45 代表剩 45%，不是減少 45%】搜尋：attackMoveMultiplier
    [Tooltip("普攻動作期間保留的移速比例；0.45 代表剩 45%，不是減少 45%")]
    public float attackMoveMultiplier=.45f;

    [Header("手勢／閃避／突進／蓄力")]
    
    // 可調數值【雙擊閃避辨識時限（秒）】搜尋：doubleTapWindow
    [Tooltip("雙擊閃避辨識時限（秒）")]
    public float doubleTapWindow=.24f;

    // 可調數值【快速滑動最小距離占螢幕短邊比例；0.08 代表 8%】搜尋：swipeScreenFraction
    [Tooltip("快速滑動最小距離占螢幕短邊比例；0.08 代表 8%")]
    public float swipeScreenFraction=.08f;

    
    // 可調數值【閃避基本精力消耗，裝備詞條可降低】搜尋：dodgeSP
    [Tooltip("閃避基本精力消耗，裝備詞條可降低")]
    public float dodgeSP=20;

    // 可調數值【閃避移動時間（秒），整段無敵】搜尋：dodgeDuration
    [Tooltip("閃避移動時間（秒），整段無敵")]
    public float dodgeDuration=.45f;

    // 可調數值【舊版局部閃避無敵起點；目前整段閃避無敵，調此值不生效】搜尋：dodgeIFrameStart
    [Tooltip("舊版局部閃避無敵起點；目前整段閃避無敵，調此值不生效")]
    public float dodgeIFrameStart=.1f;

    // 可調數值【舊版局部閃避無敵終點；目前整段閃避無敵，調此值不生效】搜尋：dodgeIFrameEnd
    [Tooltip("舊版局部閃避無敵終點；目前整段閃避無敵，調此值不生效")]
    public float dodgeIFrameEnd=.35f;

    // 可調數值【閃避結束後額外冷卻（秒）】搜尋：dodgeCooldown
    [Tooltip("閃避結束後額外冷卻（秒）")]
    public float dodgeCooldown=.35f;

    
    // 可調數值【雙擊閃避總移動距離（公尺）】搜尋：dodgeDistance
    [Tooltip("雙擊閃避總移動距離（公尺）")]
    public float dodgeDistance=6f;

    // 可調數值【滑動突進基本精力消耗】搜尋：swipeSP
    [Tooltip("滑動突進基本精力消耗")]
    public float swipeSP=35;

    // 可調數值【滑動突進起手後冷卻（秒）】搜尋：swipeCooldown
    [Tooltip("滑動突進起手後冷卻（秒）")]
    public float swipeCooldown=.8f;

    // 可調數值【滑動突進總移動距離（公尺）】搜尋：swipeDistance
    [Tooltip("滑動突進總移動距離（公尺）")]
    public float swipeDistance=6;

    // 可調數值【滑動突進傷害路徑總寬度（公尺），另計敵人命中半徑】搜尋：swipeWidth
    [Tooltip("滑動突進傷害路徑總寬度（公尺），另計敵人命中半徑")]
    public float swipeWidth=2f;

    // 可調數值【滑動突進傷害倍率，1.25 代表普通傷害的 125%】搜尋：swipeMultiplier
    [Tooltip("滑動突進傷害倍率，1.25 代表普通傷害的 125%")]
    public float swipeMultiplier=1.25f;

    
    // 可調數值【進入第一段蓄力所需長按時間（秒）；蓄力加速詞條會按比例縮短】搜尋：chargeStart
    [Tooltip("進入第一段蓄力所需長按時間（秒）；蓄力加速詞條會按比例縮短")]
    public float chargeStart=.5f;

    // 可調數值【第二段蓄力時間門檻（秒）】搜尋：chargeSecond
    [Tooltip("第二段蓄力時間門檻（秒）")]
    public float chargeSecond=1.5f;

    // 可調數值【第三段滿蓄力時間門檻（秒）；滿蓄力不會自動出招】搜尋：chargeFull
    [Tooltip("第三段滿蓄力時間門檻（秒）；滿蓄力不會自動出招")]
    public float chargeFull=3;

    // 可調數值【舊版蓄力移速比例；目前蓄力站定，調此值不生效】搜尋：chargeMoveMultiplier
    [Tooltip("舊版蓄力移速比例；目前蓄力站定，調此值不生效")]
    public float chargeMoveMultiplier=.35f;

    
    // 可調數值【三段蓄力傷害倍率，依序第一／第二／滿蓄力】搜尋：chargeMultipliers
    [Tooltip("三段蓄力傷害倍率，依序第一／第二／滿蓄力")]
    public float[] chargeMultipliers={1.5f,2.2f,3};

    // 可調數值【舊版三段蓄力角度；目前用矩形 chargeLength／chargeWidth，調此陣列不生效】搜尋：chargeArcs
    [Tooltip("舊版三段蓄力角度；目前用矩形 chargeLength／chargeWidth，調此陣列不生效")]
    public float[] chargeArcs={160,240,360};

    // 可調數值【舊版三段蓄力半徑；目前用矩形 chargeLength／chargeWidth，調此陣列不生效】搜尋：chargeRanges
    [Tooltip("舊版三段蓄力半徑；目前用矩形 chargeLength／chargeWidth，調此陣列不生效")]
    public float[] chargeRanges={2.5f,3.2f,4};

    // 可調數值【三段蓄力基礎擊退；現行擊倒會清除擊退速度，主要用 enemyKnockdownDuration 調倒地】搜尋：chargeKnockbacks
    [Tooltip("三段蓄力基礎擊退；現行擊倒會清除擊退速度，主要用 enemyKnockdownDuration 調倒地")]
    public float[] chargeKnockbacks={.8f,1.5f,2.2f};

    [Header("關卡／地圖／星等")]
    
    // 可調數值【每關波數；需與 waves 陣列長度一致，改動亦需同步結算／UI 的十波設定】搜尋：waveCount
    [Tooltip("每關波數；需與 waves 陣列長度一致，改動亦需同步結算／UI 的十波設定")]
    public int waveCount=10;

    // 可調數值【每波敵人數；需與每一筆 waves 四種敵人數量總和一致】搜尋：enemiesPerWave
    [Tooltip("每波敵人數；需與每一筆 waves 四種敵人數量總和一致")]
    public int enemiesPerWave=10;
 
    // 可調數值【兩波出生的時間間隔（秒），不等待上一波清完】搜尋：waveInterval
    [Tooltip("兩波出生的時間間隔（秒），不等待上一波清完")]
    public float waveInterval=15;

    // 可調數值【取得第二顆星的通關時間上限（秒）】搜尋：starTime
    [Tooltip("取得第二顆星的通關時間上限（秒）")]
    public float starTime=180;
 
    // 可調數值【第三顆星容許的有效受擊次數上限】搜尋：starHits
    [Tooltip("第三顆星容許的有效受擊次數上限")]
    public int starHits=3;

    
    // 可調數值【地圖 X／Z 半尺寸（公尺），80／80 即完整 160 × 160】搜尋：arenaHalfSize
    [Tooltip("地圖 X／Z 半尺寸（公尺），80／80 即完整 160 × 160")]
    public Vector2 arenaHalfSize=new Vector2(80,80);

    [Header("精力／掉落／復活")]
    
    // 可調數值【每秒精力回復量】搜尋：spRegen
    [Tooltip("每秒精力回復量")]
    public float spRegen=10;

    // 可調數值【消耗精力後暫停自然回復的時間（秒）】搜尋：spRegenDelay
    [Tooltip("消耗精力後暫停自然回復的時間（秒）")]
    public float spRegenDelay=.75f;

    // 可調數值【普通敵人掉落回血物機率；0.05 代表 5%】搜尋：hpPickupChance
    [Tooltip("普通敵人掉落回血物機率；0.05 代表 5%")]
    public float hpPickupChance=.05f;

    // 可調數值【普通敵人掉落回精物機率】搜尋：spPickupChance
    [Tooltip("普通敵人掉落回精物機率")]
    public float spPickupChance=.04f;

    
    // 可調數值【回血物回復最大生命比例】搜尋：hpRestore
    [Tooltip("回血物回復最大生命比例")]
    public float hpRestore=.15f;

    // 可調數值【回精物回復最大精力比例】搜尋：spRestore
    [Tooltip("回精物回復最大精力比例")]
    public float spRestore=.2f;

    // 可調數值【一般掉落物開始吸附的距離（公尺）】搜尋：pickupRadius
    [Tooltip("一般掉落物開始吸附的距離（公尺）")]
    public float pickupRadius=2;

    // 可調數值【大型強化石與勾玉開始吸附距離（公尺）】搜尋：valuablePickupRadius
    [Tooltip("大型強化石與勾玉開始吸附距離（公尺）")]
    public float valuablePickupRadius=3;

    // 可調數值【復活後額外無敵時間（秒）】搜尋：reviveInvulnerability
    [Tooltip("復活後額外無敵時間（秒）")]
    public float reviveInvulnerability=2;

    
    // 可調數值【普通敵人四種強化石掉落機率，依序 S／M／L／XL】搜尋：stoneChances
    [Tooltip("普通敵人四種強化石掉落機率，依序 S／M／L／XL")]
    public float[] stoneChances={.04f,.015f,.005f,.0015f};

    // 可調數值【Boss 四種強化石抽選權重，依序 S／M／L／XL；總和應為 1】搜尋：bossStoneChances
    [Tooltip("Boss 四種強化石抽選權重，依序 S／M／L／XL；總和應為 1")]
    public float[] bossStoneChances={.45f,.3f,.2f,.05f};

    
    // 可調數值【普通敵人掉落勾玉機率】搜尋：jadeChance
    [Tooltip("普通敵人掉落勾玉機率")]
    public float jadeChance=.009f;

    // 可調數值【Boss 掉落勾玉機率】搜尋：bossJadeChance
    [Tooltip("Boss 掉落勾玉機率")]
    public float bossJadeChance=.1f;

    [Header("養成／經濟／等級曲線")]
    
    // 可調數值【玩家最高等級，曲線與裝備階級需一起檢查】搜尋：maxLevel
    [Tooltip("玩家最高等級，曲線與裝備階級需一起檢查")]
    public int maxLevel=50;

    // 可調數值【永久能力強化等級上限，需匹配 upgradeCosts 長度】搜尋：maxUpgrade
    [Tooltip("永久能力強化等級上限，需匹配 upgradeCosts 長度")]
    public int maxUpgrade=20;

    // 可調數值【武器強化等級上限；UI 顯示上限也需同步】搜尋：maxEnhance
    [Tooltip("武器強化等級上限；UI 顯示上限也需同步")]
    public int maxEnhance=10;

    // 可調數值【每把武器最多投入強化石次數；UI 按鈕上限也需同步】搜尋：enhanceAttempts
    [Tooltip("每把武器最多投入強化石次數；UI 按鈕上限也需同步")]
    public int enhanceAttempts=5;

    
    // 可調數值【強化石能量翻倍機率】搜尋：enhanceCrit
    [Tooltip("強化石能量翻倍機率")]
    public float enhanceCrit=.2f;

    // 可調數值【每級武器強化增加的傷害比例】搜尋：enhanceBonus
    [Tooltip("每級武器強化增加的傷害比例")]
    public float enhanceBonus=.03f;

    // 可調數值【每級永久攻擊提升比例】搜尋：permanentAttack
    [Tooltip("每級永久攻擊提升比例")]
    public float permanentAttack=.02f;

    // 可調數值【每級永久防禦的傷害減免比例】搜尋：permanentDefense
    [Tooltip("每級永久防禦的傷害減免比例")]
    public float permanentDefense=.01f;

    
    // 可調數值【強化石最小能量，依序 S／M／L／XL】搜尋：stoneMin
    [Tooltip("強化石最小能量，依序 S／M／L／XL")]
    public int[] stoneMin={25,40,55,70};

    // 可調數值【強化石最大能量（包含此值），依序 S／M／L／XL】搜尋：stoneMax
    [Tooltip("強化石最大能量（包含此值），依序 S／M／L／XL")]
    public int[] stoneMax={40,55,70,85};

    
    // 可調數值【永久能力每一級購買金幣成本，索引 0 為升第一級】搜尋：upgradeCosts
    [Tooltip("永久能力每一級購買金幣成本，索引 0 為升第一級")]
    public int[] upgradeCosts={200,250,300,350,400,450,550,650,750,900,1050,1250,1450,1700,2050,2400,2850,3350,3950,4650};

    
    // 可調數值【技能由 Lv1 升至 Lv2、3、4、5 的金幣成本】搜尋：skillUpgradeCosts
    [Tooltip("技能由 Lv1 升至 Lv2、3、4、5 的金幣成本")]
    public int[] skillUpgradeCosts={600,1200,2400,4800};

    // 可調數值【裝備階級及敵人曲線的等級節點，與各六段數值陣列配對】搜尋：tierLevels
    [Tooltip("裝備階級及敵人曲線的等級節點，與各六段數值陣列配對")]
    public int[] tierLevels={1,10,20,30,40,50};

    // 可調數值【各階金色裝備的勾玉售價】搜尋：shopPrices
    [Tooltip("各階金色裝備的勾玉售價")]
    public int[] shopPrices={30,50,75,105,140,180};

    
    // 可調數值【各等級階段的基礎長劍最低傷害】搜尋：swordMin
    [Tooltip("各等級階段的基礎長劍最低傷害")]
    public int[] swordMin={9,13,19,27,38,52};

    // 可調數值【各等級階段的基礎長劍最高傷害】搜尋：swordMax
    [Tooltip("各等級階段的基礎長劍最高傷害")]
    public int[] swordMax={11,16,23,33,46,64};

    // 可調數值【各等級階段的套服基礎防禦】搜尋：suitDEF
    [Tooltip("各等級階段的套服基礎防禦")]
    public int[] suitDEF={2,4,7,11,16,22};

    
    // 可調數值【白／綠／藍／紫／金裝備的品質倍率】搜尋：rarityMultipliers
    [Tooltip("白／綠／藍／紫／金裝備的品質倍率")]
    public float[] rarityMultipliers={1,1.1f,1.25f,1.45f,1.7f};

    
    // 可調數值【一般衛兵在 tierLevels 六個等級節點的生命值】搜尋：normalHP
    [Tooltip("一般衛兵在 tierLevels 六個等級節點的生命值")]
    public float[] normalHP={20,35,60,95,145,210};

    // 可調數值【刺客在六個等級節點的生命值】搜尋：assassinHP
    [Tooltip("刺客在六個等級節點的生命值")]
    public float[] assassinHP={15,26,45,70,105,150};

    // 可調數值【弩手在六個等級節點的生命值】搜尋：archerHP
    [Tooltip("弩手在六個等級節點的生命值")]
    public float[] archerHP={18,30,52,82,125,180};

    // 可調數值【重型敵人在六個等級節點的生命值】搜尋：heavyHP
    [Tooltip("重型敵人在六個等級節點的生命值")]
    public float[] heavyHP={45,80,135,210,320,460};

    // 可調數值【Boss 在六個等級節點的生命值】搜尋：bossHP
    [Tooltip("Boss 在六個等級節點的生命值")]
    public float[] bossHP={250,500,900,1500,2300,3400};

    
    // 可調數值【一般衛兵在六個等級節點的傷害】搜尋：normalDamage
    [Tooltip("一般衛兵在六個等級節點的傷害")]
    public float[] normalDamage={8,14,22,32,45,60};

    // 可調數值【刺客在六個等級節點的傷害】搜尋：assassinDamage
    [Tooltip("刺客在六個等級節點的傷害")]
    public float[] assassinDamage={6,11,17,25,35,47};

    // 可調數值【弩手在六個等級節點的傷害】搜尋：archerDamage
    [Tooltip("弩手在六個等級節點的傷害")]
    public float[] archerDamage={7,12,19,28,40,53};

    // 可調數值【重型敵人在六個等級節點的傷害】搜尋：heavyDamage
    [Tooltip("重型敵人在六個等級節點的傷害")]
    public float[] heavyDamage={15,25,40,58,82,110};

    // 可調數值【Boss 在六個等級節點的傷害】搜尋：bossDamage
    [Tooltip("Boss 在六個等級節點的傷害")]
    public float[] bossDamage={20,35,55,80,110,150};

    
    // 可調數值【敵人速度、射程、前搖、後搖、冷卻、擊退承受倍率；依 EnemyKind 排序】搜尋：enemies
    [Tooltip("敵人速度、射程、前搖、後搖、冷卻、擊退承受倍率；依 EnemyKind 排序")]
    public EnemySpec[] enemies;
 
    // 可調數值【七種技能的解鎖、售價、傷害倍率、範圍、冷卻與持續時間】搜尋：skills
    [Tooltip("七種技能的解鎖、售價、傷害倍率、範圍、冷卻與持續時間")]
    public SkillSpec[] skills;
 
    // 可調數值【關卡名稱、建議等級、Boss 標記與劇情】搜尋：stages
    [Tooltip("關卡名稱、建議等級、Boss 標記與劇情")]
    public StageSpec[] stages;

    
    // 可調數值【第二階段極限模式預留參數，目前未啟用玩法】搜尋：endless
    [Tooltip("第二階段極限模式預留參數，目前未啟用玩法")]
    public EndlessConfig endless=new EndlessConfig();

    
    // 可調數值【每波一般／刺客／弩手／重型數量；需與 enemiesPerWave 和 waveCount 配合】搜尋：waves
    [Tooltip("每波一般／刺客／弩手／重型數量；需與 enemiesPerWave 和 waveCount 配合")]
    public WaveSpec[] waves={new WaveSpec(10,0,0,0),new WaveSpec(10,0,0,0),new WaveSpec(8,2,0,0),new WaveSpec(7,3,0,0),new WaveSpec(7,0,3,0),new WaveSpec(6,2,2,0),new WaveSpec(5,2,2,1),new WaveSpec(4,2,3,1),new WaveSpec(3,3,2,2),new WaveSpec(3,2,3,2)};


    // 可調數值【新資產初始化】已有 GameBalance.asset 請直接改 Inspector 中的 enemies／skills／stages；此方法不會自動覆蓋舊資產。
    // EnemySpec 參數：種類、名稱、舊版生命／傷害、速度(m/s)、射程(m)、前搖(s)、後搖(s)、冷卻(s)、擊退倍率。
    // 實際生命／傷害採用 normalHP 等等級曲線，EnemySpec.hp／damage 僅保留初始描述。
    // SkillSpec 參數：名稱、說明、解鎖等級、金幣成本、傷害倍率、範圍(m)、角度(度)、冷卻(s)、持續(s)。
    public void PopulateDefaults() {
        enemies=new[] {
            new EnemySpec(EnemyKind.Soldier,"失落衛兵",20,8,2.2f,1.15f,.45f,.45f,1.15f,1),
            new EnemySpec(EnemyKind.Assassin,"暮影刺客",15,6,3.2f,1,.28f,.3f,.85f,1.2f),
            new EnemySpec(EnemyKind.Archer,"灰燼弩手",18,7,1.8f,6,.55f,.4f,1.5f,1),
            new EnemySpec(EnemyKind.Heavy,"黑鐵守衛",45,15,1.4f,1.5f,.85f,.7f,1.8f,.3f),
            new EnemySpec(EnemyKind.Boss,"巨斧亡將",250,20,1.65f,3,.8f,.65f,1.6f,0)};
        skills=new[] {
            new SkillSpec("旋風斬","旋身斬擊周圍敵人，造成擊退。",1,0,1.6f,3,360,8),
            new SkillSpec("劍氣斬","向前釋放穿透劍氣，貫穿遠方敵人。",3,500,1.8f,7,45,7),
            new SkillSpec("戰意","8 秒內攻擊 +20%，攻速 +15%。",6,1200,1,0,0,20,8),
            new SkillSpec("震地斬","向前劈出半圓重擊，強力擊退敵人。",9,2200,2.4f,3.5f,180,12),
            new SkillSpec("血刃","6 秒內吸取造成傷害的 5%，每擊最多恢復 5% HP。",12,3500,1,0,0,25,6),
            new SkillSpec("亂舞","2 秒內連續 5 次斬擊，施放時霸體。",15,5200,.7f,2.8f,180,18,2),
            new SkillSpec("無雙斬","以霸體釋放全周大範圍斬擊。",18,7500,4.5f,4.5f,360,30)};
        string[] names={"灰燼之門","斷橋餘燼","亡將的誓言","無聲修道院","暮色長廊","黑鐵王座"};
        string[] stories={"城門已經熄燈。風裡仍有鐵與灰燼的味道。\n有人還在等你回去。握緊劍，穿過這片廢墟。",
            "橋下沒有流水，只有舊日的回聲。\n他們從兩側逼近。不要停下腳步。",
            "亡將守著最後一道門，忘了自己曾經守護誰。\n看清他的起手。等斧刃落下，再出劍。"};
        // 可調數值【關卡生成】30 關；每 6 關一章；每 3 關 Boss；等級從 1 每關 +2，上限 50。
        stages=new StageSpec[30];
        for(int i=0;i<stages.Length;i++) stages[i]=new StageSpec {title=names[i%6],subtitle=$"第 {i/6+1} 章 · {(i%3==2?"首領戰":"殲滅戰")}",level=Mathf.Min(50,1+i*2),boss=i%3==2,story=stories[i%3]};
    }
    public EnemySpec Enemy(EnemyKind k)=>enemies[(int)k];
    public float Anchor(float[] values,int level) {
        for(int i=1;i<tierLevels.Length;i++) if(level<=tierLevels[i]) return Mathf.Lerp(values[i-1],values[i],Mathf.InverseLerp(tierLevels[i-1],tierLevels[i],level));
        return values[values.Length-1];
    }
    public float EnemyHP(EnemyKind k,int lv)=>Anchor(k==EnemyKind.Boss?bossHP:k==EnemyKind.Heavy?heavyHP:k==EnemyKind.Archer?archerHP:k==EnemyKind.Assassin?assassinHP:normalHP,lv);
    public float EnemyDamage(EnemyKind k,int lv)=>Anchor(k==EnemyKind.Boss?bossDamage:k==EnemyKind.Heavy?heavyDamage:k==EnemyKind.Archer?archerDamage:k==EnemyKind.Assassin?assassinDamage:normalDamage,lv);
    // 可調數值【升級經驗】Lv1 需要 100，每級增加 40；通關經驗基數 60 + 等級×20，Boss ×1.25。
    // 可調數值【通關金幣】基數 220 + 等級×20，Boss ×1.4；Round10 四捨五入至十位。
    public int RequiredExp(int lv)=>100+(lv-1)*40;
    public int StageExp(int lv,bool boss)=>Round10((60+lv*20)*(boss?1.25f:1));
    public int StageGold(int lv,bool boss)=>Round10((220+lv*20)*(boss?1.4f:1));
    public static int Round10(float value)=>(int)(Math.Floor(value/10d+.5d)*10);
    public int Tier(int lv) {int t=0; for(int i=0;i<tierLevels.Length;i++) if(lv>=tierLevels[i])t=i;return t;}
}
}
