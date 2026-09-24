# Blade Survivor｜改數值與重新試玩

Unity 專案資料夾：`UnityProject`  
目前使用版本：**Unity 6000.3.24f1**

## 最常用的流程

**停止 Play → 修改數值 → 存檔 → 切回 Unity 等待匯入／編譯 → 再按 Play。**

平常調整攻擊距離、速度、冷卻、掉落等，優先改 **GameBalance.asset**，不用重新建立專案或重開 Unity。

## 1. 我要改哪個檔案？

下表路徑以 `UnityProject/Assets/Game/` 為起點。

| 想修改的內容 | 檔案 | 說明 |
|---|---|---|
| 生命、移速、普攻距離／間隔、閃避、突進、蓄力、跳躍、倒地、掉落、裝備數值 | [Resources/GameBalance.asset](UnityProject/Assets/Game/Resources/GameBalance.asset) | **目前遊戲實際讀取的設定，通常只改這個就夠了。** |
| 查中文說明、增加新設定欄位、調整新資產預設值 | [Scripts/Data/GameBalance.cs](UnityProject/Assets/Game/Scripts/Data/GameBalance.cs) | 搜尋中文關鍵字或「可調數值」。 |
| 普攻／連招／蓄力／無敵如何運作、部分技能公式 | [Scripts/Combat/PlayerFighter.cs](UnityProject/Assets/Game/Scripts/Combat/PlayerFighter.cs) | 修改行為邏輯；已有設定欄位的數值仍回到 `.asset` 調整。 |
| 敵人追擊、攻擊、Boss 招式、身體半徑、擊退衰減 | [Scripts/Combat/EnemyActor.cs](UnityProject/Assets/Game/Scripts/Combat/EnemyActor.cs) | 敵人基本速度與冷卻也有一部分在 `.asset` 的 `enemies`。 |
| 出生位置、防重疊、掉落吸附、投射物判定 | [Scripts/Combat/BattleController.cs](UnityProject/Assets/Game/Scripts/Combat/BattleController.cs) | 戰場流程與物件行為。 |
| 升級與詞條公式、技能槽解鎖、通關獎勵抽選 | [Scripts/Progression/ProgressionService.cs](UnityProject/Assets/Game/Scripts/Progression/ProgressionService.cs) | 成本表與成長曲線多數仍在 `.asset`。 |
| 點擊、雙擊、長按、拖曳瞄準、鍵盤操作 | [Scripts/Input/GestureInput.cs](UnityProject/Assets/Game/Scripts/Input/GestureInput.cs)、[GestureResolver.cs](UnityProject/Assets/Game/Scripts/Input/GestureResolver.cs) | 手勢門檻優先改 `.asset`；判定流程才改這裡。 |
| 鏡頭角度／遠近／跟隨、場地、特效、音效 | [Scripts/Presentation/WorldPresentation.cs](UnityProject/Assets/Game/Scripts/Presentation/WorldPresentation.cs) | 搜尋「鏡頭」「跟隨」「刀光」等中文註解。 |
| 刀身長度、角色模型、跳躍／倒地姿勢、無敵閃爍 | [Scripts/Presentation/Geometry.cs](UnityProject/Assets/Game/Scripts/Presentation/Geometry.cs) | 模型刀長與實際攻擊距離是分開設定。 |
| UI 位置、字體大小、顏色、提示文字 | [Scripts/UI/GameUI.cs](UnityProject/Assets/Game/Scripts/UI/GameUI.cs) | 座標以 1600 × 900 虛擬畫布計算。 |
| 預設視窗尺寸、Mac 打包設定 | [Editor/ProjectSetup.cs](UnityProject/Assets/Game/Editor/ProjectSetup.cs) | 一般改戰鬥數值不需要動這個檔案。 |

完整欄位與其他程式數值位置，請看 **[中文數值調整索引](TUNING_GUIDE.md)**。

### GameBalance.cs 和 GameBalance.asset 差在哪？

- **`.cs`** 定義欄位、中文提示與「新建設定資產時」的預設值。
- **`.asset`** 儲存現有專案的設定；遊戲載入時，已儲存的值會覆蓋 `.cs` 初始化值。
- 所以只改 `.cs` 的 `moveSpeed=5`，現有 `.asset` 裡的 `moveSpeed` 不會跟著改。
- 新增欄位時，在 `.cs` 定義；既有欄位調數值時，優先改 `.asset`。若也想保留新建資產的相同預設，再同步 `.cs`。
- 改 MD 文件只會改說明，**不會改遊戲數值**。

## 2. 常用數值怎麼搜尋？

| 想調整 | 搜尋欄位 | 單位／注意事項 |
|---|---|---|
| 普攻距離 | `weaponRanges` | 公尺；陣列依序為長劍、雙刀、重劍 |
| 普攻間隔 | `attackIntervals` | 秒；陣列順序同上 |
| 普攻前搖 | `attackWindup` | 秒 |
| 普攻角度 | `weaponArcs` | 總角度；120 表示左右各 60 度 |
| 攻擊時移速 | `attackMoveMultiplier` | 保留比例；0.45 表示剩 45% 速度 |
| 普攻擊退 | `weaponKnockbacks` | 基礎設定，實際位移受敵人抗性與碰撞影響 |
| 正常移速 | `moveSpeed` | 公尺／秒 |
| 閃避距離 | `dodgeDistance` | 公尺 |
| 突進距離／寬度 | `swipeDistance`／`swipeWidth` | 公尺；寬度是完整寬度 |
| 蓄力距離／寬度 | `chargeLength`／`chargeWidth` | 公尺；不要改舊版 `chargeRanges` |
| 第四擊高度／前進距離／時間 | `comboJumpHeight`／`comboJumpDistance`／`comboJumpDuration` | 公尺／公尺／秒 |
| 玩家受擊後退 | `knockdownDistance` | 公尺 |
| 玩家起身時間／保護圈 | `knockdownDuration`／`recoveryRadius` | 秒／半徑公尺 |
| 敵人倒地時間 | `enemyKnockdownDuration` | 秒 |

**範例：把初始長劍距離改成 3.5 公尺**

在 Unity 的 Project 視窗選取 `Assets/Game/Resources/GameBalance.asset`，於 Inspector 展開 `Weapon Ranges`，將第一個元素 `Element 0` 改成 `3.5`。

也可以在 IDE 開啟 `.asset`，搜尋 `weaponRanges`，只修改下面第一筆數值：

```yaml
  weaponRanges:
  - 3.5
  - 1.55
  - 2.4
```

這只是修改範例；README 不會替你套用 3.5。直接編輯 `.asset` 時保留縮排、欄位名稱及陣列順序，不要貼入 C# 的 `//` 註解。若新欄位尚未出現在文字中，優先從 Inspector 修改並儲存。

## 3. 存檔後，Unity 怎麼重新 Run？

### 在 Unity 編輯器內試玩

1. **先停止 Play Mode**：如果上方 Play 按鈕處於啟用狀態，再按一次停止。暫停不等於停止。
2. 在 IDE 修改 `.asset` 或 `.cs`，按 **⌘S** 儲存；也可在 Unity Inspector 直接修改設定資產，完成後用 **File → Save Project** 儲存。
3. 切回 Unity，等待匯入完成；修改 `.cs` 時還會重新編譯。若沒有自動更新，使用 **Assets → Refresh**。
4. 查看 **Window → General → Console**；若有新的紅色編譯錯誤，先修正後再試玩。
5. 在 Project 視窗找到並雙擊 `Assets/Game/Scenes/BattlePrototype.unity`。已開啟此場景就不用再開一次。
6. 按上方 **▶ Play**，切到 **Game** 分頁，開始新一場戰鬥體驗數值。

**不需要每次執行 `Prepare Playable Project`。** 該選單會重建基本場景，平常改數值只要停止後重新 Play。

建議停止播放再修改，避免腳本熱重載造成執行中參照丟失。Play Mode 中修改場景物件可能被還原；修改 ScriptableObject 資產則可能被保留，不要把播放模式當成一定會復原的沙盒。

### 在獨立 Mac 遊戲中試玩

已經打包的 `.app` 不會自動讀取專案剛改的新數值。需要重新打包：

1. 關閉正在執行的 **Blade Survivor.app**。
2. 依上方步驟完成存檔、匯入與編譯，並停止 Unity Play Mode。
3. 在 Unity 上方選單選 **Blade Survivor → Build macOS Playtest**。
4. 等待建置完成，Console 出現 `BLADE_BUILD Succeeded 0`。
5. 從 Finder 重新開啟根目錄的 **[Builds/Blade Survivor.app](<Builds/Blade Survivor.app>)**。

重新打包會更新同一路徑的遊戲，但不會清除玩家存檔。

## 4. 改了卻沒變？

- **只改 `.cs` 預設值？** 請確認 `.asset` 中對應值也已修改。
- **還在玩舊 `.app`？** 需要重新建置，並關閉舊程式再開新版本。
- **只暫停沒有停止？** 完全停止 Play，再重新啟動。
- **改到舊版保留欄位？** `chargeRanges`、`chargeArcs`、`chargeMoveMultiplier`、`dodgeIFrameStart/End`、`attackActive` 的中文提示已標明目前不生效。
- **角色數值受裝備／養成影響？** 基礎值還會乘上裝備、技能和永久升級效果，請看 [操作說明的存檔段落](PLAYTEST.md#存檔與調整)。一般調數值不需要刪存檔。

## 其他文件

- [GitHub 分享與自動部署](SHARE_TESTING.md)
- [完整中文數值索引](TUNING_GUIDE.md)
- [遊戲操作說明](PLAYTEST.md)
- [最新玩法企劃](unity_2_5D_action_rpg_codex_gdd_v2_visual.md)
- [測試與修改紀錄](TEST_REPORT.md)
- [美術來源](ART_SOURCES.md)
