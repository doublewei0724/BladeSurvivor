# 中文數值調整索引

## 先看這裡

1. 停止 Unity Play Mode。
2. 在 Project 選 `Assets/Game/Resources/GameBalance.asset`，在 Inspector 調整；欄位滑鼠停留可看中文提示。
3. 或搜尋下表英文欄位，在同一 `.asset` 修改已序列化的數值。不要在 YAML 插入 C# 註解。
4. `.cs` 初始化值只負責新資產預設，**已有 `.asset` 的值優先**。缺少的新欄位會先用程式初始化值；儲存資產後寫入 `.asset`。
5. 改完重新 Play；獨立 Mac 遊戲須再執行 `Blade Survivor > Build macOS Playtest`。

在程式碼搜尋 `可調數值` 或中文關鍵字即可定位。運行中 HP、冷卻倒數、連擊段數、存檔等級是狀態，不是平衡設定。舊版未使用參數已明確標示，不必試改它們。

## GameBalance 欄位

原始碼：`UnityProject/Assets/Game/Scripts/Data/GameBalance.cs`。
陣列長度／索引有資料結構限制；不要只增加上限而不擴充成本表、UI 及存檔結構。

| 搜尋欄位 | 中文用途／單位 |
|---|---|
| `baseHP` | 玩家初始生命上限（HP） |
| `baseSP` | 玩家初始精力上限（SP） |
| `moveSpeed` | 正常移動速度（公尺／秒） |
| `critChance` | 基礎暴擊機率，0.05 代表 5% |
| `critMultiplier` | 暴擊傷害倍率，1.5 代表 150% |
| `scanRange` | 自動尋敵半徑（公尺） |
| `scanInterval` | 重新尋敵間隔（秒） |
| `weaponMultipliers` | 武器傷害倍率；索引依序為長劍、雙刀、重劍 |
| `attackIntervals` | 普攻起手間隔（秒）；依序長劍、雙刀、重劍；連招接續沿用此間隔 |
| `weaponRanges` | 普攻半徑（公尺）；依序長劍、雙刀、重劍 |
| `weaponArcs` | 普攻總角度（度）；120 代表左右各 60 度；依序長劍、雙刀、重劍 |
| `weaponKnockbacks` | 普攻基礎擊退（公尺）；依序長劍、雙刀、重劍；實際位移受抗性與衰減影響 |
| `comboJumpHeight` | 連擊第四下最高跳躍高度（公尺） |
| `comboJumpDistance` | 連擊第四下向前位移（公尺） |
| `comboJumpDuration` | 連擊第四下起跳至落地時間（秒）；全程無敵 |
| `chargeLength` | 蓄力矩形向前長度（公尺），三段共用 |
| `chargeWidth` | 蓄力矩形總寬度（公尺），不是單側寬度 |
| `enemyKnockdownDuration` | 敵人被擊倒至站起時間（秒），蓄力／突進／第四擊共用 |
| `knockdownDuration` | 玩家受擊後退到站起總時間（秒），期間完全無敵 |
| `knockdownDistance` | 玩家有效受擊時向後位移（公尺） |
| `recoveryRadius` | 玩家倒地起身期間敵人隔離半徑（公尺），另加敵人身體半徑 |
| `attackWindup` | 普通攻擊起手至傷害判定的前搖（秒） |
| `attackActive` | 舊版預留命中窗（秒）；目前普攻採前搖結束單次判定，調此值不生效 |
| `attackMoveMultiplier` | 普攻動作期間保留的移速比例；0.45 代表剩 45%，不是減少 45% |
| `doubleTapWindow` | 雙擊閃避辨識時限（秒） |
| `swipeScreenFraction` | 快速滑動最小距離占螢幕短邊比例；0.08 代表 8% |
| `dodgeSP` | 閃避基本精力消耗，裝備詞條可降低 |
| `dodgeDuration` | 閃避移動時間（秒），整段無敵 |
| `dodgeIFrameStart` | 舊版局部閃避無敵起點；目前整段閃避無敵，調此值不生效 |
| `dodgeIFrameEnd` | 舊版局部閃避無敵終點；目前整段閃避無敵，調此值不生效 |
| `dodgeCooldown` | 閃避結束後額外冷卻（秒） |
| `dodgeDistance` | 雙擊閃避總移動距離（公尺） |
| `swipeSP` | 滑動突進基本精力消耗 |
| `swipeCooldown` | 滑動突進起手後冷卻（秒） |
| `swipeDistance` | 滑動突進總移動距離（公尺） |
| `swipeWidth` | 滑動突進傷害路徑總寬度（公尺），另計敵人命中半徑 |
| `swipeMultiplier` | 滑動突進傷害倍率，1.25 代表普通傷害的 125% |
| `chargeStart` | 進入第一段蓄力所需長按時間（秒）；蓄力加速詞條會按比例縮短 |
| `chargeSecond` | 第二段蓄力時間門檻（秒） |
| `chargeFull` | 第三段滿蓄力時間門檻（秒）；滿蓄力不會自動出招 |
| `chargeMoveMultiplier` | 舊版蓄力移速比例；目前蓄力站定，調此值不生效 |
| `chargeMultipliers` | 三段蓄力傷害倍率，依序第一／第二／滿蓄力 |
| `chargeArcs` | 舊版三段蓄力角度；目前用矩形 chargeLength／chargeWidth，調此陣列不生效 |
| `chargeRanges` | 舊版三段蓄力半徑；目前用矩形 chargeLength／chargeWidth，調此陣列不生效 |
| `chargeKnockbacks` | 三段蓄力基礎擊退；現行擊倒會清除擊退速度，主要用 enemyKnockdownDuration 調倒地 |
| `waveCount` | 每關波數；需與 waves 陣列長度一致，改動亦需同步結算／UI 的十波設定 |
| `enemiesPerWave` | 每波敵人數；需與每一筆 waves 四種敵人數量總和一致 |
| `waveInterval` | 兩波出生的時間間隔（秒），不等待上一波清完 |
| `starTime` | 取得第二顆星的通關時間上限（秒） |
| `starHits` | 第三顆星容許的有效受擊次數上限 |
| `arenaHalfSize` | 地圖 X／Z 半尺寸（公尺），80／80 即完整 160 × 160 |
| `spRegen` | 每秒精力回復量 |
| `spRegenDelay` | 消耗精力後暫停自然回復的時間（秒） |
| `hpPickupChance` | 普通敵人掉落回血物機率；0.05 代表 5% |
| `spPickupChance` | 普通敵人掉落回精物機率 |
| `hpRestore` | 回血物回復最大生命比例 |
| `spRestore` | 回精物回復最大精力比例 |
| `pickupRadius` | 一般掉落物開始吸附的距離（公尺） |
| `valuablePickupRadius` | 大型強化石與勾玉開始吸附距離（公尺） |
| `reviveInvulnerability` | 復活後額外無敵時間（秒） |
| `stoneChances` | 普通敵人四種強化石掉落機率，依序 S／M／L／XL |
| `bossStoneChances` | Boss 四種強化石抽選權重，依序 S／M／L／XL；總和應為 1 |
| `jadeChance` | 普通敵人掉落勾玉機率 |
| `bossJadeChance` | Boss 掉落勾玉機率 |
| `maxLevel` | 玩家最高等級，曲線與裝備階級需一起檢查 |
| `maxUpgrade` | 永久能力強化等級上限，需匹配 upgradeCosts 長度 |
| `maxEnhance` | 武器強化等級上限；UI 顯示上限也需同步 |
| `enhanceAttempts` | 每把武器最多投入強化石次數；UI 按鈕上限也需同步 |
| `enhanceCrit` | 強化石能量翻倍機率 |
| `enhanceBonus` | 每級武器強化增加的傷害比例 |
| `permanentAttack` | 每級永久攻擊提升比例 |
| `permanentDefense` | 每級永久防禦的傷害減免比例 |
| `stoneMin` | 強化石最小能量，依序 S／M／L／XL |
| `stoneMax` | 強化石最大能量（包含此值），依序 S／M／L／XL |
| `upgradeCosts` | 永久能力每一級購買金幣成本，索引 0 為升第一級 |
| `skillUpgradeCosts` | 技能由 Lv1 升至 Lv2、3、4、5 的金幣成本 |
| `tierLevels` | 裝備階級及敵人曲線的等級節點，與各六段數值陣列配對 |
| `shopPrices` | 各階金色裝備的勾玉售價 |
| `swordMin` | 各等級階段的基礎長劍最低傷害 |
| `swordMax` | 各等級階段的基礎長劍最高傷害 |
| `suitDEF` | 各等級階段的套服基礎防禦 |
| `rarityMultipliers` | 白／綠／藍／紫／金裝備的品質倍率 |
| `normalHP` | 一般衛兵在 tierLevels 六個等級節點的生命值 |
| `assassinHP` | 刺客在六個等級節點的生命值 |
| `archerHP` | 弩手在六個等級節點的生命值 |
| `heavyHP` | 重型敵人在六個等級節點的生命值 |
| `bossHP` | Boss 在六個等級節點的生命值 |
| `normalDamage` | 一般衛兵在六個等級節點的傷害 |
| `assassinDamage` | 刺客在六個等級節點的傷害 |
| `archerDamage` | 弩手在六個等級節點的傷害 |
| `heavyDamage` | 重型敵人在六個等級節點的傷害 |
| `bossDamage` | Boss 在六個等級節點的傷害 |
| `enemies` | 敵人速度、射程、前搖、後搖、冷卻、擊退承受倍率；依 EnemyKind 排序 |
| `skills` | 七種技能的解鎖、售價、傷害倍率、範圍、冷卻與持續時間 |
| `stages` | 關卡名稱、建議等級、Boss 標記與劇情 |
| `endless` | 第二階段極限模式預留參數，目前未啟用玩法 |
| `waves` | 每波一般／刺客／弩手／重型數量；需與 enemiesPerWave 和 waveCount 配合 |

## 分散在程式內的外觀、行為與公式數值

以下數值直接改對應 `.cs` 的原始數字，保存後讓 Unity 重新編譯；註解僅為中文說明。
所有模型局部座標以公尺計、旋轉以度計；更改模型外觀不會自動更改命中半徑。

### ProjectSetup.cs

- [可調數值【視窗尺寸】預設 1600×900，允許縮放；UI 基準座標需看 GameUI 的 W／H。](UnityProject/Assets/Game/Editor/ProjectSetup.cs#L19)
- [可調數值【背景貼圖匯入】最大邊長 2048、關閉 mipmap；改成更大會增加記憶體。](UnityProject/Assets/Game/Editor/ProjectSetup.cs#L23)

### BattleController.cs

- [可調數值【預熱物件池】衛兵 75、其餘普通類各 30、Boss 1；拾取物 64、投射物 40；不足會擴充。](UnityProject/Assets/Game/Scripts/Combat/BattleController.cs#L27)
- [可調數值【Boss 替換位置】最後一波第 10 位（索引 9）換 Boss；變更每波數量時需同步這個位置與公告的第 10 波。](UnityProject/Assets/Game/Scripts/Combat/BattleController.cs#L38)
- [可調數值【畫面外出生】最多嘗試 40 次；均勻分散角 137.5°，初始間隔 36°、波次偏轉 47°。](UnityProject/Assets/Game/Scripts/Combat/BattleController.cs#L53)
- [可調數值【出生離畫面距離】.68 為螢幕中心到外框距離，.5 剛好螢幕邊；出生需離地圖邊 1m。](UnityProject/Assets/Game/Scripts/Combat/BattleController.cs#L56)
- [可調數值【出生備援】極小自訂地圖找不到外框時，取玩家前方 20m 並限制在地圖內 1m。](UnityProject/Assets/Game/Scripts/Combat/BattleController.cs#L63)
- [可調數值【防重疊效能】預配 256 敵人位置，可自動擴充；最多迭代 24 次。](UnityProject/Assets/Game/Scripts/Combat/BattleController.cs#L77)
- [可調數值【敵人間距】身體半徑和之外保留 .06m；每次雙方各推一半，加 .0005m 誤差補償。](UnityProject/Assets/Game/Scripts/Combat/BattleController.cs#L89)
- [可調數值【間距求解精度】最壞重疊小於 .002m 提前停止，避免不必要迭代。](UnityProject/Assets/Game/Scripts/Combat/BattleController.cs#L98)
- [可調數值【重型掉石加成】一般強化石機率 ×1.5；基礎掉率在 GameBalance，Boss 石頭採累積機率抽一種。](UnityProject/Assets/Game/Scripts/Combat/BattleController.cs#L104)
- [可調數值【掉落外觀】尺寸 .3×.42×.3m，落點隨機偏移 ±.3m、高 .32m；幾何 5 邊 3 環。](UnityProject/Assets/Game/Scripts/Combat/BattleController.cs#L111)
- [可調數值【拾取吸附】吸附速度 9m/s、拾取距離 .65m；上下漂浮振幅 .075m、每秒旋轉 90°。](UnityProject/Assets/Game/Scripts/Combat/BattleController.cs#L117)
- [可調數值【投射物外觀】箭長 .7m、寬 .06m；玩家劍氣模型縮放 6／1／1.8；與命中寬度分開。](UnityProject/Assets/Game/Scripts/Combat/BattleController.cs#L125)
- [可調數值【投射物判定】玩家劍氣命中額外半寬 .5m、擊退 .3m；敵人箭對玩家命中半徑 .45m。](UnityProject/Assets/Game/Scripts/Combat/BattleController.cs#L129)
- [可調數值【死亡次數】第三次死亡失敗；第二次復活花費勾玉，第一次走模擬廣告。](UnityProject/Assets/Game/Scripts/Combat/BattleController.cs#L135)

### EnemyActor.cs

- [可調數值【敌人防重疊身體半徑】Boss 1.05m、重型 .65m、其餘 .46m；不是武器命中半徑。](UnityProject/Assets/Game/Scripts/Combat/EnemyActor.cs#L7)
- [可調數值【敵人命中半徑】Boss .85m、重型 .5m、其餘 .32m；生命傷害讀 GameBalance 等級曲線。](UnityProject/Assets/Game/Scripts/Combat/EnemyActor.cs#L10)
- [可調數值【死亡縮小】.45 秒縮小並回收；不影響掉落。](UnityProject/Assets/Game/Scripts/Combat/EnemyActor.cs#L14)
- [可調數值【敵人擊退衰減】速度平方低於 .01 停止；衰減速率 12，配合 Receive 的速度倍率 10 決定位移。](UnityProject/Assets/Game/Scripts/Combat/EnemyActor.cs#L18)
- [可調數值【敵人轉向／遠程站位】轉向 350 度/秒；弩手理想距離 4.7m，小於 3.7m 後退。](UnityProject/Assets/Game/Scripts/Combat/EnemyActor.cs#L22)
- [可調數值【Boss 三招前搖】扇形 .8s、震地 1.1s、衝撞 .7s。預警半徑 3.6／4.3／6m，窄招 20°、一般 110°。](UnityProject/Assets/Game/Scripts/Combat/EnemyActor.cs#L28)
- [可調數值【追擊避讓】每 .14 秒更新；半徑和額外加 .08m，避讓向量權重 2；最終防重疊另在 BattleController。](UnityProject/Assets/Game/Scripts/Combat/EnemyActor.cs#L33)
- [可調數值【敵人招式】弩箭速度 9m/s、射程 12m、起點高度 .8m；Boss 衝撞持續 .65s。](UnityProject/Assets/Game/Scripts/Combat/EnemyActor.cs#L37)
- [可調數值【敵人近戰判定】普通射程額外 +.35m；Boss 扇形 3.6m／120°，震地 4.3m／360°、傷害 ×1.3；玩家半徑 .3m。](UnityProject/Assets/Game/Scripts/Combat/EnemyActor.cs#L41)
- [可調數值【Boss 衝撞】速度 9m/s、碰撞補償 .7m、傷害 ×1.2；衝撞後搖 .7s。](UnityProject/Assets/Game/Scripts/Combat/EnemyActor.cs#L46)
- [可調數值【Boss 狂暴】HP 低於 50% 時，攻擊冷卻乘 .85（加快 15%），見上方 Recovery 分支。](UnityProject/Assets/Game/Scripts/Combat/EnemyActor.cs#L52)
- [可調數值【敵人地圖边緣】允許比玩家邊界多 .6m；Y 固定地面。](UnityProject/Assets/Game/Scripts/Combat/EnemyActor.cs#L56)
- [可調數值【擊退起速】基礎距離 × 敵人承受倍率 ×10；普通受擊僵直 .16s，重型前搖抗僵直；倒地另讀 enemyKnockdownDuration。](UnityProject/Assets/Game/Scripts/Combat/EnemyActor.cs#L64)

### PlayerFighter.cs

- [可調數值【玩家腳下標記】半徑 .55m、高度 .06m、線寬 .035m、40 段；Color 為金色 RGBA。](UnityProject/Assets/Game/Scripts/Combat/PlayerFighter.cs#L23)
- [可調數值【起身隔離圈外觀】48 段、線寬 .055m、透明度 .7；半徑讀 B.recoveryRadius。](UnityProject/Assets/Game/Scripts/Combat/PlayerFighter.cs#L25)
- [可調數值【蓄力瞄準框外觀】高度 .09m、線寬 .065m、透明度 .8；長寬讀 chargeLength／chargeWidth。](UnityProject/Assets/Game/Scripts/Combat/PlayerFighter.cs#L27)
- [可調數值【玩家後退節奏】後退在最初 .2 秒完成；总倒地時間與後退距離讀 GameBalance。](UnityProject/Assets/Game/Scripts/Combat/PlayerFighter.cs#L35)
- [可調數值【第四擊跳躍】高度／距離／時間讀 comboJump*；落地半徑 3m、全圓 360°、倍率 1、擊退 .7。](UnityProject/Assets/Game/Scripts/Combat/PlayerFighter.cs#L37)
- [可調數值【亂舞連斬】每 .4 秒一刀、角度 180°、擊退 .5；傷害範圍讀技能資料。](UnityProject/Assets/Game/Scripts/Combat/PlayerFighter.cs#L40)
- [可調數值【突進命中】以 swipeWidth 的半寬加敵人半徑判定；基礎擊退 .7，存活敵人接著倒地。](UnityProject/Assets/Game/Scripts/Combat/PlayerFighter.cs#L42)
- [可調數值【站定自動轉向】每秒最多轉 650 度；移動中朝向仍由輸入決定。](UnityProject/Assets/Game/Scripts/Combat/PlayerFighter.cs#L57)
- [可調數值【蓄力瞄準死區】方向長度平方 .04，即拖曳地面距離至少 .2m 才更新方向。](UnityProject/Assets/Game/Scripts/Combat/PlayerFighter.cs#L75)
- [可調數值【蓄力放招】抗打斷 .5 秒、普攻等待 .45 秒、鏡頭震動 .65；不是倒地的完全無敵。](UnityProject/Assets/Game/Scripts/Combat/PlayerFighter.cs#L79)
- [可調數值【突進時間／特效】目前 .24 秒走完 swipeDistance；抗打斷 .3 秒；視覺弧半徑 1.8、180°（不作傷害範圍）。](UnityProject/Assets/Game/Scripts/Combat/PlayerFighter.cs#L83)
- [可調數值【技能成長】每提升一級增加 .1（10%）效果。](UnityProject/Assets/Game/Scripts/Combat/PlayerFighter.cs#L88)
- [可調數值【技能行為】血刃／戰意持續讀技能資料；無雙斬抗打斷 .6 秒；劍氣速度 12m/s；旋風擊退 .8，其餘範圍技能 1.8。](UnityProject/Assets/Game/Scripts/Combat/PlayerFighter.cs#L90)
- [可調數值【普攻刀光】傷害倍率達 2 切金色粗刀光；壽命 .23 秒、粗細 .18／.1m；只影響外觀。](UnityProject/Assets/Game/Scripts/Combat/PlayerFighter.cs#L100)
- [可調數值【蓄力特效】前方均分 5 道半圓刀光，壽命 .3 秒、線寬 .15m；實際命中只看矩形。](UnityProject/Assets/Game/Scripts/Combat/PlayerFighter.cs#L106)
- [可調數值【血刃吸血】基礎吸血率 .05 × 技能成長；每次攻擊最大回復 HP 上限的 .05。](UnityProject/Assets/Game/Scripts/Combat/PlayerFighter.cs#L114)
- [可調數值【戰意加攻】傷害乘上 1 + .2 × 技能成長；基礎暴擊率來自衍生數值。](UnityProject/Assets/Game/Scripts/Combat/PlayerFighter.cs#L117)
- [可調數值【復活】HP／SP 回復 50%；附近 ±6m 取 20 個候選點，選距離敵人最遠的位置；搜尋敵人範圍 100m。](UnityProject/Assets/Game/Scripts/Combat/PlayerFighter.cs#L126)

### GameRoot.cs

- [可調數值【顯示品質】目標 60FPS、垂直同步 1、抗鋸齒 4 倍、陰影距離 55m；手機效能需真機驗證。](UnityProject/Assets/Game/Scripts/Core/GameRoot.cs#L11)
- [可調數值【主光】RGB(.87,.9,1)、亮度 1.25、旋轉角(48,-35,0)；下方三組 RGB 為天空／水平／地面環境光。](UnityProject/Assets/Game/Scripts/Core/GameRoot.cs#L16)

### GestureResolver.cs

- [可調數值【純手勢預設】雙擊 .24s、長按 .5s、滑動 72px；實際遊戲由 GestureInput 每幀以 GameBalance／螢幕比例覆蓋。](UnityProject/Assets/Game/Scripts/Input/GestureResolver.cs#L6)

### Geometry.cs

- [可調數值【圓形模型細度】預設 8 邊／5 環；增加可變圓滑但增加面數；size 是完整長寬高。](UnityProject/Assets/Game/Scripts/Presentation/Geometry.cs#L15)
- [可調數值【柱／角幾何】預設 6 邊；lower／upper 是底／頂半徑（公尺）。](UnityProject/Assets/Game/Scripts/Presentation/Geometry.cs#L22)
- [可調數值【刀身造型】length 是長度，width 是單側寬；刀脊高度 .055m、刀脊位置長度的 40%。](UnityProject/Assets/Game/Scripts/Presentation/Geometry.cs#L30)
- [可調數值【地面貼圖比例】UV 乘 .125，表示每 8m 重複貼圖一次；65535 是網格索引格式界線。](UnityProject/Assets/Game/Scripts/Presentation/Geometry.cs#L35)
- [可調數值【模型配色】Iron 鐵／Silver 刃／Leather 皮革／Red 披風／Gold 金飾／Skin 膚色；RGB 0～1。](UnityProject/Assets/Game/Scripts/Presentation/Geometry.cs#L41)
- [可調數值【敵人體型】Boss 寬×1.45、重型×1.3、刺客×.78；下方 Vector3 均為局部模型位置／尺寸（公尺）。](UnityProject/Assets/Game/Scripts/Presentation/Geometry.cs#L49)
- [可調數值【刀長／刀寬】刺客／雙刀 .7m、重劍 1.55m、玩家長劍 1.47m、衛兵 1.05m；只改外觀，攻擊距離另改 weaponRanges。](UnityProject/Assets/Game/Scripts/Presentation/Geometry.cs#L80)
- [可調數值【整體體型】Boss ×1.65、重型 XYZ×(1.1,1.18,1.1)、刺客×.9；需配合 BodyRadius 調碰撞。](UnityProject/Assets/Game/Scripts/Presentation/Geometry.cs#L86)
- [可調數值【無敵閃爍】透明度 .22～.72、正弦角速度 25 rad/s（約每秒4次）、冷藍色(.8,.93,1)；僅角色材質。](UnityProject/Assets/Game/Scripts/Presentation/Geometry.cs#L96)
- [可調數值【倒地動畫】前12%倒下、中間至65%躺地、最後35%起身；後仰85°、墊高 .14m。](UnityProject/Assets/Game/Scripts/Presentation/Geometry.cs#L105)
- [可調數值【走路／揮刀動畫】步態頻率 10、腿擺角32°、揮刀衰減4.5；手臂預備-100°、揮刀65°及側擺55°；這些角度不影響命中。](UnityProject/Assets/Game/Scripts/Presentation/Geometry.cs#L108)

### WorldPresentation.cs

- [可調數值【地面材質】RGB 倍率 2.3／2.1／1.9、金屬 .05、光滑 .15；貼圖路徑 CourtyardStone。](UnityProject/Assets/Game/Scripts/Presentation/WorldPresentation.cs#L11)
- [可調數值【地圖分塊】邊界外延 32m、每塊 16m；每塊 8×8 格、每格 2m，三者需同步。](UnityProject/Assets/Game/Scripts/Presentation/WorldPresentation.cs#L14)
- [可調數值【地形隨機種子】814 控制裝飾分布；73856093／19349663 是座標混合常數，不是玩法倍率。](UnityProject/Assets/Game/Scripts/Presentation/WorldPresentation.cs#L17)
- [可調數值【地面色差與道路】底亮度 .29、隨機幅度 .035；中央道路兩側各 3m；下方 Color 皆 RGB 0～1。](UnityProject/Assets/Game/Scripts/Presentation/WorldPresentation.cs#L21)
- [可調數值【碎石裝飾】每塊 5 顆；高度 .025m、寬 .7～1.7m、高 .08m、深 1.2m。](UnityProject/Assets/Game/Scripts/Presentation/WorldPresentation.cs#L29)
- [可調數值【地面紋章】每 3 塊放一個；32 道標記、半徑 3m、標記尺寸 .12×.025×.35m。](UnityProject/Assets/Game/Scripts/Presentation/WorldPresentation.cs#L34)
- [可調數值【邊界遺跡】在地圖外 2m，每 8m 一根柱；牆高 1m、厚 .7m。](UnityProject/Assets/Game/Scripts/Presentation/WorldPresentation.cs#L41)
- [可調數值【柱子造型】底座 1.2×.4×1.2m；柱身高 3m，尖頂到 3.7m，半徑 .4→.28m。](UnityProject/Assets/Game/Scripts/Presentation/WorldPresentation.cs#L51)
- [可調數值【鏡頭】俯角 42°、後退 30m、正交半高 7.8m（越小越近）、裁切 .1～150m；背景 RGB(.025,.03,.04)。](UnityProject/Assets/Game/Scripts/Presentation/WorldPresentation.cs#L56)
- [可調數值【跟隨與震動】跟隨平滑速率 12、震動每秒衰减 1.5、位移幅度 .08m。](UnityProject/Assets/Game/Scripts/Presentation/WorldPresentation.cs#L60)
- [可調數值【刀光池】96 個特效、每條 25 個點、預設線寬 .085m、圓頭 2 段。](UnityProject/Assets/Game/Scripts/Presentation/WorldPresentation.cs#L68)
- [可調數值【刀光外觀】預設 .28s、線寬 .09m、離地 .12m、半徑從 75% 展開；Ring 預設 .4s、寬 .06m。](UnityProject/Assets/Game/Scripts/Presentation/WorldPresentation.cs#L71)
- [可調數值【傷害跳字】最多約 90 筆、高度 2m、存活 .8s、每秒上飄 .8m。](UnityProject/Assets/Game/Scripts/Presentation/WorldPresentation.cs#L78)
- [可調數值【音效合成】劍 .16s／350→50Hz、受擊 .15s／110→30Hz、拾取 .2s／650→1100Hz、勝利 .8s／260→520Hz。](UnityProject/Assets/Game/Scripts/Presentation/WorldPresentation.cs#L86)
- [可調數值【背景音】44100Hz 取樣、8 秒循環；55／82.5／110Hz 三音，振幅 .018／.012／.008。](UnityProject/Assets/Game/Scripts/Presentation/WorldPresentation.cs#L88)
- [可調數值【音效質感】主音振幅 .3、雜訊 .25、包絡平方衰減、總增益 .4；44100 為取樣率。](UnityProject/Assets/Game/Scripts/Presentation/WorldPresentation.cs#L90)
- [可調數值【音高隨機】每次 .9～1.1 倍；kind 0劍／1受擊／2拾取／3勝利是事件編號，不是音量。](UnityProject/Assets/Game/Scripts/Presentation/WorldPresentation.cs#L92)

### ProgressionService.cs

- [可調數值【新存檔】5 種永久能力、4 種石頭、7 技能；初始第一技 Lv1、4 技能槽中只裝第一技、30 關星等；改長度需遷移存檔及 UI。](UnityProject/Assets/Game/Scripts/Progression/ProgressionService.cs#L17)
- [可調數值【預設音量】.65（65%），已有存檔以存檔 volume 為準。](UnityProject/Assets/Game/Scripts/Progression/ProgressionService.cs#L21)
- [可調數值【傷害公式】對 Boss 詞條 ×1.12；最終傷害至少 1，四捨五入一次。強化與永久攻擊增幅讀 GameBalance。](UnityProject/Assets/Game/Scripts/Progression/ProgressionService.cs#L33)
- [可調數值【防禦公式】100 / (100 + 防禦)，100 為減傷曲線係數；傷害至少 1。](UnityProject/Assets/Game/Scripts/Progression/ProgressionService.cs#L36)
- [可調數值【強化能量門檻】下一級門檻 = 等級×10，例如 +1 要 10、+2 要 20；保留溢出能量。](UnityProject/Assets/Game/Scripts/Progression/ProgressionService.cs#L41)
- [可調數值【永久能力／裝備詞條】每級生命 +20、精力 +5、暴擊 +.005；武器詞條暴擊 +.05、滿蓄力 - .5s；套服回血 ×1.25、精力消耗 ×.8。](UnityProject/Assets/Game/Scripts/Progression/ProgressionService.cs#L68)
- [可調數值【詞條數】金 3／紫 2／藍 1／其他 0；武器最多 3 種、套服最多 2 種。](UnityProject/Assets/Game/Scripts/Progression/ProgressionService.cs#L82)
- [可調數值【強化暴擊】能量 ×2；發生機率讀 enhanceCrit。](UnityProject/Assets/Game/Scripts/Progression/ProgressionService.cs#L94)
- [可調數值【技能等級上限】目前 5 級，對應 skillUpgradeCosts 四筆升級費用。](UnityProject/Assets/Game/Scripts/Progression/ProgressionService.cs#L99)
- [可調數值【技能槽解鎖】Lv1／5／10／15 對應 1／2／3／4 格；同步 UI 鎖定文字。](UnityProject/Assets/Game/Scripts/Progression/ProgressionService.cs#L102)
- [可調數值【第二次復活價格】扣 1 顆已存檔勾玉；待結算掉落不能使用。](UnityProject/Assets/Game/Scripts/Progression/ProgressionService.cs#L108)
- [可調數值【通關品質抽選】1 星白75%／綠25%；2 星綠75%／藍25%；3 星綠55%／藍35%／紫10%；武器55%／套服45%。](UnityProject/Assets/Game/Scripts/Progression/ProgressionService.cs#L119)

### PrototypeServices.cs

- [可調數值【模擬廣告】等待 3 秒；只供第一次死亡復活，不是真實廣告 SDK。](UnityProject/Assets/Game/Scripts/Progression/PrototypeServices.cs#L5)
- [可調數值【免費測試補給】金幣 +2000、勾玉 +30、四種強化石各 +2；會寫入存檔。](UnityProject/Assets/Game/Scripts/Progression/PrototypeServices.cs#L11)
- [可調數值【極限模式預留】每層 50 擊殺／5 波、小 Boss 每 5 層、大 Boss 每 10 層；目前玩法未啟用。](UnityProject/Assets/Game/Scripts/Progression/PrototypeServices.cs#L15)
- [可調數值【極限模式預留成長】每層生命 +8%、傷害 +6%；未啟用。](UnityProject/Assets/Game/Scripts/Progression/PrototypeServices.cs#L17)

### GameUI.cs

- [可調數值【介面座標規則】所有 Rect(x,y,width,height) 都以 W×H 的虛擬畫布為基準；X向右、Y向下。](UnityProject/Assets/Game/Scripts/UI/GameUI.cs#L6)
- [可調數值【文字與顏色】Text 最後的 size 為字級；Color(r,g,b,a) 各值0～1，a為不透明度；Border width 為線寬。](UnityProject/Assets/Game/Scripts/UI/GameUI.cs#L7)
- [可調數值【同步修改】移動技能／暫停按鈕時，需同步 BlocksGesture 的輸入阻擋區；技能解鎖／強化上限文字需與玩法一致。](UnityProject/Assets/Game/Scripts/UI/GameUI.cs#L8)
- [可調數值【Awake 介面繪製】下方座標／尺寸／字級可按上方統一規則調整。](UnityProject/Assets/Game/Scripts/UI/GameUI.cs#L15)
- [可調數值【Metrics 介面繪製】下方座標／尺寸／字級可按上方統一規則調整。](UnityProject/Assets/Game/Scripts/UI/GameUI.cs#L17)
- [可調數值【手勢阻擋區】上方130、下方740以外，以及右下(1150,560,450,340)技能區。](UnityProject/Assets/Game/Scripts/UI/GameUI.cs#L20)
- [可調數值【波次公告】顯示2.8秒；Toast一般提示3秒，均不受遊戲暫停倍速影響。](UnityProject/Assets/Game/Scripts/UI/GameUI.cs#L23)
- [可調數值【Text 介面繪製】下方座標／尺寸／字級可按上方統一規則調整。](UnityProject/Assets/Game/Scripts/UI/GameUI.cs#L28)
- [可調數值【Fill 介面繪製】下方座標／尺寸／字級可按上方統一規則調整。](UnityProject/Assets/Game/Scripts/UI/GameUI.cs#L30)
- [可調數值【Border 介面繪製】下方座標／尺寸／字級可按上方統一規則調整。](UnityProject/Assets/Game/Scripts/UI/GameUI.cs#L32)
- [可調數值【Panel 介面繪製】下方座標／尺寸／字級可按上方統一規則調整。](UnityProject/Assets/Game/Scripts/UI/GameUI.cs#L34)
- [可調數值【Line 介面繪製】下方座標／尺寸／字級可按上方統一規則調整。](UnityProject/Assets/Game/Scripts/UI/GameUI.cs#L40)
- [可調數值【Image 介面繪製】下方座標／尺寸／字級可按上方統一規則調整。](UnityProject/Assets/Game/Scripts/UI/GameUI.cs#L42)
- [可調數值【Bar 介面繪製】下方座標／尺寸／字級可按上方統一規則調整。](UnityProject/Assets/Game/Scripts/UI/GameUI.cs#L44)
- [可調數值【OnGUI 介面繪製】下方座標／尺寸／字級可按上方統一規則調整。](UnityProject/Assets/Game/Scripts/UI/GameUI.cs#L46)
- [可調數值【主選單：標題、按鈕、背景圖】下方座標／尺寸／字級可按上方統一規則調整。](UnityProject/Assets/Game/Scripts/UI/GameUI.cs#L56)
- [可調數值【頁面標題與貨幣列】下方座標／尺寸／字級可按上方統一規則調整。](UnityProject/Assets/Game/Scripts/UI/GameUI.cs#L67)
- [可調數值【底部導覽列】下方座標／尺寸／字級可按上方統一規則調整。](UnityProject/Assets/Game/Scripts/UI/GameUI.cs#L71)
- [可調數值【關卡地圖：每章6關、節點與翻頁】下方座標／尺寸／字級可按上方統一規則調整。](UnityProject/Assets/Game/Scripts/UI/GameUI.cs#L74)
- [可調數值【劇情頁：文字與出發按鈕】下方座標／尺寸／字級可按上方統一規則調整。](UnityProject/Assets/Game/Scripts/UI/GameUI.cs#L83)
- [可調數值【戰鬥介面：血條、技能、連擊與蓄力】下方座標／尺寸／字級可按上方統一規則調整。](UnityProject/Assets/Game/Scripts/UI/GameUI.cs#L89)
- [可調數值【DrawWorldLabels 介面繪製】下方座標／尺寸／字級可按上方統一規則調整。](UnityProject/Assets/Game/Scripts/UI/GameUI.cs#L110)
- [可調數值【Shade 介面繪製】下方座標／尺寸／字級可按上方統一規則調整。](UnityProject/Assets/Game/Scripts/UI/GameUI.cs#L113)
- [可調數值【暫停選單】下方座標／尺寸／字級可按上方統一規則調整。](UnityProject/Assets/Game/Scripts/UI/GameUI.cs#L115)
- [可調數值【操作說明】下方座標／尺寸／字級可按上方統一規則調整。](UnityProject/Assets/Game/Scripts/UI/GameUI.cs#L121)
- [可調數值【復活對話框】下方座標／尺寸／字級可按上方統一規則調整。](UnityProject/Assets/Game/Scripts/UI/GameUI.cs#L124)
- [可調數值【結算獎勵】下方座標／尺寸／字級可按上方統一規則調整。](UnityProject/Assets/Game/Scripts/UI/GameUI.cs#L131)
- [可調數值【裝備格子與強化按鈕】下方座標／尺寸／字級可按上方統一規則調整。](UnityProject/Assets/Game/Scripts/UI/GameUI.cs#L142)
- [可調數值【技能卡片及裝備欄】下方座標／尺寸／字級可按上方統一規則調整。](UnityProject/Assets/Game/Scripts/UI/GameUI.cs#L159)
- [可調數值【永久能力強化卡片】下方座標／尺寸／字級可按上方統一規則調整。](UnityProject/Assets/Game/Scripts/UI/GameUI.cs#L167)
- [可調數值【商城物品與測試補給】下方座標／尺寸／字級可按上方統一規則調整。](UnityProject/Assets/Game/Scripts/UI/GameUI.cs#L173)
- [可調數值【音量與設定】下方座標／尺寸／字級可按上方統一規則調整。](UnityProject/Assets/Game/Scripts/UI/GameUI.cs#L179)
- [可調數值【Reserved 介面繪製】下方座標／尺寸／字級可按上方統一規則調整。](UnityProject/Assets/Game/Scripts/UI/GameUI.cs#L183)
- [可調數值【技能圖示：128像素畫布內的線段／圓形與配色】下方座標／尺寸／字級可按上方統一規則調整。](UnityProject/Assets/Game/Scripts/UI/GameUI.cs#L187)

## 修改前後檢查

- 機制測試位於 `Assets/Game/Tests/Editor`；Editor 選單測試位於 `PlaytestDriver.cs`。測試中的預期數字不是遊戲設定，改規格後需同步測試。
- `GameBalance.asset` 的陣列數量、波數、技能槽及存檔結構有連動，相關中文提示已標出。
- UI 技能描述／商店價格／鎖定等級若有文字數字，改玩法後也要同步。
- 遊戲設計最新規則見主 GDD；TEST_REPORT 保留歷史紀錄。
