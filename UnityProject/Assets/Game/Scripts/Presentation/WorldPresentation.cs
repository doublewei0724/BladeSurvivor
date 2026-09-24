using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace BladeSurvivor {
public static class ArenaBuilder {
    public static GameObject Build(Vector2 halfSize){
        var root=new GameObject("灰燼荒原 · Ashen Expanse");
        var groundMaterial=new Material(Art.Material);groundMaterial.mainTexture=Resources.Load<Texture2D>("CourtyardStone");
    // 可調數值【地面材質】RGB 倍率 2.3／2.1／1.9、金屬 .05、光滑 .15；貼圖路徑 CourtyardStone。
        groundMaterial.SetColor("_Color",new Color(2.3f,2.1f,1.9f));groundMaterial.SetFloat("_Metallic",.05f);groundMaterial.SetFloat("_Glossiness",.15f);
        // Separate chunks allow the renderer to cull terrain beyond the camera.
    // 可調數值【地圖分塊】邊界外延 32m、每塊 16m；每塊 8×8 格、每格 2m，三者需同步。
        int extentX=Mathf.CeilToInt((halfSize.x+32)/16),extentZ=Mathf.CeilToInt((halfSize.y+32)/16);
        for(int cx=-extentX;cx<extentX;cx++)for(int cz=-extentZ;cz<extentZ;cz++){
    // 可調數值【地形隨機種子】814 控制裝飾分布；73856093／19349663 是座標混合常數，不是玩法倍率。
            var ground=new Geometry();var detail=new Geometry();var random=new System.Random(814+cx*73856093^cz*19349663);
            for(int x=0;x<8;x++)for(int z=0;z<8;z++){
                float px=cx*16+x*2,pz=cz*16+z*2;
    // 可調數值【地面色差與道路】底亮度 .29、隨機幅度 .035；中央道路兩側各 3m；下方 Color 皆 RGB 0～1。
                float shade=.29f+(float)random.NextDouble()*.035f;
                bool path=Mathf.Abs(px)<3||Mathf.Abs(pz)<3;
                Color color=path?new Color(.36f,.33f,.28f):new Color(shade*.88f,shade*.97f,shade);
                ground.Quad(new Vector3(px,0,pz),new Vector3(px,0,pz+2),new Vector3(px+2,0,pz+2),new Vector3(px+2,0,pz),color);
            }
            ground.Object($"Terrain {cx},{cz}",root.transform,groundMaterial).isStatic=true;
            // Low weathered stones and inlaid markers leave the traversable ground open.
    // 可調數值【碎石裝飾】每塊 5 顆；高度 .025m、寬 .7～1.7m、高 .08m、深 1.2m。
            for(int i=0;i<5;i++){
                var pos=new Vector3(cx*16+(float)random.NextDouble()*16,.025f,cz*16+(float)random.NextDouble()*16);
                detail.Ellipsoid(pos,new Vector3(.7f+(float)random.NextDouble(),.08f,1.2f),Art.Iron*.8f,5,2);
            }
    // 可調數值【地面紋章】每 3 塊放一個；32 道標記、半徑 3m、標記尺寸 .12×.025×.35m。
            if(cx%3==0&&cz%3==0){Vector3 center=new Vector3(cx*16+8,.025f,cz*16+8);
                for(int i=0;i<32;i++){float angle=i*Mathf.PI*2/32;detail.Box(center+new Vector3(Mathf.Sin(angle)*3,0,Mathf.Cos(angle)*3),new Vector3(.12f,.025f,.35f),Art.Gold*.7f,Quaternion.Euler(0,angle*Mathf.Rad2Deg,0));}}
            detail.Object($"Weathered markers {cx},{cz}",root.transform,Art.Material).isStatic=true;
        }
        // The outer ruins mark the actual world boundary, many screens from the start.
        var ruins=new Geometry();
    // 可調數值【邊界遺跡】在地圖外 2m，每 8m 一根柱；牆高 1m、厚 .7m。
        for(int side=-1;side<=1;side+=2){
            for(float x=-halfSize.x-2;x<=halfSize.x+2;x+=8)Pillar(ruins,new Vector3(x,0,side*(halfSize.y+2)));
            for(float z=-halfSize.y-2;z<=halfSize.y+2;z+=8)Pillar(ruins,new Vector3(side*(halfSize.x+2),0,z));
            ruins.Box(new Vector3(0,.5f,side*(halfSize.y+2)),new Vector3(halfSize.x*2+4,1,.7f),Art.Iron);
            ruins.Box(new Vector3(side*(halfSize.x+2),.5f,0),new Vector3(.7f,1,halfSize.y*2+4),Art.Iron);
        }
        ruins.Object("Distant boundary ruins",root.transform,Art.Material).isStatic=true;
        return root;
    }
    // 可調數值【柱子造型】底座 1.2×.4×1.2m；柱身高 3m，尖頂到 3.7m，半徑 .4→.28m。
    static void Pillar(Geometry mesh,Vector3 p){mesh.Box(p+Vector3.up*.2f,new Vector3(1.2f,.4f,1.2f),Art.Iron);mesh.Taper(p+Vector3.up*.4f,p+Vector3.up*3,.4f,.28f,Art.Iron,6);mesh.Taper(p+Vector3.up*3,p+Vector3.up*3.7f,.35f,0,Art.Iron,4);}
}
public class BattleCamera:MonoBehaviour {
    public Transform target;public float shake;Vector3 anchor;
    // 可調數值【鏡頭】俯角 42°、後退 30m、正交半高 7.8m（越小越近）、裁切 .1～150m；背景 RGB(.025,.03,.04)。
    void Awake(){transform.rotation=Quaternion.Euler(42,0,0);anchor=-transform.forward*30;transform.position=anchor;
        var c=GetComponent<Camera>();c.orthographic=true;c.orthographicSize=7.8f;c.nearClipPlane=.1f;c.farClipPlane=150;c.clearFlags=CameraClearFlags.SolidColor;c.backgroundColor=new Color(.025f,.03f,.04f);}
    public void SnapToTarget(){if(target!=null)transform.position=anchor+target.position;}
    // 可調數值【跟隨與震動】跟隨平滑速率 12、震動每秒衰减 1.5、位移幅度 .08m。
    void LateUpdate(){Vector3 p=anchor;if(target!=null)p+=target.position;shake=Mathf.MoveTowards(shake,0,Time.unscaledDeltaTime*1.5f);
        transform.position=Vector3.Lerp(transform.position,p,1-Mathf.Exp(-12*Time.unscaledDeltaTime))+(Vector3)UnityEngine.Random.insideUnitCircle*shake*.08f;}
}
public class CombatEffects:MonoBehaviour {
    class Effect {public LineRenderer line;public float time,life,startRadius,endRadius,arc;public Vector3 center,forward;public Color color;}
    public class DamageLabel {public Vector3 position;public string text;public Color color;public float remaining;}
    readonly List<Effect> effects=new List<Effect>();public readonly List<DamageLabel> labels=new List<DamageLabel>();Material lineMaterial;int next;
    // 可調數值【刀光池】96 個特效、每條 25 個點、預設線寬 .085m、圓頭 2 段。
    void Awake(){lineMaterial=new Material(Shader.Find("Sprites/Default"));for(int i=0;i<96;i++){
        var go=new GameObject("Pooled slash "+i);go.transform.SetParent(transform);var l=go.AddComponent<LineRenderer>();l.sharedMaterial=lineMaterial;l.positionCount=25;l.useWorldSpace=true;l.widthMultiplier=.085f;l.numCapVertices=2;l.shadowCastingMode=ShadowCastingMode.Off;l.enabled=false;effects.Add(new Effect{line=l});}}
    // 可調數值【刀光外觀】預設 .28s、線寬 .09m、離地 .12m、半徑從 75% 展開；Ring 預設 .4s、寬 .06m。
    public void Arc(Vector3 p,Vector3 forward,float radius,float arc,Color color,float life=.28f,float width=.09f){
        if(effects.Count==0)return;var e=effects[next++%effects.Count];e.center=p+Vector3.up*.12f;e.forward=forward;e.time=0;e.life=life;e.arc=arc;e.startRadius=radius*.75f;e.endRadius=radius;e.color=color;e.line.enabled=true;e.line.widthMultiplier=width;Draw(e,0);}
    public void Ring(Vector3 p,float radius,Color color,float life=.4f)=>Arc(p,Vector3.forward,radius,360,color,life,.06f);
    void Draw(Effect e,float t){float radius=Mathf.Lerp(e.startRadius,e.endRadius,t);float facing=Mathf.Atan2(e.forward.x,e.forward.z)*Mathf.Rad2Deg;
        for(int i=0;i<25;i++){float a=(facing-e.arc*.5f+e.arc*i/24)*Mathf.Deg2Rad;e.line.SetPosition(i,e.center+new Vector3(Mathf.Sin(a),0,Mathf.Cos(a))*radius);}
        Color c=e.color;c.a*=1-t;e.line.startColor=c;e.line.endColor=c;}
    // 可調數值【傷害跳字】最多約 90 筆、高度 2m、存活 .8s、每秒上飄 .8m。
    public void Number(Vector3 pos,string text,Color color){if(labels.Count>90)labels.RemoveAt(0);labels.Add(new DamageLabel{position=pos+Vector3.up*2,text=text,color=color,remaining=.8f});}
    void Update(){float dt=Time.deltaTime;foreach(var e in effects){if(!e.line.enabled)continue;e.time+=dt;if(e.time>=e.life)e.line.enabled=false;else Draw(e,e.time/e.life);}
        for(int i=labels.Count-1;i>=0;i--){labels[i].remaining-=dt;labels[i].position+=Vector3.up*dt*.8f;if(labels[i].remaining<=0)labels.RemoveAt(i);}}
    public void Clear(){foreach(var e in effects)e.line.enabled=false;labels.Clear();}
}
public class GameAudio:MonoBehaviour {
    AudioSource source,music;AudioClip slash,hit,pickup,win;
    // 可調數值【音效合成】劍 .16s／350→50Hz、受擊 .15s／110→30Hz、拾取 .2s／650→1100Hz、勝利 .8s／260→520Hz。
    void Awake(){source=gameObject.AddComponent<AudioSource>();music=gameObject.AddComponent<AudioSource>();slash=Tone("Sword",.16f,350,50,true);hit=Tone("Impact",.15f,110,30,true);pickup=Tone("Pickup",.2f,650,1100,false);win=Tone("Victory",.8f,260,520,false);
    // 可調數值【背景音】44100Hz 取樣、8 秒循環；55／82.5／110Hz 三音，振幅 .018／.012／.008。
        var clip=AudioClip.Create("Ashen wind",44100*8,1,44100,false);var samples=new float[44100*8];for(int i=0;i<samples.Length;i++){float t=i/44100f;samples[i]=(.018f*Mathf.Sin(t*2*Mathf.PI*55)+.012f*Mathf.Sin(t*2*Mathf.PI*82.5f)+.008f*Mathf.Sin(t*2*Mathf.PI*110))*Mathf.Sin(Mathf.PI*i/samples.Length);}clip.SetData(samples,0);music.clip=clip;music.loop=true;music.Play();}
    // 可調數值【音效質感】主音振幅 .3、雜訊 .25、包絡平方衰減、總增益 .4；44100 為取樣率。
    AudioClip Tone(string name,float duration,float from,float to,bool noise){int count=(int)(44100*duration);var data=new float[count];var rng=new System.Random(name.GetHashCode());float phase=0;for(int i=0;i<count;i++){float t=(float)i/count;phase+=Mathf.Lerp(from,to,t)*Mathf.PI*2/44100;data[i]=(Mathf.Sin(phase)*.3f+(noise?((float)rng.NextDouble()*2-1)*.25f:0))*Mathf.Pow(1-t,2)*.4f;}var clip=AudioClip.Create(name,count,1,44100,false);clip.SetData(data,0);return clip;}
    // 可調數值【音高隨機】每次 .9～1.1 倍；kind 0劍／1受擊／2拾取／3勝利是事件編號，不是音量。
    public void Play(int kind){source.pitch=UnityEngine.Random.Range(.9f,1.1f);source.PlayOneShot(kind==0?slash:kind==1?hit:kind==2?pickup:win);}
    public void Volume(float value){source.volume=value;music.volume=value;}
}
}
