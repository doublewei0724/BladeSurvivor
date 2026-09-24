using System;
using System.Collections.Generic;
using UnityEngine;

namespace BladeSurvivor {
// 可調數值【介面座標規則】所有 Rect(x,y,width,height) 都以 W×H 的虛擬畫布為基準；X向右、Y向下。
// 可調數值【文字與顏色】Text 最後的 size 為字級；Color(r,g,b,a) 各值0～1，a為不透明度；Border width 為線寬。
// 可調數值【同步修改】移動技能／暫停按鈕時，需同步 BlocksGesture 的輸入阻擋區；技能解鎖／強化上限文字需與玩法一致。
public class GameUI:MonoBehaviour {
    public GameRoot Root;
    const float W=1600,H=900;readonly Color gold=new Color(.78f,.61f,.35f),muted=new Color(.58f,.6f,.62f),paper=new Color(.92f,.88f,.79f),ink=new Color(.035f,.043f,.055f,.96f);
    Font font;Texture2D background;readonly Dictionary<int,GUIStyle> styles=new Dictionary<int,GUIStyle>();Texture2D[] icons;
    float scale,offsetX,offsetY,toastUntil,announceUntil,adUntil;string toast,announcement;int chapter,skillSelected,slotSelected;string selectedItem;Vector2 inventoryScroll;bool confirmQuit,showHelp;
    ProgressionService P=>Root.Progression;GameBalance B=>Root.Balance;BattleController Battle=>Root.Battle;
    // 可調數值【Awake 介面繪製】下方座標／尺寸／字級可按上方統一規則調整。
    void Awake(){font=Resources.Load<Font>("NotoSansTC-Regular");if(font==null)font=Font.CreateDynamicFontFromOSFont(new[]{"PingFang TC","Arial"},24);background=Resources.Load<Texture2D>("MenuBackground");icons=MakeIcons();}
    // 可調數值【Metrics 介面繪製】下方座標／尺寸／字級可按上方統一規則調整。
    void Metrics(){scale=Mathf.Min(UnityEngine.Screen.width/W,UnityEngine.Screen.height/H);offsetX=(UnityEngine.Screen.width-W*scale)*.5f;offsetY=(UnityEngine.Screen.height-H*scale)*.5f;}
    public Vector2 ToGUI(Vector2 point){Metrics();return new Vector2((point.x-offsetX)/scale,(UnityEngine.Screen.height-point.y-offsetY)/scale);}
    // 可調數值【手勢阻擋區】上方130、下方740以外，以及右下(1150,560,450,340)技能區。
    public bool BlocksGesture(Vector2 point){if(Root.Screen!=GameScreen.Battle)return true;Vector2 p=ToGUI(point);return p.y<130||p.y>740||new Rect(1150,560,450,340).Contains(p)||Battle.Paused||Battle.AwaitingRevive||showHelp;}
    public void ResetPage(){inventoryScroll=Vector2.zero;confirmQuit=false;showHelp=false;adUntil=0;selectedItem=P?.Save.weaponId;chapter=Root.SelectedStage/6;}
    // 可調數值【波次公告】顯示2.8秒；Toast一般提示3秒，均不受遊戲暫停倍速影響。
    public void Announce(string value){announcement=value;announceUntil=Time.unscaledTime+2.8f;}
    public void Toast(string value){toast=value;toastUntil=Time.unscaledTime+3;}
    public void OpenRevive(){adUntil=0;confirmQuit=false;}
    GUIStyle Style(int size,TextAnchor align=TextAnchor.MiddleLeft,bool bold=false){int key=size*100+(int)align+(bold?20:0);if(!styles.TryGetValue(key,out var style)){style=new GUIStyle(GUI.skin.label){font=font,fontSize=size,alignment=align,fontStyle=bold?FontStyle.Bold:FontStyle.Normal,wordWrap=true,richText=false,padding=new RectOffset(0,0,0,0)};styles[key]=style;}return style;}
    // 可調數值【Text 介面繪製】下方座標／尺寸／字級可按上方統一規則調整。
    void Text(Rect r,string text,int size=22,Color? color=null,TextAnchor align=TextAnchor.MiddleLeft,bool bold=false){var s=Style(size,align,bold);s.normal.textColor=color??paper;GUI.Label(r,text,s);}
    // 可調數值【Fill 介面繪製】下方座標／尺寸／字級可按上方統一規則調整。
    void Fill(Rect r,Color color){GUI.color=color;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=Color.white;}
    // 可調數值【Border 介面繪製】下方座標／尺寸／字級可按上方統一規則調整。
    void Border(Rect r,Color color,float width=1){Fill(new Rect(r.x,r.y,r.width,width),color);Fill(new Rect(r.x,r.yMax-width,r.width,width),color);Fill(new Rect(r.x,r.y,width,r.height),color);Fill(new Rect(r.xMax-width,r.y,width,r.height),color);}
    // 可調數值【Panel 介面繪製】下方座標／尺寸／字級可按上方統一規則調整。
    void Panel(Rect r,bool accent=false){Fill(r,ink);Border(r,accent?gold:new Color(.25f,.26f,.28f));}
    bool Button(Rect r,string text,Action action=null,bool enabled=true,bool primary=false,int size=22){bool hover=r.Contains(ToGUI(Input.mousePosition));
        Fill(r,!enabled?new Color(.09f,.1f,.12f):primary?(hover?new Color(.61f,.42f,.21f):new Color(.43f,.29f,.145f)):(hover?new Color(.17f,.19f,.21f):new Color(.065f,.078f,.09f,.95f)));
        Border(r,enabled?(primary?gold:new Color(.32f,.33f,.33f)):new Color(.2f,.21f,.22f));Text(r,text,size,enabled?paper:muted,TextAnchor.MiddleCenter);
        bool previous=GUI.enabled;GUI.enabled=enabled;bool click=GUI.Button(r,GUIContent.none,GUIStyle.none);GUI.enabled=previous;if(click){Root.Audio.Play(2);action?.Invoke();}return click;}
    // 可調數值【Line 介面繪製】下方座標／尺寸／字級可按上方統一規則調整。
    void Line(float x,float y,float width,Color? color=null)=>Fill(new Rect(x,y,width,1),color??gold);
    // 可調數值【Image 介面繪製】下方座標／尺寸／字級可按上方統一規則調整。
    void Image(Rect r,Texture image,Color? tint=null){GUI.color=tint??Color.white;GUI.DrawTexture(r,image,ScaleMode.ScaleToFit);GUI.color=Color.white;}
    // 可調數值【Bar 介面繪製】下方座標／尺寸／字級可按上方統一規則調整。
    void Bar(Rect r,float value,float max,Color color,string label){Fill(r,new Color(.04f,.045f,.055f,.9f));Fill(new Rect(r.x+2,r.y+2,(r.width-4)*Mathf.Clamp01(value/Mathf.Max(1,max)),r.height-4),color);Border(r,new Color(.55f,.49f,.4f));Text(r,label,16,paper,TextAnchor.MiddleCenter);}
    // 可調數值【OnGUI 介面繪製】下方座標／尺寸／字級可按上方統一規則調整。
    void OnGUI(){if(Root==null||P==null)return;Metrics();GUI.matrix=Matrix4x4.TRS(new Vector3(offsetX,offsetY,0),Quaternion.identity,Vector3.one*scale);
        if(Root.Screen!=GameScreen.Battle&&Root.Screen!=GameScreen.Result){if(background!=null)GUI.DrawTexture(new Rect(0,0,W,H),background,ScaleMode.ScaleAndCrop);else Fill(new Rect(0,0,W,H),ink);
            if(Root.Screen!=GameScreen.MainMenu)Fill(new Rect(0,0,W,H),new Color(.018f,.025f,.038f,.86f));}
        switch(Root.Screen){case GameScreen.MainMenu:MainMenu();break;case GameScreen.Map:Map();break;case GameScreen.Story:Story();break;case GameScreen.Battle:BattleHUD();break;case GameScreen.Result:Result();break;
            case GameScreen.Equipment:Equipment();break;case GameScreen.Skills:Skills();break;case GameScreen.Upgrades:Upgrades();break;case GameScreen.Shop:Shop();break;case GameScreen.Settings:Settings();break;case GameScreen.Recruit:Reserved(false);break;case GameScreen.Endless:Reserved(true);break;}
        if(Time.unscaledTime<toastUntil){var r=new Rect(460,35,680,58);Panel(r,true);Text(r,toast,20,paper,TextAnchor.MiddleCenter);}
        if(!string.IsNullOrEmpty(P.SaveError)){Panel(new Rect(300,5,1000,40),true);Text(new Rect(310,5,980,40),P.SaveError,16,new Color(1,.4f,.3f));}
        GUI.matrix=Matrix4x4.identity;
    }
    // 可調數值【主選單：標題、按鈕、背景圖】下方座標／尺寸／字級可按上方統一規則調整。
    void MainMenu(){
        Fill(new Rect(0,0,650,H),new Color(.015f,.02f,.03f,.26f));Text(new Rect(78,65,450,35),"THE HOLLOW ROAD",18,gold);Line(78,112,70);
        Text(new Rect(73,165,680,190),"BLADE\nSURVIVOR",79,paper,TextAnchor.MiddleLeft,true);
        Text(new Rect(82,373,520,45),"灰燼之路",30,gold);Text(new Rect(82,425,560,65),"怪物終將倒下。人們仍會前行。\n握緊手中的劍，走過最後的長夜。",21,new Color(.72f,.73f,.72f));
        Button(new Rect(82,538,355,67),"踏上旅途     →",()=>Root.Navigate(GameScreen.Map),true,true,25);
        Button(new Rect(82,620,355,55),"極限模式",()=>Root.Navigate(GameScreen.Endless));
        Button(new Rect(82,690,170,50),"設定",()=>Root.Navigate(GameScreen.Settings));Button(new Rect(267,690,170,50),"操作指南",()=>showHelp=true);
        Text(new Rect(82,809,600,28),"2.5D ACTION RPG     /     PLAYABLE PROTOTYPE",14,muted);Text(new Rect(1210,830,310,30),"一把孤劍，一條未完的路。",17,paper,TextAnchor.MiddleRight);
        if(showHelp)Help();
    }
    // 可調數值【頁面標題與貨幣列】下方座標／尺寸／字級可按上方統一規則調整。
    void Header(string title,string subtitle){Text(new Rect(66,38,650,47),title,34,paper,true?TextAnchor.MiddleLeft:TextAnchor.MiddleLeft,true);Text(new Rect(68,86,800,30),subtitle,16,muted);
        Text(new Rect(900,47,165,35),$"旅人  Lv.{P.Save.level}",21,gold);Text(new Rect(1090,47,160,35),$"金幣  {P.Save.gold:N0}",21,gold);Text(new Rect(1270,47,130,35),$"勾玉  {P.Save.jade}",21,new Color(.4f,.85f,.7f));
        Button(new Rect(1435,40,98,45),"返回",()=>Root.Navigate(Root.Screen==GameScreen.Map||Root.Screen==GameScreen.Settings?GameScreen.MainMenu:GameScreen.Map));Line(66,130,1468,new Color(.28f,.28f,.29f));}
    // 可調數值【底部導覽列】下方座標／尺寸／字級可按上方統一規則調整。
    void Nav(){GameScreen[] screens={GameScreen.Map,GameScreen.Skills,GameScreen.Equipment,GameScreen.Upgrades,GameScreen.Recruit,GameScreen.Shop};string[] names={"旅途","技能","裝備","強化","招募","商城"};
        Fill(new Rect(0,805,W,95),new Color(.026f,.032f,.044f,.97f));Line(66,805,1468,new Color(.31f,.29f,.25f));for(int i=0;i<6;i++){int j=i;Button(new Rect(66+i*246,829,232,48),names[i],()=>Root.Navigate(screens[j]),true,Root.Screen==screens[i],21);}}
    // 可調數值【關卡地圖：每章6關、節點與翻頁】下方座標／尺寸／字級可按上方統一規則調整。
    void Map(){Header("劇情旅途",$"第 {chapter+1} 章   /   灰燼的回聲");Button(new Rect(68,155,110,42),"← 前章",()=>chapter--,chapter>0);Button(new Rect(1420,155,110,42),"後章 →",()=>chapter++,chapter<4);
        string[] roman={"I","II","III","IV","V","VI"};for(int i=0;i<6;i++){int stage=chapter*6+i;var spec=B.stages[stage];bool unlocked=stage<=P.Save.unlockedStage;float x=85+i*247,y=270+(i%2)*72;
            Line(x+120,y+48,126,new Color(.35f,.29f,.2f));var r=new Rect(x,y,195,175);Panel(r,unlocked);Text(new Rect(x,y+10,195,47),spec.boss?"♜":roman[i],35,unlocked?gold:muted,TextAnchor.MiddleCenter);
            Text(new Rect(x+10,y+58,175,35),spec.title,22,unlocked?paper:muted,TextAnchor.MiddleCenter);Text(new Rect(x,y+99,195,30),$"建議 Lv.{spec.level}  ·  {(spec.boss?"首領":"殲滅")}",15,muted,TextAnchor.MiddleCenter);
            Text(new Rect(x,y+133,195,30),unlocked?Stars(P.Save.stars[stage]):"尚未抵達",20,gold,TextAnchor.MiddleCenter);
            if(unlocked&&GUI.Button(r,GUIContent.none,GUIStyle.none))Root.SelectStage(stage);}
        Panel(new Rect(85,590,1430,154));Text(new Rect(117,610,990,35),"10 波來襲 · 清除全部 100 名敵人",24,paper);Text(new Rect(117,655,1000,60),"每 15 秒出現下一波。殘敵會留在場上。\n180 秒內完成獲得二星；再將有效受傷控制在 3 次內，即可三星。",19,muted);
        Button(new Rect(1200,630,278,62),"繼續旅途 →",()=>Root.SelectStage(P.Save.unlockedStage),true,true);Nav();}
    // 可調數值【劇情頁：文字與出發按鈕】下方座標／尺寸／字級可按上方統一規則調整。
    void Story(){var spec=B.stages[Root.SelectedStage];Text(new Rect(90,76,650,40),$"CHAPTER {Root.SelectedStage/6+1:00}     /     STAGE {Root.SelectedStage+1:00}",20,gold);
        Text(new Rect(86,170,1200,110),spec.title,66,paper,TextAnchor.MiddleLeft,true);Line(90,310,100);
        Text(new Rect(90,370,1080,160),spec.story,32,paper);Text(new Rect(92,598,1060,55),$"{spec.subtitle}  ·  建議 Lv.{spec.level}  ·  100 名敵人"+(spec.boss?"  ·  巨斧亡將鎮守終波":""),21,gold);
        Button(new Rect(90,733,225,63),"返回地圖",()=>Root.Navigate(GameScreen.Map));Button(new Rect(1170,733,335,63),"拔劍 · 進入戰場 →",()=>{P.Save.tutorialSeen=true;P.Persist();Root.BeginBattle();},true,true,24);}
    public static string Stars(int count)=>new string('★',Mathf.Clamp(count,0,3))+new string('☆',3-Mathf.Clamp(count,0,3));
    // 可調數值【戰鬥介面：血條、技能、連擊與蓄力】下方座標／尺寸／字級可按上方統一規則調整。
    void BattleHUD(){var p=Battle.Player;if(p==null)return;
        Panel(new Rect(26,23,362,111));Text(new Rect(44,31,314,28),$"旅人  Lv.{P.Save.level}   /   {P.Weapon.Name}",17,paper);Bar(new Rect(44,67,326,23),p.HP,p.Stats.maxHP,new Color(.6f,.12f,.13f),$"HP   {Mathf.CeilToInt(p.HP)} / {p.Stats.maxHP:0}");Bar(new Rect(44,98,326,18),p.SP,p.Stats.maxSP,new Color(.12f,.4f,.62f),$"SP   {Mathf.CeilToInt(p.SP)} / {p.Stats.maxSP:0}");
        Panel(new Rect(565,23,470,87));Text(new Rect(580,28,440,37),$"WAVE  {Battle.Wave:00} / 10      {TimeLabel(Battle.Elapsed)}",28,paper,TextAnchor.MiddleCenter);
        Text(new Rect(580,70,440,24),$"擊倒 {Battle.Kills} / 100   ·   受傷 {Battle.Hits} 次   ·   "+(Battle.Wave<10?$"下波 {Mathf.CeilToInt(Battle.Wave*B.waveInterval-Battle.Elapsed)}s":"清除殘敵"),16,gold,TextAnchor.MiddleCenter);
        Button(new Rect(1440,25,130,49),"Ⅱ  暫停",()=>{Battle.Pause(true);Root.Gestures.ResetGesture();});Button(new Rect(1440,84,130,40),"操作",()=>{Battle.Pause(true);showHelp=true;},true,false,17);
        var boss=Battle.Boss;if(boss!=null){Text(new Rect(520,123,560,27),"巨斧亡將",21,new Color(.95f,.65f,.43f),TextAnchor.MiddleCenter);Bar(new Rect(520,156,560,15),boss.HP,boss.MaxHP,new Color(.63f,.17f,.15f),"");}
        DrawWorldLabels();
        for(int slot=0;slot<4;slot++){int s=slot,id=P.Save.equippedSkills[slot];float x=1170+(slot%2)*183,y=594+(slot/2)*125;Rect r=new Rect(x,y,165,110);Panel(r,slot<P.Slots);
            if(id>=0){Image(new Rect(x+15,y+9,64,64),icons[id]);Text(new Rect(x+82,y+12,72,27),$"{slot+1}",16,muted,TextAnchor.MiddleRight);Text(new Rect(x+8,y+79,149,25),B.skills[id].title,18,paper,TextAnchor.MiddleCenter);
                if(p.SkillCooldowns[id]>0){Fill(new Rect(x+1,y+1,163,108),new Color(0,0,0,.6f));Text(r,$"{p.SkillCooldowns[id]:0.0}",29,paper,TextAnchor.MiddleCenter);}
                if(!Battle.Paused&&!Battle.AwaitingRevive&&GUI.Button(r,GUIContent.none,GUIStyle.none))p.Skill(s);}
            else Text(r,slot<P.Slots?"未裝備":"Lv."+new[]{1,5,10,15}[slot]+" 解鎖",19,muted,TextAnchor.MiddleCenter);}
        if(p.KnockedDown)Text(new Rect(550,710,500,35),"無敵 · 起身中",22,new Color(.5f,.85f,1),TextAnchor.MiddleCenter);
        else if(p.ComboStep>0)Text(new Rect(620,690,360,35),p.ComboStep==4?"第四擊 · 躍斬":"連擊  "+p.ComboStep+" / 4",21,gold,TextAnchor.MiddleCenter);
        if(p.Charging){Bar(new Rect(580,730,440,17),p.ChargeTime,p.Stats.chargeFull,gold,"");Text(new Rect(550,753,500,35),p.ActionLabel,21,gold,TextAnchor.MiddleCenter);}
        Text(new Rect(30,817,1080,34),"點擊持續移動  ·  雙擊閃避  ·  滑動突進  ·  長按蓄力",19,new Color(.8f,.8f,.76f));Text(new Rect(30,854,1000,25),"WASD 移動   SPACE 閃避   SHIFT 突進   E 蓄力   1–4 技能   ESC 暫停",14,muted);
        Text(new Rect(30,751,510,30),$"待結算    ◇ {Battle.Pending.jade}    強化石 {Battle.Pending.stones[0]} / {Battle.Pending.stones[1]} / {Battle.Pending.stones[2]} / {Battle.Pending.stones[3]}",17,gold);
        if(Time.unscaledTime<announceUntil){float a=Mathf.Min(1,(announceUntil-Time.unscaledTime));Fill(new Rect(490,215,620,65),new Color(.025f,.025f,.03f,.84f*a));Text(new Rect(490,215,620,65),announcement,29,new Color(gold.r,gold.g,gold.b,a),TextAnchor.MiddleCenter);}
        if(Battle.AwaitingRevive)Revive();else if(Battle.Paused&&!showHelp)Pause();if(showHelp)Help();
    }
    // 可調數值【DrawWorldLabels 介面繪製】下方座標／尺寸／字級可按上方統一規則調整。
    void DrawWorldLabels(){var cam=Root.BattleCamera.GetComponent<Camera>();foreach(var e in Battle.Enemies){if(!e.Alive||e.HP>=e.MaxHP||e.Kind==EnemyKind.Boss)continue;Vector3 screen=cam.WorldToScreenPoint(e.transform.position+Vector3.up*2.5f);if(screen.z<=0)continue;Vector2 point=ToGUI(screen);Bar(new Rect(point.x-23,point.y,46,5),e.HP,e.MaxHP,new Color(.8f,.16f,.13f),"");}
        foreach(var d in Root.Effects.labels){Vector3 screen=cam.WorldToScreenPoint(d.position);Vector2 point=ToGUI(screen);Color color=d.color;color.a=Mathf.Clamp01(d.remaining*3);Text(new Rect(point.x-60,point.y-20,120,40),d.text,24,color,TextAnchor.MiddleCenter,true);}}
    // 可調數值【Shade 介面繪製】下方座標／尺寸／字級可按上方統一規則調整。
    void Shade(){Fill(new Rect(0,0,W,H),new Color(.008f,.012f,.025f,.8f));}
    // 可調數值【暫停選單】下方座標／尺寸／字級可按上方統一規則調整。
    void Pause(){Shade();Panel(new Rect(535,215,530,445),true);Text(new Rect(550,245,500,57),"暫歇片刻",38,paper,TextAnchor.MiddleCenter);Text(new Rect(570,314,460,45),"所有戰鬥計時已暫停",18,muted,TextAnchor.MiddleCenter);
        Button(new Rect(610,382,380,58),"繼續戰鬥",()=>Battle.Pause(false),true,true);Button(new Rect(610,455,380,55),"操作指南",()=>showHelp=true);
        Button(new Rect(610,526,380,55),"放棄本次旅途",()=>confirmQuit=true);
        if(confirmQuit){Panel(new Rect(450,285,700,310),true);Text(new Rect(495,322,610,95),"放棄後，本場尚未結算的強化石與勾玉將失去。",26,paper,TextAnchor.MiddleCenter);
            Button(new Rect(505,485,270,58),"繼續戰鬥",()=>confirmQuit=false);Button(new Rect(825,485,270,58),"確定放棄",()=>{confirmQuit=false;Battle.Fail();},true,true);}}
    // 可調數值【操作說明】下方座標／尺寸／字級可按上方統一規則調整。
    void Help(){Shade();Panel(new Rect(330,135,940,630),true);Text(new Rect(385,173,830,60),"戰鬥指南",37,paper);Line(385,249,830);string[] rows={"單點地面  →  持續朝點擊方向移動","雙擊  →  閃避，消耗 20 SP；中段短暂无敵","滑動  →  路徑斬擊，消耗 35 SP；仍會受傷","長按再放開  →  三階蓄力斬；蓄力時受擊會中斷","靠近敵人自動揮劍。注意紅色攻擊預警。","電腦：WASD ／ Space ／ Shift ／ 按住 E ／ 1–4"};for(int i=0;i<rows.Length;i++)Text(new Rect(385,276+i*57,840,42),rows[i],23,i==5?gold:paper);
        Button(new Rect(950,679,265,53),"明白了",()=>{showHelp=false;if(Root.Screen==GameScreen.Battle)Battle.Pause(false);},true,true);}
    // 可調數值【復活對話框】下方座標／尺寸／字級可按上方統一規則調整。
    void Revive(){Shade();Panel(new Rect(420,200,760,495),true);Text(new Rect(460,237,680,68),"劍尚未折",43,paper,TextAnchor.MiddleCenter);Text(new Rect(490,318,620,62),$"第 {Battle.Deaths} 次倒下。\n復活恢復 50% HP / SP，獲得 2 秒無敵。",22,muted,TextAnchor.MiddleCenter);
        Text(new Rect(490,400,620,55),"待結算的戰利品仍在等待你。",24,gold,TextAnchor.MiddleCenter);
        if(adUntil>0){float left=adUntil-Time.unscaledTime;Text(new Rect(510,485,580,54),$"模擬獎勵廣告 · {Mathf.CeilToInt(Mathf.Max(0,left))} 秒",24,paper,TextAnchor.MiddleCenter);
            if(left<=0){adUntil=0;Root.Ads.TryGrantRevive(Battle);}}
        else Button(new Rect(550,492,500,65),Battle.Deaths==1?(P.Save.adFree?"免廣告 · 免費復活":"觀看模擬廣告 · 免費復活"):$"消耗 1 勾玉復活（持有 {P.Save.jade}）",()=>{if(Battle.Deaths==1&&!P.Save.adFree)adUntil=Time.unscaledTime+Root.Ads.Duration;else Battle.Revive();},Battle.Deaths==1||P.Save.jade>0,true,23);
        Button(new Rect(625,594,350,50),"結束本次旅途",()=>Battle.Fail(),adUntil==0);}
    // 可調數值【結算獎勵】下方座標／尺寸／字級可按上方統一規則調整。
    void Result(){Shade();Panel(new Rect(365,65,870,768),true);Text(new Rect(415,104,770,61),Root.Victory?"黎明，尚有希望。":"灰燼不會是終點。",40,paper,TextAnchor.MiddleCenter);
        Text(new Rect(415,171,770,87),Root.Victory?Stars(Battle.ResultStars):"旅途未竟",58,gold,TextAnchor.MiddleCenter);Text(new Rect(415,277,770,38),B.stages[Battle.StageIndex].title,26,paper,TextAnchor.MiddleCenter);Line(425,338,750);
        Text(new Rect(442,361,710,100),$"戰鬥用時   {TimeLabel(Battle.Elapsed)}         有效受傷   {Battle.Hits}\n擊倒敵人   {Battle.Kills} / 100         復活次數   {Battle.Revives} / 2",24,paper);
        if(Root.Victory){Text(new Rect(442,475,710,45),$"+ {Battle.RewardExp} EXP       + {Battle.RewardGold} 金幣       + {Battle.Pending.jade} 勾玉",25,gold);
            Text(new Rect(442,533,710,72),$"獲得裝備：{Battle.RewardEquipment?.Name}\n強化石：S {Battle.Pending.stones[0]} · M {Battle.Pending.stones[1]} · L {Battle.Pending.stones[2]} · XL {Battle.Pending.stones[3]}",21,paper);
            Text(new Rect(442,631,710,38),"獎勵已寫入存檔。下一段旅途正等著你。",18,muted);}
        else Text(new Rect(442,487,710,143),"本場待結算掉落已清除。\n觀察敵人的紅色起手，用閃避避開重擊。\n也可以重返已通關關卡，累積裝備與強化。",23,muted);
        Button(new Rect(435,725,340,63),"返回營地",()=>Root.ExitResult());Button(new Rect(825,725,340,63),Root.Victory?"下一段旅途 →":"再次挑戰 →",()=>{Battle.Leave();if(Root.Victory)Root.SelectedStage=Mathf.Min(B.stages.Length-1,Battle.StageIndex+1);Root.SelectStage(Root.SelectedStage);},true,true);}
    string TimeLabel(float seconds)=>$"{(int)seconds/60:00}:{(int)seconds%60:00}";
    Color RarityColor(Rarity r)=>r==Rarity.Legendary?gold:r==Rarity.Epic?new Color(.73f,.4f,.95f):r==Rarity.Rare?new Color(.38f,.65f,1):r==Rarity.Uncommon?new Color(.45f,.79f,.56f):new Color(.7f,.72f,.74f);
    // 可調數值【裝備格子與強化按鈕】下方座標／尺寸／字級可按上方統一規則調整。
    void Equipment(){Header("行囊與裝備","裝備 · 品質 · 詞條 · 武器強化");var item=P.Save.inventory.Find(x=>x.id==selectedItem)??P.Weapon;
        Panel(new Rect(66,163,365,581));Text(new Rect(91,182,310,39),"旅人裝備",27,paper);Text(new Rect(91,240,310,105),$"武器  {P.Weapon.Name}\n套服  {(P.Suit==null?"未裝備":P.Suit.Name)}\n飾品  預留欄位 × 2",20,paper);
        var stats=P.Stats();Line(91,366,313);Text(new Rect(91,390,310,220),$"生命     {stats.maxHP:0}\n精力     {stats.maxSP:0}\n武器     {stats.minDamage:0} – {stats.maxDamage:0}\n防禦     {stats.defense:0}\n爆擊     {stats.crit*100:0.#}%",24,paper);
        Bar(new Rect(92,658,309,21),P.Save.exp,B.RequiredExp(P.Save.level),new Color(.22f,.48f,.53f),$"Lv.{P.Save.level}  ·  {P.Save.exp} / {B.RequiredExp(P.Save.level)} EXP");
        inventoryScroll=GUI.BeginScrollView(new Rect(451,163,568,581),inventoryScroll,new Rect(0,0,540,Mathf.Max(570,Mathf.CeilToInt(P.Save.inventory.Count/3f)*151)));
        for(int i=0;i<P.Save.inventory.Count;i++){var gear=P.Save.inventory[i];float x=(i%3)*181,y=(i/3)*151;var r=new Rect(x,y,169,138);Panel(r,gear.id==item.id);Border(r,RarityColor(gear.rarity));
            Image(new Rect(x+48,y+10,74,64),icons[gear.kind==EquipmentKind.Weapon?1:2]);Text(new Rect(x+6,y+79,157,27),gear.Name,17,RarityColor(gear.rarity),TextAnchor.MiddleCenter);
            Text(new Rect(x+7,y+109,155,22),$"Lv.{B.tierLevels[gear.tier]}  +{gear.enhance}"+(gear.id==P.Save.weaponId||gear.id==P.Save.suitId?"  裝備中":""),13,muted,TextAnchor.MiddleCenter);if(GUI.Button(r,GUIContent.none,GUIStyle.none))selectedItem=gear.id;}
        GUI.EndScrollView();Panel(new Rect(1040,163,494,581),true);Text(new Rect(1065,183,444,45),item.Name,27,RarityColor(item.rarity));Text(new Rect(1065,237,444,30),$"需求 Lv.{B.tierLevels[item.tier]}   ·   強化 +{item.enhance}",18,muted);
        string traits="";string[] weaponTraits={"爆擊率 +5%","對 Boss 傷害 +12%","滿蓄力時間 −0.5 秒"},suitTraits={"HP / SP 拾取回復 +25%","閃避 / 突進 SP 消耗 −20%"};for(int i=0;i<(item.kind==EquipmentKind.Weapon?3:2);i++)if(item.Has(i))traits+="◆ "+(item.kind==EquipmentKind.Weapon?weaponTraits[i]:suitTraits[i])+"\n";
        if(item.kind==EquipmentKind.Suit&&item.rarity==Rarity.Legendary)traits+="◆ 開場護盾：抵消第一次命中";Text(new Rect(1065,288,444,97),string.IsNullOrEmpty(traits)?"無附加詞條":traits,19,paper);
        Button(new Rect(1065,404,444,49),item.id==P.Save.weaponId||item.id==P.Save.suitId?"目前裝備中":"裝備",()=>{P.Equip(item);Toast("裝備已更新");},P.Save.level>=B.tierLevels[item.tier],true);
        if(item.kind==EquipmentKind.Weapon){Text(new Rect(1065,474,444,33),$"武器淬鍊  ·  已用 {item.attempts} / 5 次",21,gold);Bar(new Rect(1065,524,444,20),item.energy,(item.enhance+1)*10,new Color(.21f,.48f,.63f),item.enhance==10?"已達 +10":$"{item.energy} / {(item.enhance+1)*10} ENERGY");
            for(int i=0;i<4;i++){int stone=i;Button(new Rect(1065+i*113,566,105,63),new[]{"S","M","L","XL"}[i]+$" ×{P.Save.stones[i]}",()=>{if(P.Enhance(item,stone,out int energy,out bool crit))Toast($"{(crit?"爆發淬鍊！":"淬鍊成功")} +{energy} 能量 · 武器 +{item.enhance}");},P.Save.stones[i]>0&&item.attempts<5&&item.enhance<10,false,19);}
            Text(new Rect(1065,653,444,58),"20% 機率獲得雙倍能量。\n每把武器最多投入 5 次，剩餘能量跨級保留。",16,muted);}
        else Text(new Rect(1065,504,444,88),"套服提供防禦與品質詞條。\n第一版僅武器可強化。",20,muted);Nav();}
    // 可調數值【技能卡片及裝備欄】下方座標／尺寸／字級可按上方統一規則調整。
    void Skills(){Header("劍技","技能只計算冷卻，不消耗 SP。選擇技能後，可指定裝備欄位。");
        for(int i=0;i<7;i++){int id=i;float x=66+(i%4)*240,y=170+(i/4)*217;var s=B.skills[i];bool unlocked=P.Save.level>=s.unlock;Panel(new Rect(x,y,222,196),skillSelected==i);Image(new Rect(x+68,y+15,85,85),icons[i],unlocked?Color.white:new Color(.35f,.35f,.35f));
            Text(new Rect(x+8,y+108,206,35),s.title,24,unlocked?paper:muted,TextAnchor.MiddleCenter);Text(new Rect(x+8,y+150,206,28),P.Save.skillLevels[i]>0?$"Lv.{P.Save.skillLevels[i]} · {s.cooldown:0}s 冷卻":$"Lv.{s.unlock} 解鎖",17,muted,TextAnchor.MiddleCenter);if(GUI.Button(new Rect(x,y,222,196),GUIContent.none,GUIStyle.none))skillSelected=id;}
        var spec=B.skills[skillSelected];int level=P.Save.skillLevels[skillSelected];Panel(new Rect(1050,170,484,433),true);Text(new Rect(1080,198,420,48),spec.title,32,gold);Text(new Rect(1080,271,420,126),spec.description,24,paper);Text(new Rect(1080,413,420,40),$"冷卻 {spec.cooldown:0}s  ·  技能等級 {level}/5",20,muted);
        if(level==0)Button(new Rect(1080,501,424,61),$"學習 · {spec.cost} 金幣",()=>{P.BuySkill(skillSelected);},P.Save.level>=spec.unlock&&P.Save.gold>=spec.cost,true);
        else Button(new Rect(1080,501,424,61),level>=5?"已達最高等級":$"升級 · {B.skillUpgradeCosts[level-1]} 金幣",()=>P.UpgradeSkill(skillSelected),level<5&&P.Save.gold>=(level<5?B.skillUpgradeCosts[level-1]:0),true);
        Text(new Rect(66,638,950,34),"裝備欄位 · 點擊欄位以裝入目前選取的技能",22,gold);for(int i=0;i<4;i++){int slot=i;int id=P.Save.equippedSkills[i];Button(new Rect(66+i*371,692,350,66),i>=P.Slots?$"欄位 {i+1} · Lv.{new[]{1,5,10,15}[i]} 解鎖":$"{i+1}  {(id<0?"空欄位":B.skills[id].title)}",()=>{if(P.AssignSkill(skillSelected,slot))Toast("技能已裝備");else Toast("請先學習此技能");},i<P.Slots,false,22);}Nav();}
    // 可調數值【永久能力強化卡片】下方座標／尺寸／字級可按上方統一規則調整。
    void Upgrades(){Header("永久強化","將每一次歸來，化作下一次出發的力量。");string[] names={"攻擊","防禦","生命","精力","爆擊"},desc={"每級傷害 +2%","每級承傷 −1%","每級最大 HP +20","每級最大 SP +5","每級爆擊率 +0.5%"};
        for(int i=0;i<5;i++){int stat=i,lv=P.Save.upgrades[i];float x=66+i*296;Panel(new Rect(x,189,280,486),lv>0);Image(new Rect(x+86,227,108,108),icons[(i+1)%7]);Text(new Rect(x+12,362,256,54),names[i],33,paper,TextAnchor.MiddleCenter);
            Text(new Rect(x+12,429,256,34),$"Lv. {lv} / 20",25,gold,TextAnchor.MiddleCenter);Text(new Rect(x+12,482,256,35),desc[i],19,muted,TextAnchor.MiddleCenter);
            Button(new Rect(x+20,577,240,60),lv>=20?"已達上限":$"↑  {B.upgradeCosts[lv]} 金幣",()=>{P.Upgrade(stat);Toast("永久屬性已提升");},lv<20&&P.Save.gold>=(lv<20?B.upgradeCosts[lv]:0),true,21);}
        Text(new Rect(66,709,1468,44),"武器強化請前往「裝備」頁，選取武器並投入強化石。",21,muted,TextAnchor.MiddleCenter);Nav();}
    // 可調數值【商城物品與測試補給】下方座標／尺寸／字級可按上方統一規則調整。
    void Shop(){Header("誓約商店","金色裝備僅能以勾玉購得。關卡裝備掉落最高為紫色。");int tier=B.Tier(P.Save.level);string[] names={"誓約長劍","誓約雙刃","誓約重劍","誓約戰甲"};
        for(int i=0;i<4;i++){int which=i;float x=66+i*371;Panel(new Rect(x,181,350,421),true);Image(new Rect(x+109,213,132,132),icons[i==3?2:1]);Text(new Rect(x+15,365,320,51),names[i],28,gold,TextAnchor.MiddleCenter);Text(new Rect(x+20,431,310,39),$"需求 Lv.{B.tierLevels[tier]} · 完整金色詞條",18,muted,TextAnchor.MiddleCenter);
            Button(new Rect(x+24,510,302,61),$"購買 · {B.shopPrices[tier]} 勾玉",()=>{P.BuyEquipment(which==3?EquipmentKind.Suit:EquipmentKind.Weapon,tier,which==0?WeaponKind.Sword:which==1?WeaponKind.DualBlades:WeaponKind.Greatsword);Toast("金色裝備已放入行囊");},P.Save.jade>=B.shopPrices[tier],true);}
        Panel(new Rect(66,633,1468,122));Text(new Rect(88,651,770,39),"原型體驗補給",24,paper);Text(new Rect(88,696,820,31),"免費模擬購買：2000 金幣、30 勾玉、各 2 顆強化石。不會產生付款。",17,muted);
        Button(new Rect(1170,666,336,56),"領取測試補給",()=>{Root.Store.GrantPrototypeSupply(P);Toast("測試補給已入帳");},true,false,22);Nav();}
    // 可調數值【音量與設定】下方座標／尺寸／字級可按上方統一規則調整。
    void Settings(){Header("設定","調整旅途中的聲音與操作說明。");Panel(new Rect(280,209,1040,466));Text(new Rect(325,250,650,48),"音效與環境聲",28,paper);float volume=GUI.HorizontalSlider(new Rect(325,333,750,30),P.Save.volume,0,1);if(Mathf.Abs(volume-P.Save.volume)>.005f){P.Save.volume=volume;Root.Audio.Volume(volume);}Text(new Rect(1110,310,150,50),$"{volume*100:0}%",25,gold);
        Button(new Rect(325,406,950,61),P.Save.adFree?"原型：免廣告權益已啟用":"原型：啟用免廣告權益（免費模擬）",()=>{P.Save.adFree=!P.Save.adFree;P.Persist();});Button(new Rect(325,506,450,61),"操作指南",()=>showHelp=true);Button(new Rect(825,506,450,61),"儲存並返回首頁",()=>{P.Persist();Root.Navigate(GameScreen.MainMenu);},true,true);
        Text(new Rect(325,598,950,36),"進度自動儲存在本機。此原型沒有正式廣告、付款或網路帳號。",18,muted);if(showHelp)Help();}
    // 可調數值【Reserved 介面繪製】下方座標／尺寸／字級可按上方統一規則調整。
    void Reserved(bool endless){Header(endless?"極限模式":"旅人招募",endless?"第二階段 · 無盡之路":"第二階段 · 同行之人");Panel(new Rect(280,250,1040,420),true);Text(new Rect(340,292,920,68),endless?"更深的黑暗，仍在等待。":"這段旅途，先由你獨自走完。",36,paper,TextAnchor.MiddleCenter);
        Text(new Rect(365,395,870,110),endless?"極限模式已預留入口。\n依企劃，第一版先完成劇情關卡、戰鬥與養成。\n完成體驗後，再擴充無限層數與排行榜。":"招募系統已預留入口。\n第一版的主角、裝備與技能可以在營地管理。",24,muted,TextAnchor.MiddleCenter);
        Button(new Rect(585,560,430,61),"前往劇情旅途",()=>Root.Navigate(GameScreen.Map),true,true);Nav();}
    // 可調數值【技能圖示：128像素畫布內的線段／圓形與配色】下方座標／尺寸／字級可按上方統一規則調整。
    Texture2D[] MakeIcons(){var result=new Texture2D[7];for(int k=0;k<7;k++){int n=128;var tex=new Texture2D(n,n,TextureFormat.RGBA32,false);tex.name="Skill icon "+k;var pixels=new Color[n*n];Color c=k==1?new Color(.35f,.72f,1):k==2?new Color(1,.7f,.2f):k==4?new Color(.8f,.1f,.22f):new Color(1,.3f,.1f);
            for(int y=0;y<n;y++)for(int x=0;x<n;x++){float dx=(x-63.5f)/64,dy=(y-63.5f)/64,r=Mathf.Sqrt(dx*dx+dy*dy),a=Mathf.Atan2(dy,dx);Color value=Color.clear;if(r<.97f)value=new Color(.035f,.04f,.06f,1);
                if(r>.87f&&r<.94f)value=gold;float spiral=.38f+.16f*Mathf.Sin(a*2+k);bool slash=k==1?Mathf.Abs(dx-dy)<.1f&&r<.72f:k==2?Mathf.Abs(dx)<.17f&&dy>-.6f&&dy<.65f:k==4?r<.48f&&r>.25f:Mathf.Abs(r-spiral)<.065f&&a>-.7f;
                if(slash)value=c;if(r<.75f&&r>.71f&&a>k*.35f-2)value=c*.7f;pixels[y*n+x]=value;}tex.SetPixels(pixels);tex.Apply();result[k]=tex;}return result;}
}
}
