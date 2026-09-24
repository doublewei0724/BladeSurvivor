using System.Collections.Generic;
using UnityEngine;

namespace BladeSurvivor {
public class PlayerFighter:MonoBehaviour {
    public BattleController Battle; public PlayerDerivedStats Stats; public float HP,SP;
    public Vector3 MoveDirection; public bool Charging,Shield;public float ChargeTime;
    public float[] SkillCooldowns=new float[7];public int DodgeCount,SwipeCount,ChargeCount,InterruptedCharges;
    public float InvulnerableUntil;public bool Alive=>HP>0;
    public bool Dashing=>dashTime>0;public float ChargeRatio=>Mathf.Clamp01(ChargeTime/Stats.chargeFull);
    public string ActionLabel=>KnockedDown?"倒地 · 起身中":Dashing?"閃避 / 突進":Charging?(ChargeTime>=Stats.chargeFull?"蓄力完成 · 放開施放":"蓄力中"):"";
    // 狀態說明：下列 timer／Remaining／Count 是運行中倒數或統計，不是初始平衡數值。戰鬥設定優先改 GameBalance。
    ActorRig rig;float attackTimer,attackHitAt,scanTimer,hitStun,spDelay,dodgeCD,swipeCD,dashTime,dashTotal,dashSpeed,dashElapsed,armorTime,buffTime,leechTime,flurryTime,flurryTick;
    bool attackPending,dodgeDash;Vector3 dashDirection;EnemyActor target;readonly HashSet<EnemyActor> swipeHits=new HashSet<EnemyActor>();
    float swipeHealed,downRemaining;Vector3 downDirection;LineRenderer recoveryRing,chargePreview;
    public int ComboStep {get;private set;}
    float leapRemaining;Vector3 leapDirection;
    public bool ComboLeaping=>leapRemaining>0;
    public bool KnockedDown=>downRemaining>0;
    public float RecoveryRadius=>B.recoveryRadius;
    GameBalance B=>Battle.Balance;ProgressionService P=>Battle.Progression;
    public void Initialize(BattleController battle){Battle=battle;Stats=P.Stats();HP=Stats.maxHP;SP=Stats.maxSP;Shield=Stats.shield;MoveDirection=Vector3.zero;rig=GetComponentInChildren<ActorRig>();
    // 可調數值【玩家腳下標記】半徑 .55m、高度 .06m、線寬 .035m、40 段；Color 為金色 RGBA。
        var marker=new GameObject("Player ground marker");marker.transform.SetParent(transform,false);var line=marker.AddComponent<LineRenderer>();line.sharedMaterial=new Material(Shader.Find("Sprites/Default"));line.useWorldSpace=false;line.loop=true;line.positionCount=40;line.widthMultiplier=.035f;line.startColor=line.endColor=new Color(.9f,.7f,.32f,.8f);
    // 可調數值【起身隔離圈外觀】48 段、線寬 .055m、透明度 .7；半徑讀 B.recoveryRadius。
        var recovery=new GameObject("Recovery exclusion ring");recovery.transform.SetParent(transform,false);recoveryRing=recovery.AddComponent<LineRenderer>();recoveryRing.sharedMaterial=line.sharedMaterial;recoveryRing.useWorldSpace=false;recoveryRing.loop=true;recoveryRing.positionCount=48;recoveryRing.widthMultiplier=.055f;recoveryRing.startColor=recoveryRing.endColor=new Color(.4f,.8f,1,.7f);recoveryRing.enabled=false;
    // 可調數值【蓄力瞄準框外觀】高度 .09m、線寬 .065m、透明度 .8；長寬讀 chargeLength／chargeWidth。
        var preview=new GameObject("Charge aim rectangle");preview.transform.SetParent(transform,false);chargePreview=preview.AddComponent<LineRenderer>();chargePreview.sharedMaterial=line.sharedMaterial;chargePreview.useWorldSpace=false;chargePreview.loop=true;chargePreview.positionCount=4;chargePreview.widthMultiplier=.065f;chargePreview.startColor=chargePreview.endColor=new Color(1,.7f,.25f,.8f);chargePreview.SetPositions(new[]{new Vector3(-B.chargeWidth/2,.09f,0),new Vector3(-B.chargeWidth/2,.09f,B.chargeLength),new Vector3(B.chargeWidth/2,.09f,B.chargeLength),new Vector3(B.chargeWidth/2,.09f,0)});chargePreview.enabled=false;
        for(int i=0;i<48;i++){float a=i*Mathf.PI*2/48;recoveryRing.SetPosition(i,new Vector3(Mathf.Sin(a)*B.recoveryRadius,.07f,Mathf.Cos(a)*B.recoveryRadius));}
        for(int i=0;i<40;i++){float a=i*Mathf.PI*2/40;line.SetPosition(i,new Vector3(Mathf.Sin(a)*.55f,.06f,Mathf.Cos(a)*.55f));}}
    public void Tick(float dt){if(!Alive)return;
        for(int i=0;i<7;i++)SkillCooldowns[i]=Mathf.Max(0,SkillCooldowns[i]-dt);
        spDelay-=dt;if(spDelay<=0)SP=Mathf.Min(Stats.maxSP,SP+B.spRegen*dt);
        dodgeCD-=dt;swipeCD-=dt;hitStun-=dt;armorTime-=dt;buffTime-=dt;leechTime-=dt;
    // 可調數值【玩家後退節奏】後退在最初 .2 秒完成；总倒地時間與後退距離讀 GameBalance。
        if(KnockedDown){float elapsed=B.knockdownDuration-downRemaining;float travel=Mathf.Max(0,Mathf.Min(dt,.2f-elapsed));if(travel>0)Move(downDirection*(B.knockdownDistance/.2f)*travel);downRemaining=Mathf.Max(0,downRemaining-dt);rig.Knockdown(1-downRemaining/B.knockdownDuration);recoveryRing.enabled=KnockedDown;rig.RecoveryFlash(KnockedDown,elapsed+dt);return;}
    // 可調數值【第四擊跳躍】高度／距離／時間讀 comboJump*；落地半徑 3m、全圓 360°、倍率 1、擊退 .7。
        if(ComboLeaping){float step=Mathf.Min(dt,leapRemaining);leapRemaining-=step;Move(leapDirection*(B.comboJumpDistance/B.comboJumpDuration)*step);rig.Leap(Mathf.Sin((1-leapRemaining/B.comboJumpDuration)*Mathf.PI)*B.comboJumpHeight);rig.RecoveryFlash(ComboLeaping,B.comboJumpDuration-leapRemaining);
            attackTimer-=dt;if(leapRemaining<=0){rig.Leap(0);Attack(1,3,360,.7f,true);Battle.Effects.Ring(transform.position,3,new Color(1,.75f,.3f),.45f);Battle.Camera.shake=.6f;ComboStep=0;}return;}
    // 可調數值【亂舞連斬】每 .4 秒一刀、角度 180°、擊退 .5；傷害範圍讀技能資料。
        if(flurryTime>0){flurryTime-=dt;flurryTick-=dt;if(flurryTick<=0){flurryTick+=.4f;Attack(B.skills[5].multiplier*SkillScale(5),B.skills[5].range,180,.5f);}}
    // 可調數值【突進命中】以 swipeWidth 的半寬加敵人半徑判定；基礎擊退 .7，存活敵人接著倒地。
        if(dashTime>0){float step=Mathf.Min(dt,dashTime);dashTime-=step;dashElapsed+=step;Move(dashDirection*dashSpeed*step);
            rig.RecoveryFlash(Dashing,dashElapsed);
            if(!dodgeDash)foreach(var enemy in Battle.Enemies)if(enemy.Alive&&!swipeHits.Contains(enemy)&&Vector3.Distance(transform.position,enemy.transform.position)<=B.swipeWidth*.5f+enemy.Radius){swipeHits.Add(enemy);swipeHealed+=ApplyLifeSteal(Hit(enemy,B.swipeMultiplier,.7f),swipeHealed);enemy.KnockDown();}
            rig.Animate(2,false,false,dt);return;}
        if(Charging)ChargeTime+=dt;
        float speed=Charging?0:attackTimer>0?B.attackMoveMultiplier:1;
        if(hitStun<=0)Move(MoveDirection*B.moveSpeed*speed*dt);
        bool moving=MoveDirection.sqrMagnitude>.01f;
        // Movement owns facing. Auto-targeting may turn the player only while stationary.
        if(moving&&!Charging)transform.forward=MoveDirection;
        scanTimer-=dt;if(scanTimer<=0){scanTimer=B.scanInterval;target=null;float closest=B.scanRange*B.scanRange;
            foreach(var enemy in Battle.Enemies){if(!enemy.Alive)continue;Vector3 offset=enemy.transform.position-transform.position;
                if(moving&&Vector3.Angle(MoveDirection,offset)>Arc*.5f)continue;
                if(offset.sqrMagnitude<closest){closest=offset.sqrMagnitude;target=enemy;}}}
    // 可調數值【站定自動轉向】每秒最多轉 650 度；移動中朝向仍由輸入決定。
        if(!moving&&target!=null&&!Charging&&hitStun<=0){Vector3 facing=target.transform.position-transform.position;facing.y=0;
            if(facing.sqrMagnitude>.01f)transform.rotation=Quaternion.RotateTowards(transform.rotation,Quaternion.LookRotation(facing),dt*650);}
        attackTimer-=dt;
        if(attackPending){attackHitAt-=dt;if(attackHitAt<=0){attackPending=false;Attack(1,Range,Arc,B.weaponKnockbacks[(int)P.Weapon.weapon]);}}
        if(!Charging&&hitStun<=0&&flurryTime<=0&&attackTimer<=0&&target!=null&&HitDetector.InArc(transform.position,transform.forward,target.transform.position,target.Radius,Range,Arc)){
            attackTimer=B.attackIntervals[(int)P.Weapon.weapon]/(buffTime>0?1+.15f*SkillScale(2):1);ComboStep=ComboStep%4+1;
            if(ComboStep==4){leapRemaining=B.comboJumpDuration;rig.RecoveryFlash(true,0);leapDirection=transform.forward;attackPending=false;}
            else {attackHitAt=B.attackWindup;attackPending=true;}}
        else if(attackTimer<=0&&!attackPending)ComboStep=0;
        rig.Animate(MoveDirection.magnitude*speed,attackPending,Charging,dt);
        if(Charging&&ChargeTime>=Stats.chargeFull&&Mathf.FloorToInt(ChargeTime*5)!=Mathf.FloorToInt((ChargeTime-dt)*5))Battle.Effects.Ring(transform.position,.9f,new Color(1,.7f,.25f),.22f);
    }
    public float Range=>B.weaponRanges[(int)P.Weapon.weapon];public float Arc=>B.weaponArcs[(int)P.Weapon.weapon];
    public void SetDirection(Vector3 direction){direction.y=0;if(direction.sqrMagnitude>.01f){MoveDirection=direction.normalized;scanTimer=0;if(!Dashing&&!KnockedDown&&!ComboLeaping&&!Charging)transform.forward=MoveDirection;}}
    void Move(Vector3 delta){var p=transform.position+delta;p.x=Mathf.Clamp(p.x,-B.arenaHalfSize.x,B.arenaHalfSize.x);p.z=Mathf.Clamp(p.z,-B.arenaHalfSize.y,B.arenaHalfSize.y);p.y=0;transform.position=p;}
    public void StartCharge(){if(!Alive||KnockedDown||Dashing||hitStun>0||flurryTime>0)return;ResetCombo();Charging=true;chargePreview.enabled=true;ChargeTime=0;attackPending=false;}
    public void CancelCharge(){Charging=false;ChargeTime=0;if(chargePreview!=null)chargePreview.enabled=false;}
    // 可調數值【蓄力瞄準死區】方向長度平方 .04，即拖曳地面距離至少 .2m 才更新方向。
    public void AimCharge(Vector3 direction){direction.y=0;if(Charging&&direction.sqrMagnitude>.04f)transform.forward=direction.normalized;}
    public void ReleaseCharge(){if(!Charging)return;Charging=false;chargePreview.enabled=false;float t=ChargeTime;ChargeTime=0;float scale=Stats.chargeFull/B.chargeFull;
        if(t<B.chargeStart*scale)return;int stage=t>=Stats.chargeFull?2:t>=B.chargeSecond*scale?1:0;
    // 可調數值【蓄力放招】抗打斷 .5 秒、普攻等待 .45 秒、鏡頭震動 .65；不是倒地的完全無敵。
        armorTime=.5f;ChargeCount++;ChargeAttack(B.chargeMultipliers[stage],B.chargeKnockbacks[stage]);attackTimer=.45f;Battle.Camera.shake=.65f;}
    public bool Dodge(Vector3 direction){if(!Alive||KnockedDown||dodgeCD>0||SP<B.dodgeSP*Stats.costMultiplier)return false;
        ResetCombo();SpendSP(B.dodgeSP);CancelCharge();attackPending=false;dashDirection=direction.sqrMagnitude>.01f?direction.normalized:transform.forward;transform.forward=dashDirection;dashTotal=B.dodgeDuration;dashTime=dashTotal;rig.RecoveryFlash(true,0);dashElapsed=0;dashSpeed=B.dodgeDistance/dashTotal;dodgeDash=true;dodgeCD=B.dodgeDuration+B.dodgeCooldown;DodgeCount++;Battle.Effects.Ring(transform.position,.7f,new Color(.4f,.75f,1));return true;}
    // 可調數值【突進時間／特效】目前 .24 秒走完 swipeDistance；抗打斷 .3 秒；視覺弧半徑 1.8、180°（不作傷害範圍）。
    public bool Swipe(Vector3 direction){if(!Alive||KnockedDown||swipeCD>0||SP<B.swipeSP*Stats.costMultiplier)return false;
        ResetCombo();SpendSP(B.swipeSP);CancelCharge();attackPending=false;dashDirection=direction.sqrMagnitude>.01f?direction.normalized:transform.forward;transform.forward=dashDirection;dashTotal=.24f;dashTime=dashTotal;rig.RecoveryFlash(true,0);dashElapsed=0;dashSpeed=B.swipeDistance/dashTotal;dodgeDash=false;swipeHits.Clear();swipeHealed=0;swipeCD=B.swipeCooldown;SwipeCount++;armorTime=.3f;Battle.Audio.Play(0);Battle.Effects.Arc(transform.position,dashDirection,1.8f,180,new Color(1,.35f,.16f));return true;}
    void ResetCombo(){if(ComboLeaping)rig.RecoveryFlash(false,0);ComboStep=0;leapRemaining=0;attackPending=false;rig.Leap(0);}
    void SpendSP(float amount){SP-=amount*Stats.costMultiplier;spDelay=B.spRegenDelay;}
    // 可調數值【技能成長】每提升一級增加 .1（10%）效果。
    float SkillScale(int id)=>1+(P.Save.skillLevels[id]-1)*.1f;
    // 可調數值【技能行為】血刃／戰意持續讀技能資料；無雙斬抗打斷 .6 秒；劍氣速度 12m/s；旋風擊退 .8，其餘範圍技能 1.8。
    public bool Skill(int slot){if(!Alive||KnockedDown||slot<0||slot>=P.Slots)return false;int id=P.Save.equippedSkills[slot];if(id<0||SkillCooldowns[id]>0)return false;
        ResetCombo();CancelCharge();attackPending=false;var skill=B.skills[id];SkillCooldowns[id]=skill.cooldown;float scale=SkillScale(id);
        if(id==2){buffTime=skill.duration;Battle.Effects.Ring(transform.position,2,new Color(1,.65f,.2f));}
        else if(id==4){leechTime=skill.duration;Battle.Effects.Ring(transform.position,2,new Color(.9f,.1f,.3f));}
        else if(id==5){flurryTime=skill.duration;flurryTick=0;armorTime=skill.duration;}
        else if(id==1){Battle.Projectile(transform.position+Vector3.up*.8f,transform.forward,12,skill.range,skill.multiplier*scale,true);rig.Strike();Battle.Audio.Play(0);}
        else {if(id==6)armorTime=.6f;Attack(skill.multiplier*scale,skill.range,skill.arc,id==0?.8f:1.8f);Battle.Camera.shake=.55f;}
        return true;
    }
    // 可調數值【普攻刀光】傷害倍率達 2 切金色粗刀光；壽命 .23 秒、粗細 .18／.1m；只影響外觀。
    public void Attack(float mult,float range,float arc,float knockback,bool knockdown=false){rig.Strike();Battle.Audio.Play(0);float healed=0;
        foreach(var enemy in Battle.Enemies)if(enemy.Alive&&HitDetector.InArc(transform.position,transform.forward,enemy.transform.position,enemy.Radius,range,arc)){healed+=Hit(enemy,mult,knockback);if(knockdown)enemy.KnockDown();}
        ApplyLifeSteal(healed,0);
        Battle.Effects.Arc(transform.position,transform.forward,range,arc,mult>=2?new Color(1,.62f,.22f):new Color(1,.28f,.2f),.23f,mult>=2?.18f:.1f);
    }
    // 可調數值【蓄力特效】前方均分 5 道半圓刀光，壽命 .3 秒、線寬 .15m；實際命中只看矩形。
    void ChargeAttack(float multiplier,float knockback){
        rig.Strike();Battle.Audio.Play(0);float healed=0;
        foreach(var enemy in Battle.Enemies)if(enemy.Alive&&HitDetector.InForwardBox(transform.position,transform.forward,enemy.transform.position,enemy.Radius,B.chargeLength,B.chargeWidth)){
            healed+=Hit(enemy,multiplier,knockback);enemy.KnockDown();}
        ApplyLifeSteal(healed,0);
        for(int i=1;i<=5;i++)Battle.Effects.Arc(transform.position+transform.forward*(i*B.chargeLength/5),transform.forward,B.chargeWidth*.5f,180,new Color(1,.7f,.25f),.3f,.15f);
    }
    // 可調數值【血刃吸血】基礎吸血率 .05 × 技能成長；每次攻擊最大回復 HP 上限的 .05。
    public float ApplyLifeSteal(float damage,float alreadyHealed){if(leechTime<=0)return 0;float heal=Mathf.Min(damage*.05f*SkillScale(4),Mathf.Max(0,Stats.maxHP*.05f-alreadyHealed));float actual=Mathf.Min(Stats.maxHP-HP,heal);HP+=actual;return actual;}
    public int Hit(EnemyActor enemy,float multiplier,float knockback){bool crit=UnityEngine.Random.value<Stats.crit;
    // 可調數值【戰意加攻】傷害乘上 1 + .2 × 技能成長；基礎暴擊率來自衍生數值。
        int damage=ProgressionRules.Damage(UnityEngine.Random.Range(Mathf.RoundToInt(Stats.minDamage),Mathf.RoundToInt(Stats.maxDamage)+1),P.Weapon.enhance,P.Save.upgrades[0],multiplier*(buffTime>0?1+.2f*SkillScale(2):1),crit,enemy.Kind==EnemyKind.Boss&&Stats.bossBonus,B);
        int actual=Mathf.Min(damage,Mathf.CeilToInt(enemy.HP));enemy.Receive(damage,transform.position,knockback);Battle.Effects.Number(enemy.transform.position,damage+(crit?"!":""),crit?new Color(1,.8f,.25f):Color.white);return actual;}
    public void Receive(float raw,Vector3? source=null){if(!Alive||KnockedDown||Dashing||ComboLeaping||Time.time<InvulnerableUntil)return;
        if(Shield){Shield=false;Battle.Effects.Number(transform.position,"護盾",new Color(.5f,.8f,1));Battle.Effects.Ring(transform.position,1.2f,Color.cyan);return;}
        int damage=ProgressionRules.Incoming(raw,Stats.defense,P.Save.upgrades[1],B);HP=Mathf.Max(0,HP-damage);Battle.Hits++;Battle.Effects.Number(transform.position,"−"+damage,new Color(1,.25f,.2f));Battle.Camera.shake=.7f;Battle.Audio.Play(1);
        if(Charging){InterruptedCharges++;CancelCharge();}if(armorTime<=0)ResetCombo();if(armorTime<=0&&!KnockedDown&&HP>0){downDirection=source.HasValue?transform.position-source.Value:-transform.forward;downDirection.y=0;downDirection=downDirection.sqrMagnitude>.001f?downDirection.normalized:-transform.forward;downRemaining=B.knockdownDuration;dashTime=0;flurryTime=0;attackPending=false;attackTimer=0;recoveryRing.enabled=true;rig.RecoveryFlash(true,0);}
        if(HP<=0){CancelCharge();Battle.PlayerDied();}
    }
    // 可調數值【復活】HP／SP 回復 50%；附近 ±6m 取 20 個候選點，選距離敵人最遠的位置；搜尋敵人範圍 100m。
    public void Revive(){ResetCombo();rig.RecoveryFlash(false,0);downRemaining=0;recoveryRing.enabled=false;rig.Knockdown(1);HP=Stats.maxHP*.5f;SP=Stats.maxSP*.5f;InvulnerableUntil=Time.time+B.reviveInvulnerability;dashTime=0;hitStun=0;attackPending=false;CancelCharge();MoveDirection=Vector3.zero;
        Vector3 safest=transform.position;float best=-1;for(int i=0;i<20;i++){Vector3 p=transform.position+new Vector3(UnityEngine.Random.Range(-6f,6f),0,UnityEngine.Random.Range(-6f,6f));p.x=Mathf.Clamp(p.x,-B.arenaHalfSize.x,B.arenaHalfSize.x);p.z=Mathf.Clamp(p.z,-B.arenaHalfSize.y,B.arenaHalfSize.y);var e=Battle.Nearest(p,100);float distance=e==null?100:Vector3.Distance(p,e.transform.position);if(distance>best){best=distance;safest=p;}}transform.position=safest;Battle.Camera.SnapToTarget();Battle.Effects.Ring(safest,2,Color.cyan);}
}
public static class HitDetector {
    public static bool InForwardBox(Vector3 origin,Vector3 forward,Vector3 point,float radius,float length,float width){
        Vector3 delta=point-origin;delta.y=0;forward.y=0;forward.Normalize();float z=Vector3.Dot(delta,forward),x=Vector3.Dot(delta,Vector3.Cross(Vector3.up,forward));
        float dx=Mathf.Max(0,Mathf.Abs(x)-width*.5f),dz=Mathf.Max(0,Mathf.Max(-z,z-length));return dx*dx+dz*dz<=radius*radius;
    }

    public static bool InArc(Vector3 origin,Vector3 forward,Vector3 point,float radius,float range,float arc){Vector3 offset=point-origin;offset.y=0;return offset.magnitude-radius<=range&&(arc>=359||offset.sqrMagnitude<.01f||Vector3.Angle(forward,offset)<=arc*.5f);}
}
}
