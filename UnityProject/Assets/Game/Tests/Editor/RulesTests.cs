using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace BladeSurvivor.Tests {
public class RulesTests {
    GameBalance b;string path;
    [SetUp] public void Setup(){b=ScriptableObject.CreateInstance<GameBalance>();b.PopulateDefaults();path=Path.Combine(Path.GetTempPath(),"blade-test-"+Guid.NewGuid()+".json");}
    [TearDown] public void Cleanup(){UnityEngine.Object.DestroyImmediate(b);foreach(string suffix in new[]{"",".tmp",".bak"})if(File.Exists(path+suffix))File.Delete(path+suffix);}
    [Test] public void AttackIncludesAllModifiersAndRoundsOnce(){Assert.AreEqual(49,ProgressionRules.Damage(10,10,20,1.6f,true,true,b));}
    [Test] public void DamageCannotFallBelowOne(){Assert.AreEqual(1,ProgressionRules.Incoming(.2f,10000,20,b));}
    [Test] public void ArcHitsMultiplePointsButRejectsBehindAndOutside(){Assert.IsTrue(HitDetector.InArc(Vector3.zero,Vector3.forward,new Vector3(1,0,1),.2f,2,120));Assert.IsTrue(HitDetector.InArc(Vector3.zero,Vector3.forward,new Vector3(-1,0,1),.2f,2,120));Assert.IsFalse(HitDetector.InArc(Vector3.zero,Vector3.forward,Vector3.back,.2f,2,120));Assert.IsFalse(HitDetector.InArc(Vector3.zero,Vector3.forward,new Vector3(0,0,4),.2f,2,120));Assert.IsTrue(HitDetector.InArc(Vector3.zero,Vector3.forward,new Vector3(0,0,2.6f),.7f,2,120));}
    [TestCase(180,3,3)][TestCase(180,4,2)][TestCase(180.1f,0,1)] public void StarBoundaries(float seconds,int hits,int expected){Assert.AreEqual(expected,ProgressionRules.Stars(seconds,hits,b));}
    [Test] public void EnhancementCarriesOverflowAcrossLevels(){var e=new EquipmentItem();ProgressionRules.ApplyEnergy(e,85,b);Assert.AreEqual(3,e.enhance);Assert.AreEqual(25,e.energy);Assert.AreEqual(1,e.attempts);}
    [Test] public void SixthEnhancementCannotConsumeInventory(){var p=new ProgressionService(b,path);p.Save.stones[0]=10;for(int i=0;i<5;i++)Assert.IsTrue(p.Enhance(p.Weapon,0,out _,out _));Assert.IsFalse(p.Enhance(p.Weapon,0,out _,out _));Assert.AreEqual(5,p.Save.stones[0]);}
    [Test] public void PendingRewardsOnlyCommitOnceAndPersist(){var p=new ProgressionService(b,path);var pending=new BattleRewards{jade=2};pending.stones[3]=1;Assert.AreEqual(0,p.Save.jade);p.Commit("battle-1",0,3,pending,out _,out _);int gold=p.Save.gold;Assert.AreEqual(2,p.Save.jade);Assert.AreEqual(1,p.Save.stones[3]);Assert.IsNull(p.Commit("battle-1",0,3,pending,out _,out _));Assert.AreEqual(gold,p.Save.gold);var loaded=new ProgressionService(b,path);Assert.AreEqual(2,loaded.Save.jade);Assert.AreEqual(3,loaded.Save.stars[0]);}
    [Test] public void FailedBattleDiscardsPendingAndCannotSpendPendingJade(){var p=new ProgressionService(b,path);var pending=new BattleRewards{jade=5};pending.stones[0]=8;Assert.IsFalse(p.SpendJade());pending.Clear();Assert.AreEqual(0,pending.jade);Assert.AreEqual(0,p.Save.jade);}
    [Test] public void SkillSlotAssignmentRemovesDuplicates(){var p=new ProgressionService(b,path);p.Save.level=15;Assert.IsTrue(p.AssignSkill(0,3));Assert.AreEqual(-1,p.Save.equippedSkills[0]);Assert.AreEqual(0,p.Save.equippedSkills[3]);}
    [Test] public void LevelAnchorsAndEconomyAreConsistent(){Assert.AreEqual(35,b.EnemyHP(EnemyKind.Soldier,10));Assert.AreEqual(47.5f,b.EnemyHP(EnemyKind.Soldier,15));Assert.AreEqual(330,b.StageExp(10,true));Assert.AreEqual(550,10+20+30+40+50+60+70+80+90+100);int total=0;for(int i=1;i<50;i++)total+=b.RequiredExp(i);Assert.AreEqual(51940,total);}
    [Test] public void GoldEquipmentNeverComesFromClear(){var p=new ProgressionService(b,path);for(int i=0;i<50;i++){var reward=p.Commit("clear-"+i,0,3,new BattleRewards(),out _,out _);Assert.Less((int)reward.rarity,(int)Rarity.Legendary);}}
}
}
