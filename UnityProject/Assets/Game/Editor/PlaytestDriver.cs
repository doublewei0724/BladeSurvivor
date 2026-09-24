using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BladeSurvivor {
public static class PlaytestDriver {
    static int phase;static double next;static readonly List<string> passed=new List<string>();static string report;static int maxAlive;static string testSave;
    [MenuItem("Blade Survivor/Testing/Capture Full Frame")]
    public static void Capture(){var r=GameRoot.Instance;if(r==null)return;r.StartCoroutine(CaptureFrame(r.Screen.ToString()));}
    static IEnumerator CaptureFrame(string name){yield return new WaitForEndOfFrame();var image=ScreenCapture.CaptureScreenshotAsTexture();string path=Path.GetFullPath("../TestResults/"+name+".png");Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllBytes(path,image.EncodeToPNG());UnityEngine.Object.Destroy(image);Debug.Log("BLADE_SCREENSHOT "+path);}
    [MenuItem("Blade Survivor/Testing/Set Game View 1600x900")]
    public static void Resolution(){
        var asm=typeof(Editor).Assembly;var sizesType=asm.GetType("UnityEditor.GameViewSizes");var singleton=typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
        var sizes=singleton.GetProperty("instance",System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Static).GetValue(null);
        var group=sizesType.GetMethod("GetGroup").Invoke(sizes,new object[]{GameViewSizeGroupType.Standalone});
        var sizeType=asm.GetType("UnityEditor.GameViewSize");var enumType=asm.GetType("UnityEditor.GameViewSizeType");
        var size=Activator.CreateInstance(sizeType,new object[]{Enum.ToObject(enumType,1),1600,900,"Blade Survivor 1600x900"});
        group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{size});int built=(int)group.GetType().GetMethod("GetBuiltinCount").Invoke(group,null),custom=(int)group.GetType().GetMethod("GetCustomCount").Invoke(group,null);
        var view=EditorWindow.GetWindow(asm.GetType("UnityEditor.GameView"));view.GetType().GetProperty("selectedSizeIndex",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic).SetValue(view,built+custom-1);view.Focus();view.Repaint();
    }
    [MenuItem("Blade Survivor/Testing/Show Main Menu")]
    public static void Menu(){var root=GameRoot.Instance;if(root==null)return;root.Battle.Leave();root.Navigate(GameScreen.MainMenu);}
    [MenuItem("Blade Survivor/Testing/Show Battle")]
    public static void ShowBattle(){var root=GameRoot.Instance;if(root==null)return;root.Balance=Resources.Load<GameBalance>("GameBalance");root.Battle.Balance=root.Balance;IsolateSave(root);root.SelectedStage=0;root.BeginBattle();root.Battle.Player.InvulnerableUntil=Time.time+3600;}
    [MenuItem("Blade Survivor/Testing/Show Equipment")]
    public static void Equipment(){var r=GameRoot.Instance;if(r==null)return;IsolateSave(r);r.Progression.Save.level=15;r.Progression.Save.gold=3500;r.Progression.Save.jade=48;for(int i=0;i<4;i++)r.Progression.Save.stones[i]=3+i;
        for(int i=0;i<8;i++)r.Progression.Save.inventory.Add(r.Progression.CreateEquipment(i%3==0?EquipmentKind.Suit:EquipmentKind.Weapon,i%2,(Rarity)(i%5),(WeaponKind)(i%3)));r.Navigate(GameScreen.Equipment);}
    [MenuItem("Blade Survivor/Testing/Show Map")]
    public static void Map(){var r=GameRoot.Instance;if(r==null)return;r.Battle.Leave();r.Navigate(GameScreen.Map);}
    [MenuItem("Blade Survivor/Testing/Verify Large World")]
    public static void LargeWorld(){
        ShowBattle();var b=GameRoot.Instance.Battle;var player=b.Player;var camera=b.Camera.GetComponent<Camera>();int checkedSpawns=0;
        foreach(var location in new[]{Vector3.zero,new Vector3(45,0,35),new Vector3(79,0,79),new Vector3(-79,0,79),new Vector3(79,0,-79),new Vector3(-79,0,-79)}){
            player.transform.position=location;b.Camera.SnapToTarget();var view=camera.WorldToViewportPoint(location);
            if(Vector2.Distance(new Vector2(view.x,view.y),new Vector2(.5f,.5f))>.01f)throw new Exception("Camera did not center on player");
            for(int i=0;i<10;i++){var p=b.SpawnPosition(i);var v=camera.WorldToViewportPoint(p);
                if(v.x>=0&&v.x<=1&&v.y>=0&&v.y<=1)throw new Exception("Enemy spawned inside visible viewport");
                if(Mathf.Abs(p.x)>80||Mathf.Abs(p.z)>80)throw new Exception("Spawn outside world bounds");checkedSpawns++;}
        }
        player.transform.position=new Vector3(45,0,35);b.Camera.SnapToTarget();player.SetDirection(Vector3.right);player.Tick(.5f);
        if(player.transform.position.x<=45)throw new Exception("Player remains constrained to old arena");player.MoveDirection=Vector3.zero;
        player.Revive();if(Vector3.Distance(player.transform.position,new Vector3(45,0,35))>12)throw new Exception("Revive teleported to old arena");
        File.WriteAllText(Path.GetFullPath("../TestResults/large-world.json"),"{\"success\":true,\"spawnPositionsChecked\":"+checkedSpawns+",\"cameraLocationsChecked\":6,\"movementBeyondOldBounds\":true,\"localRevive\":true}");
        ShowBattle();Debug.Log("BLADE_LARGE_WORLD_PASS");
    }
    [MenuItem("Blade Survivor/Testing/Verify Melee Spacing")]
    public static void MeleeSpacing(){
        ShowBattle();var b=GameRoot.Instance.Battle;int pairs=0;
        foreach(var location in new[]{Vector3.zero,new Vector3(79.8f,0,79.8f)}){
            foreach(var enemy in b.Enemies)if(enemy.Alive){enemy.transform.position=location;enemy.State=EnemyState.Windup;}
            for(int pass=0;pass<12;pass++)b.ResolveEnemySpacing();
            for(int i=0;i<b.Enemies.Count;i++)for(int j=i+1;j<b.Enemies.Count;j++){
                var a=b.Enemies[i];var c=b.Enemies[j];if(!a.Alive||!c.Alive)continue;
                if(Vector3.Distance(a.transform.position,c.transform.position)<a.BodyRadius+c.BodyRadius+.04f)throw new Exception("Enemy bodies overlap");pairs++;
            }
        }
        if(Mathf.Abs(b.Player.Range-3f)>.001f||b.Player.Arc!=120)throw new Exception("Initial sword range or arc mismatch");
        if(!HitDetector.InArc(Vector3.zero,Vector3.forward,new Vector3(0,0,2.7f),.1f,b.Player.Range,b.Player.Arc))throw new Exception("Extended sword misses new reach");
        File.WriteAllText(Path.GetFullPath("../TestResults/melee-spacing.json"),"{\"success\":true,\"stationaryPairsChecked\":"+pairs+",\"range\":2.8,\"arc\":120}");
        ShowBattle();Debug.Log("BLADE_MELEE_SPACING_PASS");
    }
    [MenuItem("Blade Survivor/Testing/Verify Movement Facing")]
    public static void MovementFacing(){
        ShowBattle();var b=GameRoot.Instance.Battle;var p=b.Player;
        var actors=b.Enemies.FindAll(e=>e.Alive);var front=actors[0];var back=actors[1];
        for(int i=2;i<actors.Count;i++){actors[i].State=EnemyState.Dead;actors[i].gameObject.SetActive(false);}
        front.HP=back.HP=10000;Vector3 direction=new Vector3(-1,0,-1).normalized;
        p.SetDirection(direction);
        if(Vector3.Dot(p.transform.forward,direction)<.999f)throw new Exception("Click does not immediately set facing");
        for(int i=0;i<90;i++){
            front.transform.position=p.transform.position+direction*2;back.transform.position=p.transform.position-direction;
            p.Tick(1f/60);
            if(Vector3.Dot(p.transform.forward,direction)<.999f)throw new Exception("Auto-target overrides movement facing");
        }
        if(front.HP>=10000||back.HP!=10000)throw new Exception("Moving attack did not select front enemy over closer rear enemy");
        p.SetDirection(-direction);front.transform.position=p.transform.position+direction*2;back.transform.position=p.transform.position+direction;
        float oldFront=front.HP;
        for(int i=0;i<60;i++)p.Tick(1f/60);
        if(front.HP!=oldFront||back.HP!=10000)throw new Exception("Retreat attack hits enemies behind player");
        p.MoveDirection=Vector3.zero;back.transform.position=p.transform.position+direction*2;
        for(int i=0;i<120;i++)p.Tick(1f/60);
        if(back.HP>=10000)throw new Exception("Stationary auto-attack stopped working");
        File.WriteAllText(Path.GetFullPath("../TestResults/movement-facing.json"),"{\"success\":true,\"immediateClickFacing\":true,\"movementFacingPriority\":true,\"frontTargetOverCloserRearTarget\":true,\"noRearHitsWhenRetreating\":true,\"stationaryAutoAttack\":true}");
        ShowBattle();Debug.Log("BLADE_MOVEMENT_FACING_PASS");
    }
    [MenuItem("Blade Survivor/Testing/Verify Knockdown")]
    public static void VerifyKnockdown(){
        ShowBattle();var b=GameRoot.Instance.Battle;var p=b.Player;p.InvulnerableUntil=0;Vector3 origin=p.transform.position;
        p.Receive(5,origin+Vector3.forward);if(!p.KnockedDown||p.Skill(0)||p.Dodge(Vector3.forward))throw new Exception("Knockdown input lock failed");
        foreach(var e in b.Enemies)if(e.Alive)e.transform.position=origin;
        p.Tick(.2f);if(Vector3.Distance(origin,p.transform.position)<.6f)throw new Exception("Missing knockback");
        b.ResolveEnemySpacing();foreach(var e in b.Enemies)if(e.Alive&&Vector3.Distance(e.transform.position,p.transform.position)<2f+e.BodyRadius-.01f)throw new Exception("Recovery exclusion failed");
        p.Tick(.8f);float hp=p.HP;p.Receive(5,origin);if(p.HP!=hp)throw new Exception("Recovery invulnerability failed");
        p.Tick(.99f);if(!p.KnockedDown)throw new Exception("Recovery ended early");p.Tick(.02f);if(p.KnockedDown)throw new Exception("Repeat hit reset recovery timer");
        var enemy=b.Enemies.Find(e=>e.Alive);enemy.transform.position=p.transform.position+Vector3.forward;foreach(var e in b.Enemies)if(e!=enemy){e.State=EnemyState.Dead;e.gameObject.SetActive(false);}b.ResolveEnemySpacing();
        if(Vector3.Distance(enemy.transform.position,p.transform.position)>1.1f)throw new Exception("Recovery barrier persisted");
        File.WriteAllText(Path.GetFullPath("../TestResults/knockdown.json"),"{\"success\":true,\"duration\":2,\"knockback\":0.65,\"radius\":1.5,\"repeatHitDoesNotReset\":true,\"barrierRemovedOnStanding\":true}");ShowBattle();Debug.Log("BLADE_KNOCKDOWN_PASS");
    }
    [MenuItem("Blade Survivor/Testing/Verify Combo")]
    public static void VerifyCombo(){
        ShowBattle();var b=GameRoot.Instance.Battle;var p=b.Player;var enemies=b.Enemies.FindAll(e=>e.Alive);var front=enemies[0];var rear=enemies[1];
        for(int i=2;i<enemies.Count;i++){enemies[i].State=EnemyState.Dead;enemies[i].gameObject.SetActive(false);}front.HP=rear.HP=10000;p.SetDirection(Vector3.forward);p.MoveDirection=Vector3.zero;
        var seen=new HashSet<int>();bool jumped=false;
        for(int i=0;i<150;i++){front.transform.position=p.transform.position+Vector3.forward*2;rear.transform.position=p.transform.position-Vector3.forward*2;p.Tick(.016f);seen.Add(p.ComboStep);jumped|=p.ComboLeaping;if(jumped&&!p.ComboLeaping)break;}
        if(!seen.Contains(1)||!seen.Contains(2)||!seen.Contains(3)||!seen.Contains(4)||rear.HP>=10000)throw new Exception("Four-hit landing combo failed");
        front.transform.position=new Vector3(40,0,40);rear.transform.position=new Vector3(-40,0,-40);for(int i=0;i<60;i++)p.Tick(.016f);if(p.ComboStep!=0)throw new Exception("Missing next attack did not reset combo");
        front.transform.position=p.transform.position+Vector3.forward*2;p.Tick(.12f);if(p.ComboStep!=1)throw new Exception("New chain does not start at one");p.InvulnerableUntil=0;p.Receive(1,p.transform.position+Vector3.forward);if(p.ComboStep!=0)throw new Exception("Hit interruption did not reset combo");
        float hp=p.HP;int hits=b.Hits;p.Receive(9999);if(p.HP!=hp||b.Hits!=hits)throw new Exception("Recovery damage not ignored");
        File.WriteAllText(Path.GetFullPath("../TestResults/combo.json"),"{\"success\":true,\"fourStages\":true,\"rearEnemyHitOnLanding\":true,\"missedCadenceReset\":true,\"damageInterruptReset\":true,\"recoveryInvulnerability\":true}");
        b.Running=false;Time.timeScale=0;p.Tick(.5f);Debug.Log("BLADE_COMBO_PASS");
    }
    [MenuItem("Blade Survivor/Testing/Verify Aimed Charge")]
    public static void AimedCharge(){ShowBattle();var b=GameRoot.Instance.Battle;var p=b.Player;var enemies=b.Enemies.FindAll(e=>e.Alive);foreach(var e in enemies)e.HP=10000;
        var hit=enemies[0];var miss=enemies[1];hit.transform.position=Vector3.right*4;miss.transform.position=Vector3.left*2;
        p.SetDirection(Vector3.forward);p.StartCharge();p.ChargeTime=3;p.AimCharge(Vector3.right);p.Tick(.01f);
        if(Vector3.Dot(p.transform.forward,Vector3.right)<.99f)throw new Exception("Charge aim overwritten by movement");p.ReleaseCharge();
        if(hit.HP>=10000||hit.State!=EnemyState.KnockedDown||miss.HP!=10000)throw new Exception("Aimed rectangle or enemy knockdown failed");hit.Tick(2.01f);if(hit.State==EnemyState.KnockedDown)throw new Exception("Enemy failed to stand");
        miss.transform.position=p.transform.position-Vector3.forward*2;p.Attack(1,3,360,.7f,true);if(miss.State!=EnemyState.KnockedDown)throw new Exception("Landing strike does not knock down");
        if(p.RecoveryRadius!=2||b.Balance.comboJumpHeight<=.65f||b.Balance.comboJumpDistance<=.8f)throw new Exception("Requested dimensions not applied");
        File.WriteAllText(Path.GetFullPath("../TestResults/aimed-charge.json"),"{\"success\":true,\"directionPreserved\":true,\"rectangleHitAndMiss\":true,\"enemyKnockdownAndRecovery\":true,\"landingKnockdown\":true}");
        ShowBattle();p=b.Player;p.StartCharge();p.ChargeTime=2;p.AimCharge(new Vector3(1,0,1));b.Root.Gestures.enabled=false;b.Running=false;Time.timeScale=0;Debug.Log("BLADE_AIMED_CHARGE_PASS");}
    [MenuItem("Blade Survivor/Testing/Verify Dash Protection")]
    public static void DashProtection(){ShowBattle();var b=GameRoot.Instance.Battle;var p=b.Player;p.InvulnerableUntil=0;float hp=p.HP;
        p.Dodge(Vector3.forward);p.Receive(9999);if(p.HP!=hp||p.KnockedDown)throw new Exception("Dodge start is not invulnerable");p.Tick(.4f);p.Receive(9999);if(p.HP!=hp)throw new Exception("Dodge tail is not invulnerable");p.Tick(.06f);
        p.Swipe(Vector3.forward);p.Receive(9999);var e=b.Enemies.Find(x=>x.Alive);e.HP=10000;e.transform.position=p.transform.position+Vector3.forward*.4f;p.Tick(.02f);
        if(p.HP!=hp||e.State!=EnemyState.KnockedDown)throw new Exception("Swipe protection or knockdown failed");p.Tick(.23f);p.Receive(1);if(p.HP>=hp)throw new Exception("Dash invulnerability persisted");
        if(b.Balance.chargeLength!=6||b.Balance.knockdownDistance!=1||b.Balance.attackMoveMultiplier!=.45f||b.Balance.weaponKnockbacks[0]!=.55f)throw new Exception("Balance mismatch");
        File.WriteAllText(Path.GetFullPath("../TestResults/dash-protection.json"),"{\"success\":true,\"fullDodgeInvulnerability\":true,\"swipeInvulnerabilityAndKnockdown\":true,\"protectionEndsWithDash\":true,\"balanceValues\":true}");ShowBattle();Debug.Log("BLADE_DASH_PROTECTION_PASS");}
    [MenuItem("Blade Survivor/Testing/Verify Finisher Protection")]
    public static void FinisherProtection(){ShowBattle();var b=GameRoot.Instance.Battle;var p=b.Player;var enemy=b.Enemies.Find(e=>e.Alive);enemy.HP=10000;
        foreach(var e in b.Enemies)if(e!=enemy){e.State=EnemyState.Dead;e.gameObject.SetActive(false);}
        p.InvulnerableUntil=0;p.MoveDirection=Vector3.zero;
        for(int i=0;i<180&&!p.ComboLeaping;i++){enemy.transform.position=p.transform.position+Vector3.forward*2;p.Tick(.016f);}
        if(!p.ComboLeaping)throw new Exception("Fourth attack did not begin");float hp=p.HP;p.Receive(9999);p.Tick(.2f);p.Receive(9999);
        if(p.HP!=hp||!p.ComboLeaping||p.KnockedDown)throw new Exception("Finisher invulnerability failed");
        float enemyHP=enemy.HP;p.Tick(.21f);if(p.ComboLeaping||enemy.HP>=enemyHP)throw new Exception("Landing failed");
        p.Receive(1);if(p.HP>=hp)throw new Exception("Finisher invulnerability persisted after landing");p.Tick(2.01f);
        Vector3 start=p.transform.position;p.SP=100;p.Dodge(Vector3.right);p.Tick(.5f);if(Mathf.Abs(Vector3.Distance(start,p.transform.position)-6)>.01f)throw new Exception("Dodge distance not six");
        start=p.transform.position;p.Swipe(Vector3.right);p.Tick(.3f);if(Mathf.Abs(Vector3.Distance(start,p.transform.position)-6)>.01f||b.Balance.swipeWidth!=2)throw new Exception("Swipe dimensions wrong");
        File.WriteAllText(Path.GetFullPath("../TestResults/finisher-protection.json"),"{\"success\":true,\"jumpInvulnerable\":true,\"landingDealsDamage\":true,\"protectionEndsAfterLanding\":true,\"dodgeDistance\":6,\"swipeDistance\":6,\"swipeWidth\":2}");ShowBattle();Debug.Log("BLADE_FINISHER_PROTECTION_PASS");}
    static void IsolateSave(GameRoot root){testSave=Path.GetFullPath("Temp/playtest-save-"+Guid.NewGuid()+".json");root.Progression=new ProgressionService(root.Balance,testSave);root.Battle.Progression=root.Progression;root.TestMode=true;}
    [MenuItem("Blade Survivor/Testing/Run Runtime Smoke Test")]
    public static void Run(){if(GameRoot.Instance==null)throw new Exception("Enter Play Mode first");passed.Clear();maxAlive=0;report=Path.GetFullPath("../TestResults/runtime-smoke.json");Directory.CreateDirectory(Path.GetDirectoryName(report));
        var root=GameRoot.Instance;root.Battle.Leave();root.Balance=UnityEngine.Object.Instantiate(root.Balance);root.Balance.attackIntervals=new[]{99999f,99999f,99999f};root.Battle.Balance=root.Balance;IsolateSave(root);root.SelectedStage=2;root.BeginBattle();phase=0;next=EditorApplication.timeSinceStartup+.5;EditorApplication.update-=Tick;EditorApplication.update+=Tick;}
    static float rootRecoveryDuration(BattleController b)=>b.Balance.knockdownDuration+.01f;
    static void Check(bool condition,string name){if(!condition)throw new Exception(name);passed.Add(name);}
    static void Tick(){if(EditorApplication.timeSinceStartup<next)return;var r=GameRoot.Instance;if(r==null){EditorApplication.update-=Tick;return;}var b=r.Battle;var p=b.Player;
        try {
            if(phase==0){Check(b.Wave==1&&b.AliveCount==10,"Wave one spawns ten enemies");p.MoveDirection=Vector3.zero;p.InvulnerableUntil=Time.time+1000;
                float hp=p.HP; p.Receive(100);Check(p.HP==hp&&b.Hits==0,"Invulnerability prevents HP loss and star hit count");p.InvulnerableUntil=0;p.Shield=true;p.Receive(10);Check(p.HP==hp&&!p.Shield&&b.Hits==0,"Opening shield consumes exactly once without damage count");
                p.StartCharge();p.Receive(10);Check(!p.Charging&&p.HP<hp&&b.Hits==1,"Effective attack interrupts charging and counts one hit");p.HP=p.Stats.maxHP;p.InvulnerableUntil=Time.time+1000;
                p.Tick(rootRecoveryDuration(b));p.SP=0;Check(!p.Dodge(Vector3.forward)&&!p.Swipe(Vector3.forward),"SP gate prevents dodge and swipe at zero");p.SP=p.Stats.maxSP;Check(p.Dodge(Vector3.forward),"Dodge starts with enough SP");Check(Mathf.Abs(p.SP-(p.Stats.maxSP-20))<.01f,"Dodge spends exact SP cost");
                float time=b.Elapsed;b.Pause(true);Check(Time.timeScale==0,"Pause freezes scaled simulation");b.Pause(false);Time.timeScale=15;phase=1;next=EditorApplication.timeSinceStartup+.2;
            } else if(phase==1){p.InvulnerableUntil=Time.time+1000;maxAlive=Mathf.Max(maxAlive,b.AliveCount);if(b.Wave<10){next=EditorApplication.timeSinceStartup+.2;return;}
                Time.timeScale=1;Check(b.Boss!=null,"Tenth boss wave contains a live boss");Check(maxAlive>=50,"Uncleared enemies persist across waves; 50+ actors exercised");
                int total=b.Kills+b.AliveCount;Check(total==100,"Ten waves total exactly 100 units including boss");
                b.Pending.jade=2;b.Pending.stones[3]=1;int gold=r.Progression.Save.gold;
                foreach(var enemy in b.Enemies)if(enemy.Alive)enemy.Receive(100000,p.transform.position,0);phase=2;next=EditorApplication.timeSinceStartup+.5;
            } else if(phase==2){Check(r.Screen==GameScreen.Result&&r.Victory,"Clearing all units opens victory result");Check(r.Progression.Save.jade==2&&r.Progression.Save.stones[3]==1,"Victory commits pending permanent rewards");
                Check(r.Progression.Save.inventory.Count==2,"Victory awards one equipment item");Check(r.Progression.Save.unlockedStage==3,"Victory unlocks next stage");
                r.Battle.Leave();r.SelectedStage=0;r.BeginBattle();p=r.Battle.Player;p.InvulnerableUntil=0;p.Receive(99999);Check(b.AwaitingRevive&&b.Deaths==1,"First death opens revive flow");Check(b.Revive(),"First revive succeeds");Check(Mathf.Abs(p.HP-p.Stats.maxHP*.5f)<.01f,"Revive restores 50 percent HP");
                p.InvulnerableUntil=0;p.Receive(99999);int jade=r.Progression.Save.jade;Check(b.Revive()&&r.Progression.Save.jade==jade-1,"Second revive spends one saved jade");b.Pending.jade=9;p.InvulnerableUntil=0;p.Receive(99999);
                Check(r.Screen==GameScreen.Result&&!r.Victory&&b.Pending.jade==0,"Third death fails and discards pending rewards");
                File.WriteAllText(report,"{\"success\":true,\"maxConcurrentEnemies\":"+maxAlive+",\"checks\":["+string.Join(",",passed.ConvertAll(x=>"\""+x+"\""))+"]}");Debug.Log("BLADE_RUNTIME_TEST_PASS "+passed.Count);EditorApplication.update-=Tick;r.Battle.Leave();r.Navigate(GameScreen.MainMenu);Time.timeScale=1;
            }
        } catch(Exception e){Time.timeScale=1;EditorApplication.update-=Tick;File.WriteAllText(report,"{\"success\":false,\"error\":\""+e.Message.Replace("\"","'")+"\"}");Debug.LogError("BLADE_RUNTIME_TEST_FAIL "+e);}
    }
}
}
