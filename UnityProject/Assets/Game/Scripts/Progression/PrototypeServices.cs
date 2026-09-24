namespace BladeSurvivor {
public interface IRewardedAdService { bool IsAvailable {get;} float Duration {get;} bool TryGrantRevive(BattleController battle); }
public interface IIAPService { bool IsMock {get;} void GrantPrototypeSupply(ProgressionService progression); }
public sealed class MockRewardedAdService:IRewardedAdService {
    // 可調數值【模擬廣告】等待 3 秒；只供第一次死亡復活，不是真實廣告 SDK。
    public bool IsAvailable=>true;public float Duration=>3;
    public bool TryGrantRevive(BattleController battle)=>battle.AwaitingRevive&&battle.Deaths==1&&battle.Revive();
}
public sealed class MockIAPService:IIAPService {
    public bool IsMock=>true;
    // 可調數值【免費測試補給】金幣 +2000、勾玉 +30、四種強化石各 +2；會寫入存檔。
    public void GrantPrototypeSupply(ProgressionService p){p.Save.gold+=2000;p.Save.jade+=30;for(int i=0;i<4;i++)p.Save.stones[i]+=2;p.Persist();}
}
[System.Serializable] public class EndlessConfig {
    // 可調數值【極限模式預留】每層 50 擊殺／5 波、小 Boss 每 5 層、大 Boss 每 10 層；目前玩法未啟用。
    public int killsPerLayer=50,wavesPerLayer=5,bossEvery=5,bigBossEvery=10;
    // 可調數值【極限模式預留成長】每層生命 +8%、傷害 +6%；未啟用。
    public float hpGrowth=.08f,damageGrowth=.06f;
    public bool dropsPermanentRewards=false;
}
}
