using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace BladeSurvivor {
public class Geometry {
    readonly List<Vector3> vertices=new List<Vector3>(); readonly List<int> triangles=new List<int>(); readonly List<Color> colors=new List<Color>();
    public void Tri(Vector3 a,Vector3 b,Vector3 c,Color color){int i=vertices.Count;vertices.Add(a);vertices.Add(b);vertices.Add(c);colors.Add(color);colors.Add(color);colors.Add(color);triangles.Add(i);triangles.Add(i+1);triangles.Add(i+2);}
    public void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d,Color color){Tri(a,b,c,color);Tri(a,c,d,color);}
    public void Box(Vector3 center,Vector3 size,Color color,Quaternion rotation=default){
        if(rotation==default)rotation=Quaternion.identity;
        Vector3[] p=new Vector3[8];for(int i=0;i<8;i++)p[i]=center+rotation*Vector3.Scale(new Vector3((i&1)==0?-.5f:.5f,(i&2)==0?-.5f:.5f,(i&4)==0?-.5f:.5f),size);
        Quad(p[0],p[2],p[3],p[1],color);Quad(p[4],p[5],p[7],p[6],color);Quad(p[0],p[4],p[6],p[2],color);Quad(p[1],p[3],p[7],p[5],color);Quad(p[2],p[6],p[7],p[3],color);Quad(p[0],p[1],p[5],p[4],color);
    }
    // 可調數值【圓形模型細度】預設 8 邊／5 環；增加可變圓滑但增加面數；size 是完整長寬高。
    public void Ellipsoid(Vector3 center,Vector3 size,Color color,int sides=8,int rings=5){
        for(int y=0;y<rings;y++)for(int x=0;x<sides;x++){
            Vector3 P(int xx,int yy){float a=xx*Mathf.PI*2/sides,b=yy*Mathf.PI/rings;return center+Vector3.Scale(new Vector3(Mathf.Sin(a)*Mathf.Sin(b),Mathf.Cos(b),Mathf.Cos(a)*Mathf.Sin(b)),size*.5f);}
            Quad(P(x,y),P(x+1,y),P(x+1,y+1),P(x,y+1),color);
        }
    }
    // 可調數值【柱／角幾何】預設 6 邊；lower／upper 是底／頂半徑（公尺）。
    public void Taper(Vector3 bottom,Vector3 top,float lower,float upper,Color color,int sides=6){
        Vector3 dir=(top-bottom).normalized;Quaternion rot=Quaternion.FromToRotation(Vector3.up,dir);
        for(int i=0;i<sides;i++){float a=i*Mathf.PI*2/sides,b=(i+1)*Mathf.PI*2/sides;
            var pa=rot*new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));var pb=rot*new Vector3(Mathf.Cos(b),0,Mathf.Sin(b));
            Quad(bottom+pa*lower,top+pa*upper,top+pb*upper,bottom+pb*lower,color);
            Tri(top,top+pa*upper,top+pb*upper,color);Tri(bottom,bottom+pb*lower,bottom+pa*lower,color);}
    }
    // 可調數值【刀身造型】length 是長度，width 是單側寬；刀脊高度 .055m、刀脊位置長度的 40%。
    public void Blade(Vector3 basePoint,float length,float width,Color metal){
        Vector3 a=basePoint+new Vector3(-width,0,0),b=basePoint+new Vector3(width,0,0),tip=basePoint+new Vector3(0,0,length),ridge=basePoint+new Vector3(0,.055f,length*.4f);
        Tri(a,ridge,tip,metal);Tri(ridge,b,tip,metal*.65f);Tri(a,b,ridge,metal);Tri(a,tip,b,metal*.6f);
    }
    // 可調數值【地面貼圖比例】UV 乘 .125，表示每 8m 重複貼圖一次；65535 是網格索引格式界線。
    public Mesh Mesh(string name="Crafted mesh"){var m=new Mesh{name=name,indexFormat=vertices.Count>65535?IndexFormat.UInt32:IndexFormat.UInt16};m.SetVertices(vertices);m.SetColors(colors);m.SetUVs(0,vertices.ConvertAll(v=>new Vector2(v.x*.125f,v.z*.125f)));m.SetTriangles(triangles,0);m.RecalculateNormals();m.RecalculateBounds();return m;}
    public GameObject Object(string name,Transform parent,Material material){var g=new GameObject(name);g.transform.SetParent(parent,false);g.AddComponent<MeshFilter>().sharedMesh=Mesh(name);g.AddComponent<MeshRenderer>().sharedMaterial=material;return g;}
}

public static class Art {
    // 可調數值【模型配色】Iron 鐵／Silver 刃／Leather 皮革／Red 披風／Gold 金飾／Skin 膚色；RGB 0～1。
    public static readonly Color Iron=new Color(.23f,.27f,.3f),Silver=new Color(.67f,.71f,.73f),Leather=new Color(.16f,.12f,.105f),Red=new Color(.42f,.075f,.075f),Gold=new Color(.66f,.46f,.23f),Skin=new Color(.73f,.56f,.43f);
    static Material material;public static Material Material {get {if(material==null){material=new Material(Resources.Load<Shader>("IronVertex"));material.enableInstancing=true;}return material;}}
    static readonly Dictionary<int,GameObject> templates=new Dictionary<int,GameObject>();
    public static GameObject Humanoid(int kind,Transform parent){
        if(templates.TryGetValue(kind,out var template)&&template!=null){var copy=Object.Instantiate(template,parent);copy.name=kind<0?"Wanderer":((EnemyKind)kind).ToString();copy.SetActive(true);return copy;}
        var root=new GameObject(kind<0?"Wanderer":((EnemyKind)kind).ToString());root.transform.SetParent(parent,false);
        bool hero=kind<0,heavy=kind==3,boss=kind==4,archer=kind==2,assassin=kind==1;
    // 可調數值【敵人體型】Boss 寬×1.45、重型×1.3、刺客×.78；下方 Vector3 均為局部模型位置／尺寸（公尺）。
        float width=boss?1.45f:heavy?1.3f:assassin?.78f:1;
        Color cloth=hero?Red:assassin?new Color(.25f,.12f,.28f):archer?new Color(.27f,.21f,.13f):boss?Red:Leather;
        Color armor=hero?Iron:boss?new Color(.19f,.19f,.23f):heavy?new Color(.3f,.24f,.23f):Iron;
        var body=new Geometry();body.Ellipsoid(new Vector3(0,1.27f,0),new Vector3(.72f*width,.83f,.4f),armor);
        body.Box(new Vector3(0,.97f,.025f),new Vector3(.63f*width,.15f,.43f),Leather);body.Box(new Vector3(0,.98f,.255f),new Vector3(.15f,.13f,.04f),Gold);
        for(int i=-1;i<=1;i++)body.Box(new Vector3(i*.22f*width,.75f,.02f),new Vector3(.23f,.42f,.39f),cloth,Quaternion.Euler(i*6,0,i*9));
        body.Ellipsoid(new Vector3(0,1.66f,0),new Vector3(.39f,.25f,.43f),cloth);
        body.Box(new Vector3(0,1.33f,.225f),new Vector3(.095f,.7f,.07f),Leather,Quaternion.Euler(0,0,-30));
        for(int i=-1;i<=1;i+=2){body.Ellipsoid(new Vector3(i*.4f*width,1.47f,0),new Vector3(heavy||boss?.5f:.33f,.29f,.5f),armor);
            body.Taper(new Vector3(i*.4f*width,1.51f,0),new Vector3(i*.63f*width,1.83f,0),boss||heavy?.13f:.04f,0,Silver);}
        if(hero||boss||archer||assassin){for(int i=0;i<5;i++){float x=(i-2)*.17f;var a=new Vector3(x,1.6f,-.16f);var b=new Vector3(x+.2f,1.6f,-.16f);var c=new Vector3(x+.3f,.48f+(i%2)*.14f,-.43f);var d=new Vector3(x-.1f,.44f,-.48f);
            body.Quad(a,b,c,d,cloth);body.Quad(d,c,b,a,cloth*.7f);}}
        body.Object("Torso & mantle",root.transform,Material);
        var head=new Geometry();head.Ellipsoid(new Vector3(0,1.91f,.015f),new Vector3(.42f,.46f,.4f),hero?Skin:armor);
        if(hero){head.Ellipsoid(new Vector3(0,2.08f,-.02f),new Vector3(.48f,.26f,.44f),new Color(.075f,.08f,.095f));
            for(int i=0;i<9;i++){float a=i*2.4f;var p=new Vector3(Mathf.Cos(a)*.16f,2.09f,Mathf.Sin(a)*.17f);head.Taper(p,p+new Vector3(Mathf.Cos(a)*.13f,.11f,Mathf.Sin(a)*.1f),.1f,0,Iron*.5f);}}
        else if(assassin||archer){head.Ellipsoid(new Vector3(0,1.98f,-.06f),new Vector3(.57f,.57f,.48f),cloth);head.Box(new Vector3(0,1.94f,.218f),new Vector3(.32f,.22f,.02f),Color.black);}
        else {head.Box(new Vector3(0,1.9f,.22f),new Vector3(.35f,.08f,.04f),Color.black);head.Box(new Vector3(0,1.97f,.22f),new Vector3(.035f,.3f,.06f),Gold);
            head.Taper(new Vector3(0,2.08f,0),new Vector3(0,2.32f,-.1f),.16f,0,armor);}
        for(int i=-1;i<=1;i+=2)head.Box(new Vector3(i*.09f,1.94f,.245f),new Vector3(.057f,.025f,.012f),hero?Color.black:new Color(1,.22f,.04f));
        if(boss)for(int i=-1;i<=1;i+=2)head.Taper(new Vector3(i*.2f,2.07f,0),new Vector3(i*.52f,2.45f,-.1f),.11f,0,Gold);
        head.Object("Head",root.transform,Material);
        for(int side=-1;side<=1;side+=2){
            var leg=new GameObject(side<0?"Left leg":"Right leg");leg.transform.SetParent(root.transform,false);leg.transform.localPosition=new Vector3(side*.19f*width,.93f,0);
            var g=new Geometry();g.Taper(Vector3.zero,new Vector3(0,-.72f,.03f),.145f*width,.09f,Leather);g.Ellipsoid(new Vector3(0,-.36f,.08f),new Vector3(.24f*width,.3f,.19f),armor);g.Box(new Vector3(0,-.77f,.11f),new Vector3(.25f*width,.2f,.4f),armor);g.Object("Greave",leg.transform,Material);
            var arm=new GameObject(side<0?"Left arm":"Right arm");arm.transform.SetParent(root.transform,false);arm.transform.localPosition=new Vector3(side*.42f*width,1.42f,0);
            g=new Geometry();g.Taper(Vector3.zero,new Vector3(side*.05f,-.5f,.13f),.12f*width,.085f,Leather);g.Ellipsoid(new Vector3(side*.04f,-.32f,.1f),new Vector3(.23f*width,.32f,.23f),armor);g.Ellipsoid(new Vector3(side*.05f,-.55f,.14f),new Vector3(.18f,.19f,.19f),hero?Leather:armor);
            if(side>0){var grip=new Vector3(.055f,-.54f,.16f);g.Taper(grip,grip+new Vector3(0,0,.36f),.055f,.055f,Leather);g.Box(grip+new Vector3(0,0,.3f),new Vector3(.4f,.06f,.065f),Gold);
                if(heavy||boss){g.Taper(grip,grip+new Vector3(0,0,1.38f),.05f,.05f,Leather);g.Ellipsoid(grip+new Vector3(0,0,1.12f),new Vector3(boss?1.2f:.7f,.18f,.55f),Silver);g.Taper(grip+new Vector3(-.38f,0,1.1f),grip+new Vector3(-.58f,0,1.36f),.17f,0,Silver);}
                else if(archer){g.Box(grip+new Vector3(0,0,.42f),new Vector3(.085f,.09f,.85f),Leather);g.Taper(grip+new Vector3(-.45f,0,.4f),grip+new Vector3(.45f,0,.4f),.045f,.045f,Gold);}
    // 可調數值【刀長／刀寬】刺客／雙刀 .7m、重劍 1.55m、玩家長劍 1.47m、衛兵 1.05m；只改外觀，攻擊距離另改 weaponRanges。
                else g.Blade(grip+new Vector3(0,0,.32f),assassin||kind==-2?.7f:kind==-3?1.55f:kind==-1?1.47f:1.05f,assassin||kind==-2?.07f:kind==-3?.17f:.1f,Silver);
            } else if(kind==0){g.Ellipsoid(new Vector3(-.12f,-.34f,.25f),new Vector3(.58f,.68f,.13f),Gold);g.Ellipsoid(new Vector3(-.12f,-.34f,.33f),new Vector3(.47f,.55f,.09f),Leather);g.Ellipsoid(new Vector3(-.12f,-.34f,.39f),new Vector3(.14f,.14f,.13f),Silver);}
            else if(assassin||kind==-2)g.Blade(new Vector3(-.08f,-.55f,.2f),.65f,.065f,Silver);
            g.Object("Armament",arm.transform,Material);
        }
    // 可調數值【整體體型】Boss ×1.65、重型 XYZ×(1.1,1.18,1.1)、刺客×.9；需配合 BodyRadius 調碰撞。
        if(boss)root.transform.localScale=Vector3.one*1.65f;else if(heavy)root.transform.localScale=new Vector3(1.1f,1.18f,1.1f);else if(assassin)root.transform.localScale=Vector3.one*.9f;
        var rig=root.AddComponent<ActorRig>();rig.Bind();
        template=Object.Instantiate(root);template.name="Rig template "+kind;template.SetActive(false);Object.DontDestroyOnLoad(template);templates[kind]=template;return root;
    }
}
public class ActorRig:MonoBehaviour {
    Transform leftLeg,rightLeg,leftArm,rightArm; float swing,phase; Vector3 baseScale;
    public void Bind(){leftLeg=transform.Find("Left leg");rightLeg=transform.Find("Right leg");leftArm=transform.Find("Left arm");rightArm=transform.Find("Right arm");baseScale=transform.localScale;}
    Renderer[] bodyRenderers;Material ghostMaterial;MaterialPropertyBlock ghostTint;bool ghostActive;
    // 可調數值【無敵閃爍】透明度 .22～.72、正弦角速度 25 rad/s（約每秒4次）、冷藍色(.8,.93,1)；僅角色材質。
    public void RecoveryFlash(bool active,float elapsed){
        if(bodyRenderers==null){bodyRenderers=GetComponentsInChildren<MeshRenderer>();ghostMaterial=new Material(Resources.Load<Shader>("IronGhost"));ghostTint=new MaterialPropertyBlock();}
        if(active!=ghostActive){foreach(var body in bodyRenderers)body.sharedMaterial=active?ghostMaterial:Art.Material;ghostActive=active;}
        if(active){ghostTint.SetColor("_Color",new Color(.8f,.93f,1,Mathf.Lerp(.22f,.72f,(Mathf.Sin(elapsed*25)+1)*.5f)));foreach(var body in bodyRenderers)body.SetPropertyBlock(ghostTint);}
        else foreach(var body in bodyRenderers)body.SetPropertyBlock(null);
    }
    public void Leap(float height){transform.localPosition=Vector3.up*height;}
    void OnDestroy(){if(ghostMaterial!=null)Destroy(ghostMaterial);}
    // 可調數值【倒地動畫】前12%倒下、中間至65%躺地、最後35%起身；後仰85°、墊高 .14m。
    public void Knockdown(float progress){float tilt=progress<.12f?Mathf.SmoothStep(0,1,progress/.12f):progress<.65f?1:1-Mathf.SmoothStep(0,1,(progress-.65f)/.35f);transform.localRotation=Quaternion.Euler(-85*tilt,0,0);transform.localPosition=Vector3.up*(.14f*tilt);}
    public void Strike(){swing=1;}
    // 可調數值【走路／揮刀動畫】步態頻率 10、腿擺角32°、揮刀衰減4.5；手臂預備-100°、揮刀65°及側擺55°；這些角度不影響命中。
    public void Animate(float speed,bool windup,bool charge,float dt){if(leftLeg==null)Bind();phase+=dt*(speed>0?10:1);swing=Mathf.Max(0,swing-dt*4.5f);
        float gait=Mathf.Sin(phase)*Mathf.Min(speed,1)*32;leftLeg.localRotation=Quaternion.Euler(gait,0,0);rightLeg.localRotation=Quaternion.Euler(-gait,0,0);
        rightArm.localRotation=Quaternion.Euler(charge||windup?-100:-gait*.6f-swing*65,0,swing*55);leftArm.localRotation=Quaternion.Euler(charge?-50:gait*.6f,0,-swing*20);
        transform.localScale=baseScale;}
}
}
