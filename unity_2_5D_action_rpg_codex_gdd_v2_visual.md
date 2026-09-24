# Unity 2.5D 橫向動作 RPG — 完整 GDD / Codex + Unity MCP 開發規格

> 版本：v0.2 Prototype / 操作回饋修訂（2026-09-24）  
> 目標：此文件可直接交給 Codex，透過 Unity MCP 從零建立可玩的 Prototype，之後所有數值均必須資料化，方便實機測試後調整。  
> 核心原則：**先把戰鬥手感、波次、成長與掉落做對，再換正式美術。不要把平衡數值硬寫死在 MonoBehaviour。**


## 最新玩法修訂摘要（2026-09-24）

本節及下方已修訂章節是目前玩法規格；以本輪使用者確認的操作回饋覆蓋初稿數值。

| 機制 | 最新規則 |
|---|---|
| 地圖／鏡頭 | 160 × 160 世界單位；固定 42° 斜俯視並跟隨角色，敵人從目前畫面外四周接近 |
| 移動朝向 | 點擊方向決定前進與面向；移動中只攻擊前方，停下才自動轉向敵人 |
| 初始長劍 | 半徑 3m、120°、間隔 0.6s、前搖 0.15s；刀身相對初版加長 40% |
| 四連擊 | 第 1–3 下前方扇形；第 4 下跳高 1.1m、前進 1.1m，0.4s 落地，半徑 3m／360°／×1 傷害 |
| 斷招 | 下一個攻擊間隔到期未接續則重置；被打斷或使用其他動作也重置 |
| 玩家受擊 | 後退約 1m，倒地至站起共 2s；全程無敵、半透明閃爍，半徑 2m 阻擋敵人身體，起身立即解除 |
| 蓄力 | 按住並拖曳瞄準，站定；放開沿最後方向攻擊前方長 6m、寬 2.5m 的矩形；保留三段倍率 |
| 敵人倒地 | 蓄力、突進及第四擊命中的存活敵人倒地約 2s；包含 Boss，倒地仍可受傷 |
| 敵人間距 | 所有存活狀態皆保持身體間距，不能重疊擠成一點 |
| 滑動突進 | 6m 距離、2m 路徑寬、×1.25 傷害、35 SP、0.8s 冷卻；同敵人最多命中一次 |

操作方式見 [PLAYTEST.md](PLAYTEST.md)，實際驗證與歷次修改紀錄見 [TEST_REPORT.md](TEST_REPORT.md)。測試紀錄中的舊數值代表當時版本，不作為最新玩法規格。

## 視覺參考素材（必讀）

在專案根目錄 / `images/` 內有 3 張視覺參考圖，Codex 必須把它們視為**第一版美術方向依據**：

- `player_character_concept.png`
  - 主角風格參考
  - 用於主角比例、服裝方向、武器掛點、戰鬥姿態、2.5D / 類 3D 呈現感
- `enemy_concept_sheet.png`
  - 敵人風格參考
  - 用於一般怪、速度怪、遠程怪、重型怪的輪廓差異、武器差異、剪影辨識
- `ui_asset_reference.png`
  - UI 與 2D 圖示風格參考
  - 用於戰鬥 HUD、技能圖示、強化石圖示、勾玉、金幣、裝備卡片、結算畫面、裝備欄位等

### 視覺執行原則

1. **戰鬥中的角色、敵人、Boss、其手持武器必須以 2.5D / 類 3D 方向製作**。
2. **非戰鬥中的大多數介面與道具圖示使用 2D 即可**，例如：
   - HP / SP 掉落物
   - 強化石
   - 勾玉
   - 金幣 UI
   - 技能 Icon
   - 強化頁 Icon
   - 裝備欄位 / 卡片
   - 關卡結算掉落物顯示
3. Codex 實作 Prototype 時，主角與敵人**不能只用「正方形主角、長方形敵人」或單純膠囊體當作主要成果**。
4. 即使是 Placeholder，也至少要做出：
   - 基本人形主角
   - 基本人形敵人
   - 可辨識的武器外觀
   - 不同敵種之間明確不同的輪廓與武器
5. 四種普通敵人**不能只是同一模組換衣服**，必須至少做到：
   - 輪廓不同
   - 武器不同
   - 姿態不同
   - 第一眼就能辨認職能
6. UI 不可只有裸文字，至少要有基本：
   - HUD 框體
   - 技能按鈕底板
   - HP / SP 視覺條
   - 結算頁版面
   - 裝備頁 / 強化頁的面板與插槽


---

# 0. Codex 執行指令

請以此文件為唯一玩法規格，使用 Unity 建立手機橫向、固定俯視 2.5D / 類 3D 動作 RPG。

開發順序：

1. 建立資料架構與 ScriptableObject。
2. 完成玩家移動、手勢輸入、自動普通攻擊、扇形 AoE、擊退。
3. 完成敵人 AI、敵人攻擊間隔、受擊、擊退、死亡。
4. 完成 10 Wave 關卡與 Boss Wave。
5. 完成 HP / SP、閃避、滑動攻擊、蓄力攻擊。
6. 完成四格技能系統。
7. 完成關卡結算與 1～3 星。
8. 完成 EXP / Level / Gold / 永久屬性強化。
9. 完成裝備、品質、詞條、武器強化。
10. 完成戰鬥掉落、Pending Reward、失敗清除。
11. 完成復活流程。
12. 完成劇情模式 UI / 裝備 / 技能 / 強化 / 招募預留 / 商城。
13. 最後再做極限模式骨架。

Prototype 階段可以先使用簡化資源與暫代素材，但請遵守以下規則：
- 可使用簡化 low-poly / 臨時模型 / 基本材質快速驗證玩法。
- **不要只交付純 Primitive / Capsule / Cube / Square / Rectangle 的極簡幾何版本作為主要視覺成果。**
- 至少要做出：
  - 基本人形主角
  - 基本人形敵人
  - 可辨識的武器外觀
  - 基本 UI 框線 / 按鈕 / 圖示
- 可先粗糙，但必須看得出整體風格有參考 `player_character_concept.png`、`enemy_concept_sheet.png`、`ui_asset_reference.png`。

所有重要參數必須可以在 Inspector 或 ScriptableObject 修改。

---

# 1. 遊戲定位

- Engine：Unity
- Platform：iOS / Android
- Orientation：Landscape 橫向
- Camera：固定 42° 斜俯視角並完整跟隨角色，不允許玩家旋轉；非固定螢幕大小的競技場
- 戰場：160 × 160 世界單位，可四處移動。敵人出生在目前鏡頭外，並位於地圖內，從四周逐漸靠近。
- Visual：2.5D / 類 3D
- Genre：關卡制動作 RPG + 裝備養成 + 角色養成
- Normal Attack：自動攻擊
- Player Skill：手動施放
- Core：走位、閃避、滑動突進、蓄力、技能、清怪效率

---

# 2. 首頁

首頁：

- 劇情模式
- 極限模式
- 設定

---

# 3. 劇情模式首頁

主要區域：

- 關卡地圖

底部 Menu：

1. 技能
2. 裝備
3. 強化
4. 招募
5. 商城

「招募」第一版只保留入口與資料介面，不需要完整功能。

---

# 4. 玩家操作

## 4.1 不使用虛擬搖桿

整個戰鬥畫面的大部分非 UI 區域皆為 Gesture Area。

右下技能 UI 與右上 Pause 必須排除於 Gesture Input。

---

## 4.2 單點

玩家短按戰鬥區域：

1. 將 Screen Position Raycast 到地面。
2. 計算 Player → Click World Point 的方向。
3. 設定為新的 `MoveDirection`。
4. 玩家立即面向並持續朝該方向移動，自動鎖敵不能覆蓋移動朝向。
5. 不會因抵達點擊位置而停止。
6. 下一次點擊會更新方向。

---

## 4.3 雙擊：閃避

- Double Tap Window：0.24 秒（可調）
- 朝第二次點擊位置方向快速閃避。
- 消耗 SP：20
- Dodge Distance：6m
- Dodge Duration：0.45 秒
- Invincible Frame：整段 Dodge Duration（0～0.45 秒），期間完全無敵且不被打斷
- Internal Cooldown：0.35 秒
- 無足夠 SP 時不能使用。

閃避成功躲掉的攻擊：

- 不扣 HP
- 不計入三星「受攻擊次數」

---

## 4.4 滑動：突進範圍攻擊

快速滑動並放開、且尚未進入長按蓄力時，手勢位移超過 Swipe Threshold：

- Threshold：建議以螢幕短邊約 8% 為初始值，不硬寫 px。
- 朝滑動方向快速突進。
- 路徑上的敵人受到 AoE 傷害。
- 同一次 Swipe Attack，每名敵人最多受傷一次。
- 消耗 SP：35
- Cooldown：0.8 秒
- 整段突進完全無敵、不受傷、不被打斷；結束即解除。
- 命中的存活敵人倒地約 2 秒，含 Boss；同次仍只傷害一次。
- Damage：一般普攻傷害 × 1.25
- 路徑寬度：2m
- 突進距離：6.0m

---

## 4.5 長按：蓄力攻擊

### Charge Timing

- 0～0.49 秒：不成立為蓄力。
- 0.5～1.49 秒：Stage 1 Charge
- 1.5～2.99 秒：Stage 2 Charge
- 3.0 秒：Full Charge

### Charge 狀態

- 蓄力時站定；按住不放可持續拖曳，以起始按壓點到目前觸點的方向瞄準。
- 顯示隨瞄準方向轉動的 6m × 2.5m 矩形預覽。放開依最後瞄準方向出招。
- 進入蓄力後，拖曳不再觸發滑動突進；無拖曳時保留既有面向。
- 蓄力時**沒有霸體**
- 被任何有效敵人攻擊命中 → 立即中斷蓄力
- 已經蓄滿 3 秒但尚未放開 → 仍可被攻擊打斷
- 只有玩家「放開手指，正式進入 Charge Attack 動畫」後才有霸體
- 此處 Charge Release／技能的出招霸體只代表**不被中斷 / 不進 Hit Stun**，與受擊倒地期間的完全無敵不同。
- 霸體期間仍會扣 HP，也仍計入三星受攻擊次數

### Charge Damage / Range

一般刀第一版：

| 蓄力 | Damage | 前方長度 | 總寬度 | 擊倒 |
|---|---:|---:|---:|---|
| 0.5～1.49s | ×1.50 | 6m | 2.5m | 約 2s |
| 1.5～2.99s | ×2.20 | 6m | 2.5m | 約 2s |
| 3.0s Full | ×3.00 | 6m | 2.5m | 約 2s |

矩形以角色當前位置為起點向前延伸，判定包含敵人體積。三階段不再使用舊版扇形／圓形範圍。

Full Charge 不會自動施放，玩家可以保持蓄滿，直到放開或被敵人打斷。

---

# 5. HUD

Landscape UI：

左上：

- HP Bar
- SP Bar（在 HP 正下方）

右上：

- Pause

右下：

- Skill 1
- Skill 2
- Skill 3
- Skill 4

不顯示普通攻擊鍵。

---

# 6. 玩家初始屬性

第一版：

- Base HP：100
- Base SP：100
- Base Crit Chance：5%
- Crit Damage：150%
- Move Speed：5.0m/s
- 初始武器：一般刀
- 初始刀 Damage：9～11

---

# 7. 普通攻擊

## 7.1 普攻不做單體鎖定

`CurrentTarget` 只負責：

- 移動中只從前方扇形選擇目標，不能改變角色面向。
- 停下移動時才可自動轉向附近敵人。
- 決定何時開始攻擊。

真正 Hit 時：

- 找出攻擊範圍內所有敵人
- 所有符合條件敵人都受到傷害
- 不設 Max Target

---

## 7.2 一般刀第一版參數

- Target Scan Range：5.0m
- Attack Range：3.0m
- Attack Arc：120°（左右各 60°）
- Attack Interval：0.6 秒
- Windup：0.15 秒
- Active Hit Window：約 0.12 秒
- Recovery：剩餘 Attack Interval
- 攻擊期間 Move Speed × 0.45（保留 45% 移速，減速 55%）
- 普攻 Knockback：0.55m（基礎）

Damage：

- 每次 Hit 在武器 MinDamage～MaxDamage 之間取整數亂數。
- 初始 9～11。

一般怪 HP = 20，因此：
- 大部分情況 2 刀
- 傷害偏低時需要第 3 刀

---

## 7.3 Hit Detection

不要使用「刀模型 Collider 碰到才扣血」作為主要判定。

建議：

1. `Physics.OverlapSphereNonAlloc`
2. 先找 Attack Range 內 Enemy Layer。
3. 使用角色 Forward 與 Enemy 方向計算角度。
4. `angle <= attackArc / 2` → 命中。
5. 大型敵人距離判定使用 `Collider.ClosestPoint()`，不要只使用 Transform 中心點。
6. 每次 Attack Instance 使用 HashSet / AttackId 防止同一刀重複命中同一敵人。

---

## 7.4 四連擊

- 初始長劍按 0.6 秒攻擊間隔接續第 1、2、3、4 下；不是上一刀結束後再額外等待 0.6 秒。
- 下一個攻擊時機必須能出手，否則連擊歸零，下次從第一下開始。
- 第 1～3 下：前方 120°、半徑 3m，倍率 ×1。
- 第 4 下：向前跳約 1.1m、最高約 1.1m，跳躍約 0.4 秒；落地才造成以落點為中心半徑 3m、360°、×1 傷害，並擊倒存活敵人約 2 秒。
- 完成第四擊後重置段數；受擊打斷、閃避、突進、技能或蓄力也會重置。第四擊從起跳至落地傷害判定完成全程無敵，不受傷、不被打斷；落地後解除。
- HUD 顯示連擊段數。其他武器／攻速增益沿用各自攻擊間隔判定接續。

## 7.5 受擊倒地、無敵與敵人間距

- 沒有閃避無敵、護盾或出招霸體抵擋時，有效受擊先扣除該次傷害，再向遠離攻擊來源方向後退約 1m 並倒地。
- 從後退、倒地至完全站起總共約 2 秒，期間完全無敵：任何後續傷害無效、不增加受擊次數、不延長起身時間。
- 期間鎖定移動／攻擊／技能，身體半透明閃爍，HUD 顯示「無敵・起身中」。
- 以角色為中心半徑 2m 的隔離圈阻擋敵人身體進入，計算時另加敵人身體半徑；起身即解除無敵、閃爍與隔離。
- 所有存活敵人於移動、前搖、攻擊、受擊和倒地時都必須保持身體間距；武器模型不作為阻擋體積。
- 蓄力、突進與第四擊可擊倒包含 Boss 的存活敵人約 2 秒，中止其當前攻擊。敵人倒地不具有玩家的無敵／隔離圈，仍可受到傷害，站起後恢復追擊。

---

# 8. 三種武器定位

Prototype 優先只完成一般刀，但架構必須支援三種 WeaponType。

| 武器 | Damage 倍率 | Attack Interval | Range | Arc | Knockback |
|---|---:|---:|---:|---:|---:|
| 雙刀 | ×0.75 | 0.55s | 1.55m | 100° | 0.25m |
| 一般刀 | ×1.00 | 0.60s | 3.00m | 120° | 0.55m |
| 雙手大刀 | ×1.45 | 1.25s | 2.40m | 160° | 0.70m |

特色：

### 雙刀
- 距離最短
- 攻擊最快
- 單次傷害最低
- 風險最高

### 一般刀
- 全能力中等
- 初始武器

### 雙手大刀
- 目前距離 2.4m；一般刀因操作回饋已加長至 3m
- 攻擊最慢
- 單次傷害最高
- AoE / Knockback 最強

---

# 9. 擊退系統

玩家普通攻擊命中敵人：

- Damage
- Hit Reaction
- Normal Knockback

角色與敵人**身體碰撞不會造成傷害**。

不同 EnemyType Knockback Multiplier：

| Enemy | Knockback Received |
|---|---:|
| 一般 | 100% |
| 速度 | 120% |
| 遠程 | 100% |
| 重型 | 30% |
| Boss | 0% |

一般怪 / 速度怪 / 遠程怪在 Windup 中被正常普攻擊退：

- Attack Cancel
- 進入 Hit Reaction
- 重新 Chase

重型怪：

- 普通攻擊可造成小幅位移
- 特定 Heavy Attack Windup 可以設為 SuperArmor，不被普通攻擊打斷

Boss：

- 普通攻擊不造成 Knockback
- 普攻不打斷 Boss Attack

---

# 10. 敵人傷害判定核心規則

**Touch / Body Collision != Damage**

玩家碰到敵人不扣血。

敵人必須走完整攻擊流程：

`Chase → AttackRange → Windup → ActiveHit → Recovery → Cooldown → Chase/Attack`

只有 ActiveHit 的 HitBox / Projectile 命中 Player，才：

- 扣 HP
- `PlayerDamageTakenCount + 1`

因此玩家可以近距離砍敵人，而不會因為身體碰撞持續扣血。

---

# 11. 第一版敵人類型

## 11.1 Lv.1 基礎數值

| Type | HP | Damage | Move Speed | Attack Range | Windup | Recovery | Cooldown |
|---|---:|---:|---:|---:|---:|---:|---:|
| 一般 | 20 | 8 | 2.2 | 1.15m | 0.45s | 0.45s | 1.15s |
| 速度 | 15 | 6 | 3.2 | 1.00m | 0.28s | 0.30s | 0.85s |
| 遠程 | 18 | 7 | 1.8 | 6.0m | 0.55s | 0.40s | 1.50s |
| 重型 | 45 | 15 | 1.4 | 1.50m | 0.85s | 0.70s | 1.80s |
| Boss 1-3 | 250 | 20 基準 | Custom | Custom | Custom | Custom | Custom |

遠程：

- Preferred Distance：約 4.5～5.0m
- Projectile Speed：9m/s

---

# 12. 敵人成長曲線

以下為不同進度的 HP Anchor：

| 推薦 Lv | 一般 | 速度 | 遠程 | 重型 | Boss 基準 HP |
|---|---:|---:|---:|---:|---:|
| 1 | 20 | 15 | 18 | 45 | 250 |
| 10 | 35 | 26 | 30 | 80 | 500 |
| 20 | 60 | 45 | 52 | 135 | 900 |
| 30 | 95 | 70 | 82 | 210 | 1500 |
| 40 | 145 | 105 | 125 | 320 | 2300 |
| 50 | 210 | 150 | 180 | 460 | 3400 |

Enemy Damage Anchor：

| 推薦 Lv | 一般 | 速度 | 遠程 | 重型 | Boss 普攻基準 |
|---|---:|---:|---:|---:|---:|
| 1 | 8 | 6 | 7 | 15 | 20 |
| 10 | 14 | 11 | 12 | 25 | 35 |
| 20 | 22 | 17 | 19 | 40 | 55 |
| 30 | 32 | 25 | 28 | 58 | 80 |
| 40 | 45 | 35 | 40 | 82 | 110 |
| 50 | 60 | 47 | 53 | 110 | 150 |

非 Anchor Level 使用線性插值。

難度不能只靠加 HP / Damage，也必須提高：

- 遠程比例
- 速度怪比例
- 重型怪比例
- Attack Pattern
- AI 壓力
- 混合兵種比例

---

# 13. 關卡 Wave 系統

每關：

- 10 Wave
- 每 Wave 固定時間刷新
- Wave Interval：15 秒
- **不等待上一波清空**
- 上一波殘留怪永久留場，直到死亡
- 玩家清怪越慢，怪物就會越堆越多

Spawn：

- 每 Wave 從不同方向 / 多個 Spawn Point 靠近玩家。
- 不要固定單一方向。

---

## 13.1 普通關

- Wave 1～10：每波 10 隻
- Total Enemy：100

---

## 13.2 Boss 關

每 3 關一個 Boss。

例如：

- 1-1 Normal
- 1-2 Normal
- 1-3 Boss
- 1-4 Normal
- 1-5 Normal
- 1-6 Boss

Boss 關：

- Wave 1～9：每波 10 隻
- Wave 10：9 隻一般 / 混合怪 + 1 Boss
- Total Enemy Units：100

Wave 10 在 135 秒準時出現。

如果 Wave 1～9 有小怪沒死：

- 不清除
- Boss 與所有殘留怪一起存在

Boss 死亡也不代表立刻過關。

必須清除本場所有 100 個敵方單位才 Clear。

---

# 14. Wave Composition 第一版範例

依 StageConfig 可覆寫。

Normal Stage 初始範例：

- Wave 1：10 一般
- Wave 2：10 一般
- Wave 3：8 一般 + 2 速度
- Wave 4：7 一般 + 3 速度
- Wave 5：7 一般 + 3 遠程
- Wave 6：6 一般 + 2 速度 + 2 遠程
- Wave 7：5 一般 + 2 速度 + 2 遠程 + 1 重型
- Wave 8：4 一般 + 2 速度 + 3 遠程 + 1 重型
- Wave 9：3 一般 + 3 速度 + 2 遠程 + 2 重型
- Wave 10：3 一般 + 2 速度 + 3 遠程 + 2 重型

Boss Stage Wave 10：

- 9 隻依關卡設定的混合怪
- 1 Boss

第一關如果 Prototype 壓力太高，可先降低遠程 / 重型比例，但系統必須支援上述配置。

---

# 15. 關卡星級

## 一星
成功破關。

## 二星
180 秒內破關。

## 三星
同時符合：

- 180 秒內破關
- `PlayerDamageTakenCount <= 3`

Prototype 若實測過難，改為：

- `<= 5`

有效受傷定義：

- 玩家真的扣到 HP → +1
- Dodge iFrame 躲掉 → 不算
- 金色套服開場護盾抵消 → 不算
- Charge Release 霸體時被打仍扣 HP → 算
- Body Collision → 不算

---

# 16. SP 系統

初始：

- Max SP：100
- SP Regen：10 / sec
- 使用 SP 後延遲 0.75 秒才開始恢復

消耗：

- Dodge：20
- Swipe Attack：35

SP = 0：

- 仍可普通移動
- 仍可普攻
- 仍可 Charge
- 仍可 Skill
- 不能 Dodge / Swipe Attack

技能只使用 CD，不消耗 SP。

---

# 17. 主動技能系統

角色最多裝備 4 個技能。

Skill Slot 解鎖：

- Lv.1：Slot 1
- Lv.5：Slot 2
- Lv.10：Slot 3
- Lv.15：Slot 4

Skill 本身需達角色等級後，以 Gold 購買。

---

## 17.1 第一版技能

### Skill 1 — 旋風斬
- Unlock：Lv.1
- 第一個技能免費
- 360° AoE
- Range：3.0m
- Damage：ATK ×1.60
- CD：8s
- 中 Knockback

### Skill 2 — 劍氣斬
- Unlock：Lv.3
- Cost：500 Gold
- 角色面向方向直線劍氣
- Range：7m
- Damage：ATK ×1.80
- CD：7s
- 用來處理遠程敵人

### Skill 3 — 戰意
- Unlock：Lv.6
- Cost：1200 Gold
- Duration：8s
- Attack Damage +20%
- Attack Speed +15%
- CD：20s

### Skill 4 — 震地斬
- Unlock：Lv.9
- Cost：2200 Gold
- 前方大範圍 180°
- Range：3.5m
- Damage：ATK ×2.40
- CD：12s
- 強 Knockback

### Skill 5 — 血刃
- Unlock：Lv.12
- Cost：3500 Gold
- Duration：6s
- Damage 造成時恢復 HP
- Life Steal：5%
- 每個 Attack Instance 回復量上限：MaxHP ×5%
- CD：25s

### Skill 6 — 亂舞
- Unlock：Lv.15
- Cost：5200 Gold
- 約 2 秒連續 5 Hit
- Total Damage：約 ATK ×3.50
- CD：18s
- 小～中範圍
- 施放時有霸體但仍會受傷

### Skill 7 — 無雙斬
- Unlock：Lv.18
- Cost：7500 Gold
- 360° 大範圍
- Range：4.5m
- Damage：ATK ×4.50
- CD：30s
- 強 Knockback
- 出招期間霸體但仍會受傷

---

## 17.2 Skill Upgrade

Skill Max Lv：5

Lv.1 → 5 Upgrade Cost：

- Lv2：600
- Lv3：1200
- Lv4：2400
- Lv5：4800 Gold

Damage Skill：

- 每提升 1 Skill Lv，基礎技能傷害 +10%（相對於 Lv1 Skill Multiplier）

Buff Skill：

- 每級提升效果值約 10%，資料化。

所有技能數值使用 SkillData ScriptableObject，不硬寫。

---

# 18. 角色等級 / EXP

Player Max Level：50

升下一級 EXP：

`RequiredExp(Lv → Lv+1) = 100 + (Lv - 1) × 40`

例：

- Lv1→2：100
- Lv2→3：140
- Lv10→11：460
- Lv20→21：860
- Lv30→31：1260
- Lv40→41：1660
- Lv49→50：2020

Lv1 → Lv50 總需求：

**51,940 EXP**

---

# 19. 關卡 EXP

不採「每隻怪直接給角色 EXP」。

EXP 在 Clear 結算發放，避免刷怪漏洞。

Normal Stage EXP：

`EXP = roundTo10(60 + RecommendedLevel × 20)`

Boss Stage：

`BossEXP = roundTo10(NormalEXP × 1.25)`

Anchor：

| 推薦等級 | Normal | Boss |
|---|---:|---:|
| 1 | 80 | 100 |
| 10 | 260 | 330 |
| 20 | 460 | 580 |
| 30 | 660 | 830 |
| 40 | 860 | 1080 |
| 50 | 1060 | 1330 |

重刷舊關卡：

- 不衰減 EXP
- 後期關卡自然因基礎 EXP 較高而效率更高

---

# 20. 永久屬性強化

五種：

1. 攻擊
2. 防禦
3. HP
4. SP
5. 爆擊

每種 Max Lv：20

效果：

| 屬性 | 每 Lv | Lv20 |
|---|---:|---:|
| Attack | +2% | +40% |
| Defense | 受到傷害 -1% | -20% |
| HP | +20 | +400 |
| SP | +5 | +100 |
| Crit Chance | +0.5% | +10% |

基礎 Crit = 5%。

如果武器有 Crit 詞條 +5%，角色永久 Crit Lv20：

- 約 20% Crit（未含未來其他系統）

Crit Damage：

- 150%

---

# 21. 永久屬性升級 Gold Cost

五個屬性使用同一 Cost Curve：

| 升到 Lv | Gold |
|---|---:|
| 1 | 200 |
| 2 | 250 |
| 3 | 300 |
| 4 | 350 |
| 5 | 400 |
| 6 | 450 |
| 7 | 550 |
| 8 | 650 |
| 9 | 750 |
| 10 | 900 |
| 11 | 1050 |
| 12 | 1250 |
| 13 | 1450 |
| 14 | 1700 |
| 15 | 2050 |
| 16 | 2400 |
| 17 | 2850 |
| 18 | 3350 |
| 19 | 3950 |
| 20 | 4650 |

單一屬性 Lv0→20：

- 29,500 Gold

五項全部 Lv20：

- 147,500 Gold

---

# 22. 關卡 Gold

普通怪不直接掉 Gold，避免地面過度混亂。

Normal Stage Clear Gold：

`Gold = roundTo10(220 + RecommendedLevel × 20)`

Boss Stage：

`BossGold = roundTo10(NormalGold × 1.40)`

約：

| 推薦 Lv | Normal | Boss |
|---|---:|---:|
| 1 | 240 | 340 |
| 10 | 420 | 590 |
| 20 | 620 | 870 |
| 30 | 820 | 1150 |
| 40 | 1020 | 1430 |
| 50 | 1220 | 1710 |

Gold 可用於：

- 永久屬性
- Skill 購買
- Skill 升級

也可透過付費取得 Gold。

---

# 23. 裝備欄位

目前只做：

- Weapon ×1（必須裝備）
- Suit ×1（可不裝）
- Accessory ×2（欄位保留，內容後續再做）

第一版不做：

- Helmet
- Gloves
- Shoes 等細分

---

# 24. 裝備世代

需求角色等級：

- Lv1
- Lv10
- Lv20
- Lv30
- Lv40
- Lv50（最後一套）

達到等級只是解鎖該世代掉落 / 裝備資格。

Prototype 不需要一次做完所有美術，但資料結構需支援。

---

# 25. 裝備品質

品質：

1. 白
2. 綠
3. 藍
4. 紫
5. 金

白～紫：

- 關卡破關掉落

金：

- **永遠不從關卡掉落**
- 只能商城使用勾玉購買

商城不賣白 / 綠 / 藍 / 紫。

---

# 26. 一般刀六世代攻擊力

Rarity Base Multiplier：

- 白：100%
- 綠：110%
- 藍：125%
- 紫：145%
- 金：170%

一般刀：

| Req Lv | 白 | 綠 | 藍 | 紫 | 金 |
|---|---|---|---|---|---|
| 1 | 9–11 | 10–12 | 11–14 | 13–16 | 15–19 |
| 10 | 13–16 | 14–18 | 16–20 | 19–23 | 22–27 |
| 20 | 19–23 | 21–25 | 24–29 | 28–33 | 32–39 |
| 30 | 27–33 | 30–36 | 34–41 | 39–48 | 46–56 |
| 40 | 38–46 | 42–51 | 48–58 | 55–67 | 65–78 |
| 50 | 52–64 | 57–70 | 65–80 | 75–93 | 88–109 |

雙刀與雙手大刀由 WeaponType Damage Multiplier 套用。

---

# 27. 武器強化

目前只有 Weapon 可以強化。

- +0 ～ +10
- 每把武器最多使用強化石 5 次
- 使用一次石頭就消耗 1 次 Enhance Attempt
- 5/5 後不能再投入任何強化石
- 如果提前 +10，直接完成

Energy Requirements：

| Enhance | Need |
|---|---:|
| +0 → +1 | 10 |
| +1 → +2 | 20 |
| +2 → +3 | 30 |
| +3 → +4 | 40 |
| +4 → +5 | 50 |
| +5 → +6 | 60 |
| +6 → +7 | 70 |
| +7 → +8 | 80 |
| +8 → +9 | 90 |
| +9 → +10 | 100 |

Total Energy +0→+10：

**550**

Energy 會跨等級保留，不浪費 Overflow。

---

## 27.1 強化石 Energy

正式第一版：

| Stone | Energy |
|---|---:|
| S | 25～40 |
| M | 40～55 |
| L | 55～70 |
| XL | 70～85 |

每次投入：

- 20% 機率 Critical Enhance
- 該次 Energy ×2

5 顆 XL 全投入時：

- 平均 Energy 約 465
- 五次內達到 +10 的機率約 **12.56%**

目的：

- +10 稀有但可達成
- XL 有明顯價值
- S/M 主要做中低強化
- L/XL 用於高強化挑戰

---

## 27.2 武器強化效果

每 +1：

- Weapon Base Damage +3%

因此：

- +10 = Weapon Base Damage +30%

Damage 計算順序建議：

`RandomWeaponDamage × (1 + EnhanceBonus) × (1 + PermanentAttackBonus) × SkillMultiplier × CritMultiplier × BossBonus`

最終取整數。

---

# 28. 武器詞條

白、綠：

- 無詞條

三個武器詞條：

1. Crit Chance +5%
2. Boss Damage +12%
3. Full Charge Time -0.5 秒

Rarity：

- 藍：隨機 1 條，不重複
- 紫：隨機 2 條，不重複
- 金：三條全部擁有

Charge -0.5 秒：

- Full Charge 3.0s → 2.5s
- Stage 閾值按比例縮短或使用 ChargeData 可配置

---

# 29. 套服

第一版 Suit 主要提供 Defense。

White Suit Base DEF Anchor：

- Lv1：2
- Lv10：4
- Lv20：7
- Lv30：11
- Lv40：16
- Lv50：22

品質倍率同 Weapon：

- 白 100%
- 綠 110%
- 藍 125%
- 紫 145%
- 金 170%

取整數。

Incoming Damage：

`RawDamage × 100 / (100 + SuitDEF) × (1 - PermanentDefenseReduction)`

最低 Damage = 1。

---

# 30. 套服詞條

白、綠：

- 無詞條

兩個一般詞條：

1. HP / SP Pickup Restore +25%
2. Dodge / Swipe Attack SP Cost -20%

品質：

- 藍：隨機 1 條
- 紫：兩條都有
- 金：兩條都有 + 金裝專屬護盾

金色 Suit 專屬：

### Opening Shield

- 開場得到 1 Shield Charge
- 第一次敵人有效攻擊 Hit：
  - 完全抵消 Damage
  - 不扣 HP
  - 不計入三星 DamageTakenCount
  - Shield 消失
- 每場只生效一次
- 復活不重置 Shield

---

# 31. 關卡裝備掉落

每次成功 Clear：

- 保證 1 件 Equipment Reward
- Prototype 尚未做飾品時：
  - Weapon：55%
  - Suit：45%

星級決定品質池。

### 1 Star
- 白：75%
- 綠：25%

### 2 Star
- 綠：75%
- 藍：25%

### 3 Star
- 綠：55%
- 藍：35%
- 紫：10%

永遠不包含金色。

Boss 關第一版仍只保證 1 件裝備；未來可追加 Boss Exclusive Drop。

---

# 32. 戰鬥掉落

Enemy Death 可以掉：

- HP Pickup
- SP Pickup
- S Stone
- M Stone
- L Stone
- XL Stone
- 勾玉

---

## 32.1 HP / SP Pickup

Non-Boss Base Drop：

- HP Pickup：5%
- SP Pickup：4%

效果：

- HP：立即回 MaxHP ×15%
- SP：立即回 MaxSP ×20%

Suit Restore Trait：

- 上述效果 ×1.25

不進背包。

---

## 32.2 強化石掉率

每個 Non-Boss Enemy 獨立 Roll：

- S：4.00%
- M：1.50%
- L：0.50%
- XL：0.15%

一場 100 隻怪的基礎期望：

- S：約 4
- M：約 1.5
- L：約 0.5
- XL：約 0.15

重型怪：

- 強化石掉率 ×1.5

Boss：

- 額外保證掉 1 顆 Stone
- Boss Stone Quality：
  - S 45%
  - M 30%
  - L 20%
  - XL 5%

所有數值放 DropTable ScriptableObject。

---

# 33. 勾玉掉率

目標：

- 平均約每關 1 顆上下
- 仍然讓單次掉落感覺稀有

Non-Boss：

- 每隻 0.9%

普通 100 怪關：

- 期望約 0.9 顆 / 場

Boss：

- Boss 自己 10% 額外勾玉率

Boss Stage：

- 99 Non-Boss × 0.9% + Boss 10%
- 期望約 0.99 顆 / 場

勾玉可：

- 戰鬥低機率取得
- 付費購買

用途：

- 金色裝備
- 第二次復活

---

# 34. 金裝商城

商城只賣：

- 金色 Weapon
- 金色 Suit
- 未來金色 Accessory

貨幣：

- 勾玉

第一版金裝價格：

| Req Lv | 勾玉 |
|---|---:|
| 1 | 30 |
| 10 | 50 |
| 20 | 75 |
| 30 | 105 |
| 40 | 140 |
| 50 | 180 |

Prototype 必須把價格資料化，正式營運前再依留存 / 取得速度調整。

---

# 35. 掉落物存在與吸附

所有掉落物：

- 永久存在直到：
  - 玩家撿取
  - 關卡成功結束
  - 關卡失敗

不做時間消失。

Pickup：

- 進入 Magnet Radius 自動吸向玩家
- 不需要按按鈕

建議：

- Normal Pickup Magnet Radius：2.0m
- L / XL / 勾玉：3.0m

大量掉落物必須 Object Pool。

不要讓每個 Pickup 都有昂貴 Update。

---

# 36. Pending Reward

永久性戰鬥掉落：

- 強化石
- 勾玉

戰鬥中只寫到：

`PendingBattleReward`

成功破關：

- Commit 到 Player Save

關卡失敗：

- 全部丟棄

目的：

玩家如果已經撿到 XL / 勾玉，死亡時會更有復活動機。

---

# 37. 死亡 / 復活

每場最多復活 2 次。

## 第一次死亡

玩家可：

- 看 Rewarded Ad → 免費復活

如果玩家有永久去廣告：

- 不用看廣告
- 直接享有本場第一次免費復活

復活：

- HP = MaxHP ×50%
- SP = MaxSP ×50%
- 原地或安全點附近復活
- 復活後 2 秒無敵
- 敵人不清除
- Wave Timer 不重置
- 建議復活選安全 NavMesh Point / 離最近敵人至少 2.5m

---

## 第二次死亡

- 消耗 1 勾玉復活
- HP 50%
- SP 50%
- 2 秒無敵

---

## 第三次死亡

- 不允許復活
- Stage Failed
- Pending Reward 全部清除
- 本場不給 Clear EXP / Gold / Equipment

---

# 38. Pause

Pause Menu：

- Resume
- Settings
- Quit Stage

Quit Stage 視同 Failed：

- Pending Reward 清除
- 無 Clear Reward

---

# 39. Damage / Combat Formula

## Player Attack

`WeaponRoll = Random(MinDamage, MaxDamage)`

`Attack = WeaponRoll × (1 + EnhanceLv ×0.03) × (1 + PermanentAttackLv ×0.02)`

如果 Skill：

`Attack × SkillMultiplier`

如果 Crit：

`×1.5`

如果 Target = Boss 且有 Boss Trait：

`×1.12`

最後四捨五入，最低 1。

---

## Player Incoming

`Damage = EnemyRawDamage × 100/(100+SuitDEF) × (1 - PermanentDefenseLv ×0.01)`

最後四捨五入，最低 1。

---

# 40. Story / Stage Before Battle

每個 Story Stage 開始前：

- 必須有劇情段落
- 第一版可使用：
  - Character Portrait
  - Name
  - Dialogue Box
  - Tap to Continue
  - Skip

劇情資料不要硬寫在 Scene。

使用 StoryData / DialogueNode 資料化。

---

# 41. Boss 設計規則

不是每關有 Boss。

每 3 關 Boss。

Boss：

- 第 1～3 下普通普攻不可擊退；蓄力、突進與第 4 下落地可使 Boss 倒地約 2 秒。
- 擁有至少 3 個 Attack Pattern
- Attack 必須有明顯 Windup
- 重招可中斷玩家 Charge
- Boss Damage 必須走正常 Damage System
- Boss 不是碰到玩家就扣血

第一個 Boss Prototype：

### Attack A — Front Heavy Slash
- Windup 0.8s
- 前方 120°
- 高傷害

### Attack B — 360 Smash
- Windup 1.1s
- 地面警示
- 360° AoE

### Attack C — Charge
- Windup 0.7s
- 鎖定方向
- 直線衝撞

Boss HP 50% 以下：

- Attack Cooldown -15%
- 不額外無限召怪，因 Wave 10 本身已有 9 小怪

---

# 42. 極限模式（第二階段開發）

首頁可以進入極限模式。

特性：

- 無劇情
- 直接從 Layer 1
- 持續往上
- 每層敵人更強
- 每 5 層 Boss
- 每 10 層 Big Boss
- 戰鬥途中只掉 HP / SP
- **不掉強化石**
- **不掉勾玉**
- 玩家死亡後才一次總結並發獎

第一版預留參數：

- 每層 50 Kill
- 每層約 5 Wave ×10
- HP 每層 +8%
- Damage 每層 +6%
- 每 5 Layer Boss
- 每 10 Layer Big Boss

此模式不是 Prototype 第一優先，先建立 Interface / Config，不需要先完成內容。

---

# 43. Unity Architecture

建議目錄：

```text
Assets/
  Game/
    Art/
    Audio/
    Data/
      Characters/
      Weapons/
      Suits/
      Skills/
      Enemies/
      Bosses/
      Stages/
      Drops/
      Balance/
    Prefabs/
      Player/
      Enemies/
      Bosses/
      Pickups/
      VFX/
      UI/
    Scenes/
      Boot
      MainMenu
      StoryMap
      Battle
    Scripts/
      Core/
      Input/
      Player/
      Combat/
      AI/
      Stage/
      Progression/
      Equipment/
      Enhancement/
      Drops/
      UI/
      Save/
      Monetization/
```

---

# 44. 建議主要類別

## Core
- GameBootstrap
- GameStateManager
- TimeService
- SaveService

## Input
- TouchInputController
- GestureResolver
- GameplayInputBlocker

## Player
- PlayerController
- PlayerMovement
- PlayerCombat
- PlayerChargeAttack
- PlayerDodge
- PlayerSwipeAttack
- PlayerSkillController
- PlayerStats
- PlayerHealth
- PlayerSP

## Combat
- IDamageable
- DamageData
- DamageCalculator
- HitDetector
- KnockbackReceiver
- AttackInstance
- StatusEffectController

## Enemy
- EnemyController
- EnemyStateMachine
- EnemyMovement
- EnemyCombat
- EnemyHealth
- EnemyRangedCombat
- BossController

## Stage
- StageManager
- WaveManager
- SpawnManager
- StageTimer
- StageStarEvaluator

## Drop
- DropManager
- DropTable
- PickupController
- PickupMagnet
- PendingBattleReward

## Progression
- PlayerLevelSystem
- ExpService
- GoldService
- PermanentUpgradeSystem

## Equipment
- EquipmentInventory
- EquipmentLoadout
- WeaponRuntime
- SuitRuntime
- EquipmentTraitResolver

## Enhancement
- WeaponEnhancementService
- EnhancementResult
- EnhancementUI

## UI
- BattleHUD
- HPBar
- SPBar
- SkillButton
- PauseUI
- ResultUI
- RevivalUI
- EquipmentUI
- SkillUI
- UpgradeUI
- ShopUI

## Monetization abstraction
- IRewardedAdService
- IIAPService
- AdFreeEntitlement

第一版可做 Mock Service，不需先綁正式 SDK。

---

# 45. ScriptableObject Data

至少建立：

- PlayerBaseStatsData
- WeaponData
- WeaponTierData
- SuitData
- EquipmentRarityData
- SkillData
- EnemyData
- BossData
- StageData
- WaveData
- DropTableData
- PermanentUpgradeData
- EnhancementStoneData
- ShopItemData
- ExpCurveData
- GoldCurveData

所有平衡數值必須從 Data 讀取。

---

# 46. Object Pool 必須從第一版就做

因為前一波怪不清空會繼續累積。

必須 Pool：

- Enemy
- Projectile
- Damage Number
- Pickup
- VFX
- Slash Effect

不要在戰鬥中頻繁 `Instantiate / Destroy`。

---

# 47. AI / Performance 規則

- Player Target Scan 不要每 Frame 掃描。
- Target Scan Interval：0.10s。
- CurrentTarget 尚存活且仍在合理距離內，不頻繁切換。
- 遠處 Enemy AI 可以降低 Tick Rate。
- 不使用 `FindObjectsOfType` 做每幀索敵。
- Enemy Navigation 可用 NavMesh / 自訂簡化移動，但需避免大量 Agent 高成本。
- Prototype 先測同場 50 Enemy 穩定運行。
- 目標再測 80～100 Enemy 殘留極端案例。

---

# 48. Save Data

至少儲存：

- Player Level
- Current EXP
- Gold
- Magatama
- S/M/L/XL Stone 數量
- Permanent Upgrade Levels
- Owned Equipment
- Equipped Weapon
- Equipped Suit
- Accessory Slots
- Weapon Enhancement State
  - EnhanceLevel
  - CurrentEnergy
  - AttemptsUsed
- Owned Skills
- Skill Levels
- Equipped 4 Skills
- Story Stage Progress
- Best Stars per Stage
- AdFree Entitlement

Battle Pending Reward 不直接寫永久 Save，只有 Clear Commit。

---

# 48.1 Prototype 視覺驗收補充

除了功能可玩，第一版還必須達到下列最低視覺要求：

- [ ] Player 不是單純膠囊 / 正方形占位，而是至少有基本人形外觀與刀類武器。
- [ ] Normal / Speed / Ranged / Heavy Enemy 不是同一模型只換顏色或衣服，必須有不同輪廓與不同武器。
- [ ] Boss 有獨立外觀，體型大於普通敵人。
- [ ] 戰鬥 HUD 已有基本 UI 樣式（HP / SP / Skill Buttons / Pause），不是只有裸文字。
- [ ] 技能 Icon、強化石 Icon、金幣 / 勾玉 Icon、裝備卡片有基本 2D 圖示或 placeholder sprite。
- [ ] 關卡結算、裝備頁、強化頁至少有基本 UI 框體與版面。

# 49. Prototype 驗收條件

第一個可玩版本必須完成：

## Battle Core
- [ ] Landscape 固定 Camera
- [ ] 單點持續移動
- [ ] 雙擊 Dodge
- [ ] Swipe Attack
- [ ] Charge Attack 3 階
- [ ] 自動普攻
- [ ] 120° AoE
- [ ] 普攻 Knockback
- [ ] Enemy Body Touch 不扣血
- [ ] Enemy Attack 有 Windup / Hit / Recovery / Cooldown
- [ ] Player Hit Count 正確

## Enemies
- [ ] Normal
- [ ] Speed
- [ ] Ranged
- [ ] Heavy
- [ ] Boss Prototype

## Stage
- [ ] 10 Wave
- [ ] 15 秒強制 Spawn
- [ ] 殘怪不消失
- [ ] Boss Wave = 9 Enemy + 1 Boss
- [ ] 100 Enemy Clear
- [ ] 1/2/3 Star

## Player Progression
- [ ] EXP / Lv1～50
- [ ] Gold
- [ ] 5 Permanent Stats
- [ ] Skill Unlock
- [ ] 4 Skill Slots

## Equipment
- [ ] Weapon
- [ ] Suit
- [ ] White/Green/Blue/Purple/Gold
- [ ] Traits
- [ ] +0～+10
- [ ] 5 Enhancement Attempts

## Drop
- [ ] HP/SP
- [ ] S/M/L/XL
- [ ] Magatama
- [ ] Pickup Magnet
- [ ] Pending Reward

## Death
- [ ] First Rewarded Ad / Ad-Free revive
- [ ] Second 1 Magatama revive
- [ ] Third death fail
- [ ] 50% HP/SP
- [ ] 2s invulnerability
- [ ] Failed stage loses Pending Reward

---

# 50. 第一輪平衡測試要記錄的 Telemetry

即使 Prototype 不上後端，也應在 Editor Log / Debug UI 記錄：

每場：

- Stage ID
- Clear / Fail
- Clear Time
- DamageTakenCount
- Death Count
- Revive Count
- 每 Wave 結束時場上殘怪數
- Boss Spawn 時場上殘怪數
- Boss Kill Time
- Normal Attack 平均每隻怪需要 Hit 數
- Dodge 使用次數
- Swipe 使用次數
- Charge 使用次數 / 被打斷次數
- SP 低於 20% 的時間比例
- HP Pickup 數
- SP Pickup 數
- S/M/L/XL Drop 數
- Magatama Drop 數
- 最終 Star
- Equipment Rarity

這些數據用來後續調：

- 180 秒是否合理
- 三星 <=3 Hit 是否過難
- 每波 15 秒是否太快 / 太慢
- 敵人 HP
- Boss HP
- SP 消耗
- 強化石 / 勾玉掉率

---

# 51. MVP 開發原則

第一個 Milestone 不需要：

- 正式角色美術
- 正式劇情大量內容
- 招募系統
- 飾品內容
- 極限模式完整內容
- 正式廣告 SDK
- 正式 IAP SDK

第一個 Milestone 只需要證明：

> 「玩家使用點擊 / 雙擊 / 滑動 / 長按控制角色，在固定俯視 2.5D 場景中，自動用扇形普攻擊退並消滅一波波敵人；敵人不靠碰撞傷害，而是有真正攻擊節奏；玩家能在 10 Wave、100 Enemy、Boss Wave 中完成關卡並獲得星級與養成獎勵。」

如果這個核心不好玩，不要先擴大量內容。

---

# 52. Codex 第一個實作任務

先建立一個 `BattlePrototype` Scene：

1. 灰色平面地圖。
2. Capsule Player。
3. 一般刀 Placeholder。
4. 每波 10 個出生位置分布於當前畫面外四周，需檢查地圖邊界；不固定於整張地圖外緣。
5. Normal Enemy Capsule。
6. Player：
   - 單點移動
   - 自動索敵
   - 120° / 3m 普攻與第四段落地 360° 攻擊
   - 9～11 Damage
   - 0.4m Knockback
7. Enemy：
   - 20 HP
   - 接近 Player
   - 進 AttackRange 後不靠碰撞扣血
   - Windup → Hit → Recovery → Cooldown
8. 先做 Wave 1：
   - 10 Normal Enemies
9. 確認：
   - 普攻可以同時打中扇形內多名敵人
   - 同一刀同一敵人只受一次傷
   - Enemy 被擊退
   - 玩家與 Enemy 身體接觸不直接扣血
10. 完成後再依本文件逐步加入 10 Wave、Gesture、SP、Skill、Boss、Progression。

不要一次把所有系統塞進單一 MonoBehaviour。
必須保持模組化、資料化、可測試、可調參數。

---

# 53. 後續平衡原則

目前所有數值都是 First Balance Pass。

優先保留的是「規則」而不是「數字」。

尤其以下數值允許在實機測試後調整：

- Enemy HP / Damage
- 15 秒 Wave Interval
- 180 秒星級時間
- 三星 Hit <=3
- SP 消耗 / Regen
- Skill Damage / CD
- Drop Rate
- Magatama Economy
- Gold Economy
- Enhancement Energy Range
- Gold Equipment Price

但不要任意改變以下核心設計：

- Fixed top-down 2.5D landscape
- 無虛擬搖桿
- 單點持續移動
- 雙擊 Dodge
- Swipe Attack
- 長按 Charge
- 自動 AoE 普攻
- Enemy Body Collision 不傷害
- Enemy 真正 Attack Hit 才扣血
- 15 秒固定 Wave，不等清完
- 殘怪累積
- Boss Wave 9 小怪 + 1 Boss
- Clear 才 Commit Permanent Drop
- White～Purple 關卡取得
- Gold Equipment 商城勾玉限定
- Weapon 最多 5 次 Enhance Attempt

## 本輪操作數值修訂

雙擊閃避與滑動突進的霸體均指全程無敵，與一般技能的抗打斷霸體區分。普攻动作期間移速由 55% 降至 45%；初始長劍基礎擊退由 0.4m 調至 0.55m，實際位移受敵人抗性、衰減與身體碰撞影響。蓄力長度 6m、寬度 2.5m；玩家受擊後退 1m，倒地無敵與起身保護圈規則維持。

## 可調數值中文註解

參閱 [TUNING_GUIDE.md](TUNING_GUIDE.md)。GameBalance 每個欄位均有中文註解與 Inspector 提示；其他戰鬥公式、鏡頭、動畫、模型、介面及音效數值亦有「可調數值」搜尋標記。已有 GameBalance.asset 的值優先於 C# 初始化值。
