# 分享朋友測試與自動部署

## 分享方式

此專案使用 **Unity WebGL + GitHub Pages**。朋友打開網址即可在瀏覽器玩：

**https://doublewei0724.github.io/BladeSurvivor/**

這是網頁版，不需要先安裝 Unity 或 Mac `.app`。Unity 6 官方支援桌面 Chrome、Edge、Firefox、Safari，以及支援 WebGL 2 的 iOS Safari 15+／Android Chrome 58+；此專案仍需朋友實際回報手機的載入時間與效能。PWA 安裝圖示和離線快取不是目前上線條件。

## 每次更新

在根目錄 `BladeSurvivor` 執行：

```bash
git add -A
git commit -m "描述這次修改"
git push origin main
```

`.github/workflows/deploy-webgl.yml` 會在推送 `main` 後，用 Unity 6000.3.24f1 自動建置 WebGL，並將 `build/WebGL` 部署到 GitHub Pages。朋友使用同一網址，重新整理後即可玩新版。可在 repository 的 **Actions** 分頁確認建置與部署是否成功。**必須先完成下方 Unity 授權 Secrets 設定**；在此之前，網址顯示的是已發布的初版，推送原始碼不會更新遊戲。

本地的 `Builds/Blade Survivor.app` 與 Unity `Library` 不會被上傳。GitHub Pages 的 WebGL 遊戲以原始碼重新編譯，和 Mac `.app` 是不同平台的建置。

## 首次啟用 GitHub Actions

1. Repository 的 **Settings → Pages → Build and deployment → Source** 選 **GitHub Actions**（此項已設定）。
2. Repository 的 **Settings → Secrets and variables → Actions → New repository secret**，建立 Unity 授權資訊。不要把密碼或授權檔 commit 到 Git。
   - Unity Personal：`UNITY_LICENSE`（授權檔內容）、`UNITY_EMAIL`、`UNITY_PASSWORD`。
   - Unity Pro：`UNITY_SERIAL`、`UNITY_EMAIL`、`UNITY_PASSWORD`。
3. 到 **Actions → Build and deploy Unity WebGL** 執行一次 **Run workflow**，或再推一次 commit。
4. 待 `build` 和 `deploy` 都綠燈後，打開上方分享網址。

Unity Personal 的 CI 授權檔取得步驟依 GameCI 官方文件：[GameCI Activation](https://game.ci/docs/github/activation/)。Unity 帳號密碼只放在 GitHub Actions Secrets，勿傳給測試朋友，也不要放在聊天室或專案檔案。

若 Build 顯示缺少 Unity 授權，請先設定 Secrets；原始碼已推上 GitHub 不代表新版網頁建置已成功。初版由 `.github/workflows/publish-initial-webgl.yml` 發布一次；日後正常更新使用上述自動建置流程。若 Pages 顯示 404，檢查 Pages Source 是否為 GitHub Actions，再看 Actions 的 `deploy` 工作。

## 本機先建網頁版

本機需在 Unity Hub 的 **Installs → Unity 6000.3.24f1 → Add modules** 安裝 **WebGL Build Support**。完成後停止 Play Mode，在 Unity 選 **Blade Survivor → Build WebGL Playtest**。輸出位於根目錄 `build/WebGL`，此資料夾已加入 `.gitignore`。

WebGL 輸出要由 HTTP 伺服器載入，不能直接雙擊 `index.html` 的 `file://` 路徑。可用：

```bash
cd build/WebGL
python3 -m http.server 8000
```

再在瀏覽器開 `http://localhost:8000/`。停止伺服器按 `Ctrl+C`。

## 本機存檔與測試回饋

WebGL 存檔保存在朋友各自的瀏覽器儲存區，不會同步到你的 Mac 存檔，也不會上傳到 GitHub。請朋友回報裝置／瀏覽器、關卡、重現步驟與截圖；不要公開自己的帳號密碼。
