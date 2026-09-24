using UnityEngine;

namespace BladeSurvivor {
public enum EnemyState { Chase, Windup, Recovery, Cooldown, Stagger, Charge, KnockedDown, Dead }
public class EnemyActor:MonoBehaviour {
    public EnemyKind Kind;public EnemyState State;public float HP,MaxHP,Radius,Damage;public bool Alive=>State!=EnemyState.Dead&&gameObject.activeSelf;
    // 可調數值【敌人防重疊身體半徑】Boss 1.05m、重型 .65m、其餘 .46m；不是武器命中半徑。
    public float BodyRadius=>Kind==EnemyKind.Boss?1.05f:Kind==EnemyKind.Heavy?.65f:.46f;
    BattleController battle;EnemySpec spec;ActorRig rig;float timer,deathTime,separationTimer;Vector3 lockedDirection,separation,knockVelocity;int pattern;bool chargeHit;
    // 可調數值【敵人命中半徑】Boss .85m、重型 .5m、其餘 .32m；生命傷害讀 GameBalance 等級曲線。
    public void Initialize(BattleController b,EnemyKind kind,int level,Vector3 position){battle=b;Kind=kind;spec=b.Balance.Enemy(kind);MaxHP=b.Balance.EnemyHP(kind,level);HP=MaxHP;Damage=b.Balance.EnemyDamage(kind,level);Radius=kind==EnemyKind.Boss?.85f:kind==EnemyKind.Heavy?.5f:.32f;
        transform.position=position;State=EnemyState.Chase;timer=0;deathTime=0;knockVelocity=Vector3.zero;pattern=-1;rig=GetComponentInChildren<ActorRig>();rig.Knockdown(1);gameObject.SetActive(true);}
    public void Tick(float dt){
    // 可調數值【死亡縮小】.45 秒縮小並回收；不影響掉落。
        if(State==EnemyState.Dead){deathTime+=dt;transform.localScale=Vector3.Lerp(Vector3.one,Vector3.zero,deathTime/.45f);if(deathTime>.45f)gameObject.SetActive(false);return;}
        if(State==EnemyState.KnockedDown){timer=Mathf.Max(0,timer-dt);rig.Knockdown(1-timer/battle.Balance.enemyKnockdownDuration);if(timer<=0){State=EnemyState.Chase;knockVelocity=Vector3.zero;}return;}
        var player=battle.Player;Vector3 delta=player.transform.position-transform.position;delta.y=0;float distance=delta.magnitude;
    // 可調數值【敵人擊退衰減】速度平方低於 .01 停止；衰減速率 12，配合 Receive 的速度倍率 10 決定位移。
        if(knockVelocity.sqrMagnitude>.01f){transform.position+=knockVelocity*dt;knockVelocity=Vector3.Lerp(knockVelocity,Vector3.zero,dt*12);}
        timer-=dt;bool moving=false;
        switch(State){
    // 可調數值【敵人轉向／遠程站位】轉向 350 度/秒；弩手理想距離 4.7m，小於 3.7m 後退。
            case EnemyState.Chase:
                if(delta.sqrMagnitude>.01f)transform.rotation=Quaternion.RotateTowards(transform.rotation,Quaternion.LookRotation(delta),dt*350);
                float desired=Kind==EnemyKind.Archer?4.7f:spec.range;
                if(distance<=spec.range&&timer<=0){State=EnemyState.Windup;lockedDirection=delta.normalized;
                    if(Kind==EnemyKind.Boss)pattern=(pattern+1)%3;
    // 可調數值【Boss 三招前搖】扇形 .8s、震地 1.1s、衝撞 .7s。預警半徑 3.6／4.3／6m，窄招 20°、一般 110°。
                    timer=Kind==EnemyKind.Boss?(pattern==0?.8f:pattern==1?1.1f:.7f):spec.windup;
                    battle.Effects.Arc(transform.position,lockedDirection,Kind==EnemyKind.Boss?(pattern==1?4.3f:pattern==2?6:3.6f):spec.range+.3f,
                        Kind==EnemyKind.Boss&&pattern==1?360:Kind==EnemyKind.Archer||pattern==2?20:110,new Color(1,.12f,.06f),timer,.07f);
                } else if(distance>desired||Kind==EnemyKind.Archer&&distance<3.7f){
    // 可調數值【追擊避讓】每 .14 秒更新；半徑和額外加 .08m，避讓向量權重 2；最終防重疊另在 BattleController。
                    separationTimer-=dt;if(separationTimer<=0){separationTimer=.14f;separation=Vector3.zero;foreach(var e in battle.Enemies){if(e==this||!e.Alive)continue;Vector3 diff=transform.position-e.transform.position;float sq=diff.sqrMagnitude;float min=Radius+e.Radius+.08f;if(sq>.001f&&sq<min*min)separation+=diff.normalized*(min-Mathf.Sqrt(sq));}}
                    Vector3 move=(delta.normalized*(Kind==EnemyKind.Archer&&distance<3.7f?-1:1)+separation*2).normalized;transform.position+=move*spec.speed*dt;moving=true;}
                break;
    // 可調數值【敵人招式】弩箭速度 9m/s、射程 12m、起點高度 .8m；Boss 衝撞持續 .65s。
            case EnemyState.Windup: if(timer<=0){rig.Strike();
                    if(Kind==EnemyKind.Archer)battle.Projectile(transform.position+Vector3.up*.8f,lockedDirection,9,12,Damage,false);
                    else if(Kind==EnemyKind.Boss&&pattern==2){State=EnemyState.Charge;timer=.65f;chargeHit=false;break;}
    // 可調數值【敵人近戰判定】普通射程額外 +.35m；Boss 扇形 3.6m／120°，震地 4.3m／360°、傷害 ×1.3；玩家半徑 .3m。
                    else {float range=Kind==EnemyKind.Boss?(pattern==1?4.3f:3.6f):spec.range+.35f,arc=Kind==EnemyKind.Boss&&pattern==1?360:120;
                        if(HitDetector.InArc(transform.position,lockedDirection,player.transform.position,.3f,range,arc))player.Receive(Damage*(Kind==EnemyKind.Boss&&pattern==1?1.3f:1),transform.position);
                        battle.Effects.Arc(transform.position,lockedDirection,range,arc,new Color(1,.2f,.06f),.2f,.12f);}
                    State=EnemyState.Recovery;timer=spec.recovery;}break;
    // 可調數值【Boss 衝撞】速度 9m/s、碰撞補償 .7m、傷害 ×1.2；衝撞後搖 .7s。
            case EnemyState.Charge:
                transform.position+=lockedDirection*9*dt;moving=true;
                if(!chargeHit&&Vector3.Distance(transform.position,player.transform.position)<Radius+.7f){chargeHit=true;player.Receive(Damage*1.2f,transform.position);}
                if(timer<=0){State=EnemyState.Recovery;timer=.7f;}break;
            case EnemyState.Recovery:if(timer<=0){State=EnemyState.Cooldown;timer=spec.cooldown*(Kind==EnemyKind.Boss&&HP<MaxHP*.5f?.85f:1);}break;
    // 可調數值【Boss 狂暴】HP 低於 50% 時，攻擊冷卻乘 .85（加快 15%），見上方 Recovery 分支。
            case EnemyState.Cooldown:if(timer<=0){State=EnemyState.Chase;}break;
            case EnemyState.Stagger:if(timer<=0)State=EnemyState.Chase;break;
        }
    // 可調數值【敵人地圖边緣】允許比玩家邊界多 .6m；Y 固定地面。
        Vector3 p=transform.position;p.y=0;p.x=Mathf.Clamp(p.x,-battle.Balance.arenaHalfSize.x-.6f,battle.Balance.arenaHalfSize.x+.6f);p.z=Mathf.Clamp(p.z,-battle.Balance.arenaHalfSize.y-.6f,battle.Balance.arenaHalfSize.y+.6f);transform.position=p;
        rig.Animate(moving?1:0,State==EnemyState.Windup,false,dt);
    }
    public void KnockDown(){if(!Alive)return;State=EnemyState.KnockedDown;timer=battle.Balance.enemyKnockdownDuration;knockVelocity=Vector3.zero;rig.Knockdown(0);}
    public void Receive(float damage,Vector3 source,float knockback){if(!Alive)return;HP=Mathf.Max(0,HP-damage);
        if(HP<=0){State=EnemyState.Dead;deathTime=0;battle.EnemyDied(this);return;}
        if(State==EnemyState.KnockedDown)return;
    // 可調數值【擊退起速】基礎距離 × 敵人承受倍率 ×10；普通受擊僵直 .16s，重型前搖抗僵直；倒地另讀 enemyKnockdownDuration。
        if(spec.knockback>0){Vector3 delta=transform.position-source;delta.y=0;knockVelocity=delta.normalized*knockback*spec.knockback*10;
            if(!(Kind==EnemyKind.Heavy&&State==EnemyState.Windup)){State=EnemyState.Stagger;timer=.16f;}}
    }
}
}
