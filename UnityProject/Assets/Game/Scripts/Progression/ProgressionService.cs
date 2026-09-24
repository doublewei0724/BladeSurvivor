using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BladeSurvivor {
[Serializable] public class EquipmentItem {
    public string id; public EquipmentKind kind; public WeaponKind weapon; public Rarity rarity;
    public int tier, enhance, energy, attempts, traits;
    public string Name => (rarity==Rarity.Legendary?"誓約 · ":rarity==Rarity.Epic?"暮光 · ":rarity==Rarity.Rare?"淬銀 · ":rarity==Rarity.Uncommon?"精製 · ":"旅人 · ")+
        (kind==EquipmentKind.Suit?"戰甲":weapon==WeaponKind.DualBlades?"雙刃":weapon==WeaponKind.Greatsword?"重劍":"長劍");
    public bool Has(int bit)=>(traits&(1<<bit))!=0;
}
    // 存檔格式：version／level／upgrades 等為玩家進度；改初始值只影響新存檔，不會覆蓋舊存檔。
[Serializable] public class SaveData {
    public int version=1, level=1, exp, gold, jade, unlockedStage, selectedStage;
    // 可調數值【新存檔】5 種永久能力、4 種石頭、7 技能；初始第一技 Lv1、4 技能槽中只裝第一技、30 關星等；改長度需遷移存檔及 UI。
    public int[] upgrades=new int[5], stones=new int[4], skillLevels={1,0,0,0,0,0,0}, equippedSkills={0,-1,-1,-1}, stars=new int[30];
    public List<EquipmentItem> inventory=new List<EquipmentItem>();
    public string weaponId, suitId, lastCommittedBattle;
    // 可調數值【預設音量】.65（65%），已有存檔以存檔 volume 為準。
    public bool adFree, tutorialSeen; public float volume=.65f;
}
[Serializable] public class BattleRewards {
    public int[] stones=new int[4]; public int jade;
    public void Clear(){Array.Clear(stones,0,stones.Length);jade=0;}
}
public class PlayerDerivedStats {
    public float maxHP,maxSP,crit,minDamage,maxDamage,defense,costMultiplier=1,pickupMultiplier=1,chargeFull;
    public bool bossBonus,shield;
}
public static class ProgressionRules {
    // 可調數值【傷害公式】對 Boss 詞條 ×1.12；最終傷害至少 1，四捨五入一次。強化與永久攻擊增幅讀 GameBalance。
    public static int Damage(float rolled,int enhance,int attackLevel,float mult,bool crit,bool bossBonus,GameBalance b) =>
        Mathf.Max(1,Mathf.RoundToInt(rolled*(1+enhance*b.enhanceBonus)*(1+attackLevel*b.permanentAttack)*mult*(crit?b.critMultiplier:1)*(bossBonus?1.12f:1)));
    // 可調數值【防禦公式】100 / (100 + 防禦)，100 為減傷曲線係數；傷害至少 1。
    public static int Incoming(float raw,float def,int upgrade,GameBalance b)=>Mathf.Max(1,Mathf.RoundToInt(raw*100/(100+def)*(1-upgrade*b.permanentDefense)));
    public static int Stars(float time,int hits,GameBalance b)=>time>b.starTime?1:hits>b.starHits?2:3;
    public static void ApplyEnergy(EquipmentItem item,int amount,GameBalance b){
        item.energy+=amount;item.attempts++;
    // 可調數值【強化能量門檻】下一級門檻 = 等級×10，例如 +1 要 10、+2 要 20；保留溢出能量。
        while(item.enhance<b.maxEnhance && item.energy>=(item.enhance+1)*10){item.energy-=(item.enhance+1)*10;item.enhance++;}
        if(item.enhance==b.maxEnhance)item.energy=0;
    }
}
public sealed class ProgressionService {
    public SaveData Save {get;private set;}
    public readonly GameBalance Balance;
    public string SaveError {get;private set;}
    readonly string file;
    public ProgressionService(GameBalance b,string path=null){Balance=b;file=path??Path.Combine(Application.persistentDataPath,"blade-survivor-v1.json");Load();}
    void Load(){
        try {if(File.Exists(file))Save=JsonUtility.FromJson<SaveData>(File.ReadAllText(file));}
        catch(Exception e){Debug.LogWarning("Save recovery: "+e.Message);try{if(File.Exists(file+".bak"))Save=JsonUtility.FromJson<SaveData>(File.ReadAllText(file+".bak"));}catch(Exception){}}
        if(Save==null)Save=new SaveData();
        if(Save.inventory==null)Save.inventory=new List<EquipmentItem>();
        Save.level=Mathf.Clamp(Save.level,1,Balance.maxLevel);
        if(Save.inventory.Count==0){var sword=CreateEquipment(EquipmentKind.Weapon,0,Rarity.Common,WeaponKind.Sword);Save.inventory.Add(sword);Save.weaponId=sword.id;}
        if(Weapon==null)Save.weaponId=Save.inventory.Find(x=>x.kind==EquipmentKind.Weapon)?.id;
    }
    public void Persist(){
        try {Directory.CreateDirectory(Path.GetDirectoryName(file));File.WriteAllText(file+".tmp",JsonUtility.ToJson(Save,true));
            if(File.Exists(file)){
#if UNITY_WEBGL && !UNITY_EDITOR
                // Browser virtual filesystems don't reliably support atomic File.Replace.
                File.Copy(file,file+".bak",true);File.Delete(file);File.Move(file+".tmp",file);
#else
                File.Replace(file+".tmp",file,file+".bak");
#endif
            }else File.Move(file+".tmp",file);SaveError=null;}
        catch(Exception e){SaveError="存檔失敗："+e.Message;Debug.LogError(SaveError);}
    }
    public EquipmentItem Weapon=>Save.inventory.Find(x=>x.id==Save.weaponId);
    public EquipmentItem Suit=>Save.inventory.Find(x=>x.id==Save.suitId);
    // 可調數值【永久能力／裝備詞條】每級生命 +20、精力 +5、暴擊 +.005；武器詞條暴擊 +.05、滿蓄力 - .5s；套服回血 ×1.25、精力消耗 ×.8。
    public PlayerDerivedStats Stats(){
        var s=new PlayerDerivedStats {maxHP=Balance.baseHP+Save.upgrades[2]*20,maxSP=Balance.baseSP+Save.upgrades[3]*5,
            crit=Balance.critChance+Save.upgrades[4]*.005f,chargeFull=Balance.chargeFull};
        var w=Weapon;
        if(w!=null){float rarity=Balance.rarityMultipliers[(int)w.rarity],type=Balance.weaponMultipliers[(int)w.weapon];
            s.minDamage=Mathf.RoundToInt(Balance.swordMin[w.tier]*rarity)*type;s.maxDamage=Mathf.RoundToInt(Balance.swordMax[w.tier]*rarity)*type;
            if(w.Has(0))s.crit+=.05f;s.bossBonus=w.Has(1);if(w.Has(2))s.chargeFull-=.5f;}
        var a=Suit;if(a!=null){s.defense=Mathf.RoundToInt(Balance.suitDEF[a.tier]*Balance.rarityMultipliers[(int)a.rarity]);
            if(a.Has(0))s.pickupMultiplier=1.25f;if(a.Has(1))s.costMultiplier=.8f;s.shield=a.rarity==Rarity.Legendary;}
        return s;
    }
    public EquipmentItem CreateEquipment(EquipmentKind kind,int tier,Rarity rarity,WeaponKind type=WeaponKind.Sword){
        var e=new EquipmentItem{id=Guid.NewGuid().ToString("N"),kind=kind,tier=tier,rarity=rarity,weapon=type};
    // 可調數值【詞條數】金 3／紫 2／藍 1／其他 0；武器最多 3 種、套服最多 2 種。
        int count=rarity==Rarity.Legendary?3:rarity==Rarity.Epic?2:rarity==Rarity.Rare?1:0,max=kind==EquipmentKind.Weapon?3:2;
        count=Mathf.Min(count,max);while(count>0){int bit=1<<UnityEngine.Random.Range(0,max);if((e.traits&bit)==0){e.traits|=bit;count--;}}
        return e;
    }
    public bool Equip(EquipmentItem item){if(item==null||Save.level<Balance.tierLevels[item.tier])return false;
        if(item.kind==EquipmentKind.Weapon)Save.weaponId=item.id;else Save.suitId=item.id;Persist();return true;}
    public bool Upgrade(int index){if(index<0||index>=5||Save.upgrades[index]>=Balance.maxUpgrade)return false;
        int price=Balance.upgradeCosts[Save.upgrades[index]];if(Save.gold<price)return false;Save.gold-=price;Save.upgrades[index]++;Persist();return true;}
    public bool Enhance(EquipmentItem item,int stone,out int energy,out bool critical){energy=0;critical=false;
        if(item==null||item.kind!=EquipmentKind.Weapon||stone<0||stone>3||Save.stones[stone]<=0||item.attempts>=Balance.enhanceAttempts||item.enhance>=Balance.maxEnhance)return false;
        Save.stones[stone]--;energy=UnityEngine.Random.Range(Balance.stoneMin[stone],Balance.stoneMax[stone]+1);critical=UnityEngine.Random.value<Balance.enhanceCrit;
    // 可調數值【強化暴擊】能量 ×2；發生機率讀 enhanceCrit。
        if(critical)energy*=2;ProgressionRules.ApplyEnergy(item,energy,Balance);Persist();return true;
    }
    public bool BuySkill(int id){var skill=Balance.skills[id];if(Save.skillLevels[id]>0||Save.level<skill.unlock||Save.gold<skill.cost)return false;
        Save.gold-=skill.cost;Save.skillLevels[id]=1;for(int i=0;i<Slots;i++)if(Save.equippedSkills[i]<0){Save.equippedSkills[i]=id;break;}Persist();return true;}
    // 可調數值【技能等級上限】目前 5 級，對應 skillUpgradeCosts 四筆升級費用。
    public bool UpgradeSkill(int id){int level=Save.skillLevels[id];if(level<1||level>=5||Save.gold<Balance.skillUpgradeCosts[level-1])return false;
        Save.gold-=Balance.skillUpgradeCosts[level-1];Save.skillLevels[id]++;Persist();return true;}
    // 可調數值【技能槽解鎖】Lv1／5／10／15 對應 1／2／3／4 格；同步 UI 鎖定文字。
    public int Slots=>Save.level>=15?4:Save.level>=10?3:Save.level>=5?2:1;
    public bool AssignSkill(int skill,int slot){if(slot>=Slots||Save.skillLevels[skill]==0)return false;
        for(int i=0;i<4;i++)if(Save.equippedSkills[i]==skill)Save.equippedSkills[i]=-1;Save.equippedSkills[slot]=skill;Persist();return true;}
    public bool BuyEquipment(EquipmentKind kind,int tier,WeaponKind weapon=WeaponKind.Sword){if(Save.level<Balance.tierLevels[tier]||Save.jade<Balance.shopPrices[tier])return false;
        Save.jade-=Balance.shopPrices[tier];Save.inventory.Add(CreateEquipment(kind,tier,Rarity.Legendary,weapon));Persist();return true;}
    // 可調數值【第二次復活價格】扣 1 顆已存檔勾玉；待結算掉落不能使用。
    public bool SpendJade(){if(Save.jade<1)return false;Save.jade--;Persist();return true;}
    public EquipmentItem Commit(string battleId,int stage,int stars,BattleRewards pending,out int exp,out int gold){
        exp=gold=0;if(Save.lastCommittedBattle==battleId)return null;
        var spec=Balance.stages[stage];exp=Balance.StageExp(spec.level,spec.boss);gold=Balance.StageGold(spec.level,spec.boss);
        Save.gold+=gold;Save.exp+=exp;
        while(Save.level<Balance.maxLevel&&Save.exp>=Balance.RequiredExp(Save.level)){Save.exp-=Balance.RequiredExp(Save.level);Save.level++;}
        if(Save.level==Balance.maxLevel)Save.exp=0;
        for(int i=0;i<4;i++)Save.stones[i]+=pending.stones[i];Save.jade+=pending.jade;
        Save.stars[stage]=Mathf.Max(Save.stars[stage],stars);Save.unlockedStage=Mathf.Max(Save.unlockedStage,Mathf.Min(stage+1,Balance.stages.Length-1));
        float roll=UnityEngine.Random.value;
    // 可調數值【通關品質抽選】1 星白75%／綠25%；2 星綠75%／藍25%；3 星綠55%／藍35%／紫10%；武器55%／套服45%。
        Rarity rarity=stars==1?(roll<.75f?Rarity.Common:Rarity.Uncommon):stars==2?(roll<.75f?Rarity.Uncommon:Rarity.Rare):(roll<.55f?Rarity.Uncommon:roll<.9f?Rarity.Rare:Rarity.Epic);
        var item=CreateEquipment(UnityEngine.Random.value<.55f?EquipmentKind.Weapon:EquipmentKind.Suit,Balance.Tier(Save.level),rarity);
        Save.inventory.Add(item);Save.lastCommittedBattle=battleId;Persist();return item;
    }
}
}
