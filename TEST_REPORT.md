# 驗證紀錄

## 大地圖與視角修訂

依後續使用者說明，改為固定斜俯視角、鏡頭跟隨角色的大地圖，取代原先螢幕範圍內的小競技場。
新增實際執行檢查通過：地圖中心、遠處與四個角落共 6 個位置均保持角色置中；60 個出生點均位於畫面外且在地圖內；角色可走出舊競技場範圍；復活留在原位置附近。
結果：[large-world.json](TestResults/large-world.json)。新畫面：[Battle.png](TestResults/Battle.png)。

環境：Unity 6000.3.24f1、macOS、內建渲染管線。

## 已完成

- Unity MCP：重新整理腳本、切換 Play Mode、執行選單、擷取畫面、執行測試與建置均成功。
- EditMode：**19 / 19 通過**，0 失敗。涵蓋傷害、扇形判定、星等界線、強化、結算重複防護、存檔讀回、技能配置與點擊／雙擊／滑動／長按／UI 阻擋。
- 執行中整合測試：**20 項通過**。驗證 10 波合計 100 隻（含 Boss）、舊波敵人保留、無敵與護盾、蓄力中斷、SP 門檻、暫停、通關結算、關卡解鎖、兩次復活與第三次死亡失敗。
- 壓力情境實際累積 **100 隻存活敵人**。此測試使用隔離存檔、停用自動普攻、測試無敵和加速時間；並非人工通關紀錄，也不是行動裝置 FPS 保證。
- 已檢視完整 1600×900 主選單、戰鬥與裝備畫面。
- macOS 建置結果 `Succeeded`、0 個建置錯誤。
- 獨立遊戲已成功啟動，輸出包含 x86_64／arm64 的 Universal Binary；啟動及戰鬥記錄未發現例外或錯誤。

## 檔案

- [EditMode 結果](TestResults/edit-mode.json)
- [執行中整合測試](TestResults/runtime-smoke.json)
- [主選單](TestResults/MainMenu.png)
- [戰鬥](TestResults/Battle.png)
- [裝備](TestResults/Equipment.png)

Unity Console 最後檢查沒有錯誤；有兩則 Metal memoryless depth surface 的 load/store 警告。
尚未驗收觸控真機、行動效能、所有關卡人工平衡與正式廣告／付款服務。

## 重跑

1. Unity Test Runner 執行 `BladeSurvivor.Tests` 的 EditMode 測試。
2. Play Mode 下選 `Blade Survivor > Testing > Run Runtime Smoke Test`。
3. 離開 Play Mode，再用 `Blade Survivor > Build macOS Playtest` 產出遊戲。

測試工具位於 Editor 組件，不會包含在玩家版本；測試進度儲存在專案 Temp 目錄的獨立檔案。

## 普攻與敵人間距修訂

初始長劍普攻 2 → 2.8 公尺、刀身 1.05 → 1.47 單位；角度維持 120°。加入所有存活狀態的身體分離。重疊於中心及牆角的站定敵人共 90 組配對距離檢查通過，並驗證新普攻距離與角度。結果：[melee-spacing.json](TestResults/melee-spacing.json)。

## 移動朝向修正

移動方向優先於自動鎖敵；僅在停下時允許自動轉向敵人。通過點擊立即面向、持續移動不被背後敵人轉向、前方目標優先於較近的背後目標、轉身離開不命中背後敵人，以及停下後恢復自動轉向攻擊的執行測試。結果：[movement-facing.json](TestResults/movement-facing.json)。

## 受擊倒地與起身

普攻距離 3、間隔 0.6 秒、前搖 0.15 秒。執行測試通過：0.65 公尺後退、倒地期間操作鎖定、1.5 公尺敵人體積隔離、重複受擊不延長兩秒起身時間、仍承受傷害、起身後移除隔離。結果：[knockdown.json](TestResults/knockdown.json)。

## 無敵閃爍與四連擊（覆蓋上一版倒地可受傷規則）

倒地期間完全忽略傷害及受擊計數。新增半透明閃爍專用材質，站起恢复正常。執行測試通過 1–4 段連擊、第四段落地命中身後敵人、錯過接續節奏重置、受擊打斷重置、倒地不受任何後續傷害；兩秒起身與隔離解除測試再次通過。結果：[combo.json](TestResults/combo.json)。

## 拖曳瞄準與敵人倒地

EditMode 更新為 21 / 21 通過，新增長按拖曳與旋轉矩形邊界檢查。實際執行確認蓄力方向不被移動覆蓋、矩形內外命中區別、敵人倒地後站起、第四段擊倒與新版跳躍／保護圈尺寸。結果：[aimed-charge.json](TestResults/aimed-charge.json)。

## 閃避與突進無敵修訂

驗證閃避起始及結束前全程無敵、突進無敵並擊倒敵人、動作結束恢復傷害判定，以及蓄力 6m／玩家後退 1m／普攻移速 45%／長劍基礎擊退 0.55m 的載入數值。結果：[dash-protection.json](TestResults/dash-protection.json)。

## 第四擊無敵及位移修訂

新增執行驗證：第四击起跳與空中傷害無效、落地正常傷害、落地後保護解除，以及閃避／突進6m位移與突進2m寬度。見 [finisher-protection.json](TestResults/finisher-protection.json)。GameBalance 97 個欄位已補中文用途／單位／生效提示，其他可調常數依來源補上中文搜尋註解及 TUNING_GUIDE 索引。
