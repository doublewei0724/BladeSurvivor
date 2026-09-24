using UnityEngine;
namespace BladeSurvivor {
public class GestureInput:MonoBehaviour {
    public GameRoot Root;readonly GestureResolver resolver=new GestureResolver();int finger=-1;Vector2 pointer;
    public void ResetGesture(){resolver.Cancel();finger=-1;if(Root?.Battle?.Player!=null)Root.Battle.Player.CancelCharge();}
    void Start(){resolver.OnGesture=Handle;}
    void Update(){var battle=Root.Battle;if(Root.Screen!=GameScreen.Battle||!battle.Running||battle.AwaitingRevive){ResetGesture();return;}
        if(Input.GetKeyDown(KeyCode.Escape)){battle.Pause(!battle.Paused);ResetGesture();}if(battle.Paused)return;
        resolver.DoubleTapWindow=Root.Balance.doubleTapWindow;resolver.HoldThreshold=Root.Balance.chargeStart*battle.Player.Stats.chargeFull/Root.Balance.chargeFull;resolver.SwipeThreshold=Mathf.Min(UnityEngine.Screen.width,UnityEngine.Screen.height)*Root.Balance.swipeScreenFraction;
        var player=battle.Player;float x=Input.GetAxisRaw("Horizontal"),z=Input.GetAxisRaw("Vertical");
        if(Mathf.Abs(x)+Mathf.Abs(z)>.1f)player.SetDirection(new Vector3(x,0,z));
        else if(Input.GetKeyUp(KeyCode.W)||Input.GetKeyUp(KeyCode.A)||Input.GetKeyUp(KeyCode.S)||Input.GetKeyUp(KeyCode.D)||Input.GetKeyUp(KeyCode.UpArrow)||Input.GetKeyUp(KeyCode.DownArrow)||Input.GetKeyUp(KeyCode.LeftArrow)||Input.GetKeyUp(KeyCode.RightArrow))player.MoveDirection=Vector3.zero;
        if(Input.GetKeyDown(KeyCode.Space)||Input.GetMouseButtonDown(1))player.Dodge(player.MoveDirection.sqrMagnitude>.01f?player.MoveDirection:player.transform.forward);
        if(Input.GetKeyDown(KeyCode.LeftShift)||Input.GetKeyDown(KeyCode.RightShift))player.Swipe(player.MoveDirection.sqrMagnitude>.01f?player.MoveDirection:player.transform.forward);
        if(Input.GetKeyDown(KeyCode.E))player.StartCharge();if(Input.GetKey(KeyCode.E))player.AimCharge(Ground(Input.mousePosition)-player.transform.position);if(Input.GetKeyUp(KeyCode.E)){player.AimCharge(Ground(Input.mousePosition)-player.transform.position);player.ReleaseCharge();}
        if(Input.GetKeyDown(KeyCode.Alpha1))player.Skill(0);if(Input.GetKeyDown(KeyCode.Alpha2))player.Skill(1);if(Input.GetKeyDown(KeyCode.Alpha3))player.Skill(2);if(Input.GetKeyDown(KeyCode.Alpha4))player.Skill(3);
        if(Input.touchCount>0){foreach(var touch in Input.touches){if(touch.phase==TouchPhase.Began&&finger<0){finger=touch.fingerId;resolver.Begin(touch.position,Time.unscaledTime,Root.UI.BlocksGesture(touch.position));}
                if(touch.fingerId!=finger)continue;pointer=touch.position;if(touch.phase==TouchPhase.Ended){resolver.End(touch.position,Time.unscaledTime);finger=-1;}else if(touch.phase==TouchPhase.Canceled)ResetGesture();}}
        else {pointer=Input.mousePosition;if(Input.GetMouseButtonDown(0))resolver.Begin(pointer,Time.unscaledTime,Root.UI.BlocksGesture(pointer));if(Input.GetMouseButtonUp(0))resolver.End(pointer,Time.unscaledTime);}
        resolver.Tick(pointer,Time.unscaledTime);
    }
    void Handle(GestureKind kind,Vector2 start,Vector2 end){var p=Root?.Battle?.Player;if(p==null)return;
        switch(kind){case GestureKind.Tap:p.SetDirection(Ground(end)-p.transform.position);break;case GestureKind.Dodge:p.Dodge(Ground(end)-p.transform.position);break;
            case GestureKind.Swipe:p.Swipe(Ground(end)-Ground(start));break;case GestureKind.ChargeStart:p.StartCharge();if(p.Charging)p.ChargeTime=resolver.HoldThreshold;break;case GestureKind.ChargeAim:p.AimCharge(Ground(end)-Ground(start));break;case GestureKind.ChargeRelease:p.ReleaseCharge();break;case GestureKind.Cancel:p.CancelCharge();break;}}
    // 數值說明【地面投影】地面高度為 Y=0；調整場地高度時須同步 Plane 與戰鬥移動高度。
    Vector3 Ground(Vector2 p){var ray=Root.BattleCamera.GetComponent<Camera>().ScreenPointToRay(p);new Plane(Vector3.up,Vector3.zero).Raycast(ray,out float distance);return ray.GetPoint(distance);}
}
}
