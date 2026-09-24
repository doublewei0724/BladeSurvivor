using System;
using UnityEngine;
namespace BladeSurvivor {
public enum GestureKind { Tap,Dodge,Swipe,ChargeStart,ChargeAim,ChargeRelease,Cancel }
public sealed class GestureResolver {
    // 可調數值【純手勢預設】雙擊 .24s、長按 .5s、滑動 72px；實際遊戲由 GestureInput 每幀以 GameBalance／螢幕比例覆蓋。
    public float DoubleTapWindow=.24f,HoldThreshold=.5f,SwipeThreshold=72;
    public Action<GestureKind,Vector2,Vector2> OnGesture;
    bool held,blocked,charging;Vector2 start,pendingPosition;float downAt,lastTap=-10,pendingAt=-1;
    public void Begin(Vector2 point,float time,bool overUI){held=true;blocked=overUI;charging=false;start=point;downAt=time;}
    public void Tick(Vector2 point,float time){
        if(held&&!blocked&&!charging&&time-downAt>=HoldThreshold){charging=true;pendingAt=-1;OnGesture?.Invoke(GestureKind.ChargeStart,start,point);}
        if(held&&!blocked&&charging)OnGesture?.Invoke(GestureKind.ChargeAim,start,point);
        if(pendingAt>=0&&time>=pendingAt){pendingAt=-1;OnGesture?.Invoke(GestureKind.Tap,pendingPosition,pendingPosition);}
    }
    public void End(Vector2 point,float time){if(!held)return;held=false;if(blocked)return;
        if(charging){OnGesture?.Invoke(GestureKind.ChargeAim,start,point);charging=false;OnGesture?.Invoke(GestureKind.ChargeRelease,start,point);return;}
        if(Vector2.Distance(start,point)>=SwipeThreshold){pendingAt=-1;lastTap=-10;OnGesture?.Invoke(GestureKind.Swipe,start,point);return;}
        if(time-lastTap<=DoubleTapWindow){pendingAt=-1;lastTap=-10;OnGesture?.Invoke(GestureKind.Dodge,start,point);}
        else {lastTap=time;pendingPosition=point;pendingAt=time+DoubleTapWindow;}
    }
    public void Cancel(){held=blocked=charging=false;pendingAt=-1;lastTap=-10;OnGesture?.Invoke(GestureKind.Cancel,start,start);}
}
}
