<div align="center">
<img src="./client/src/assets/findone-readme.png" alt="Findone Logo" style="width: 100px; height: 100px; border-radius: 50px; margin-bottom: 20px;" />
</div>

FindOne 是一款基於 ASP.NET Core 和 Vue3 的即時互動遊戲 Web App，結合了影像分析技術和多人線上功能。本專案使用 Vue3 和 TailwindCSS 實現介面和美術、使用 SignalR 實現即時通訊，並利用 Redis 進行數據快取。

## 技術架構

- **前端框架**: Vue3 + TypeScript
- **UI 框架**: TailwindCSS
- **狀態管理**: Pinia
- **後端框架**: ASP.NET Core
- **即時通訊**: SignalR
- **快取與訊息佇列**: Redis
- **容器化**: Docker & Docker Compose
- **影像辨識技術**: Google Gemini API

## 專案結構

```
├─client/                        # 前端 Vue3 專案
│   ├─src/
│   │   ├─assets/               # 靜態資源（圖片、樣式等）
│   │   ├─components/           # Vue 元件
│   │   ├─entities/             # TypeScript 型別定義
│   │   ├─services/             # API 服務
│   │   ├─stores/               # Pinia 狀態管理
│   │   ├─views/                # 頁面元件
│   │   ├─App.vue              # 根元件
│   │   └─main.ts              # 入口文件
│   ├─public/                   # 公共資源
│   └─package.json             # 前端依賴配置
│
├─server/                       # 後端 ASP.NET Core 專案
│   ├─Controllers/
│   │   └─RedisController.cs    # Redis 快取控制器
│   ├─Data/
│   │   └─targets.txt          # 遊戲目標物清單
│   ├─Hubs/
│   │   └─GameHub.cs           # SignalR 遊戲中樞
│   ├─Models/
│   │   ├─Room.cs              # 房間模型
│   │   ├─Image.cs             # 影像模型
│   │   ├─Round.cs             # 回合模型
│   │   ├─Score.cs             # 分數模型
│   │   └─User.cs              # 使用者模型
│   ├─Services/
│   │   ├─GameService.cs       # 遊戲服務
│   │   ├─GoogleAIService.cs   # Google AI 服務
│   │   ├─ImageService.cs      # 影像服務
│   │   ├─RoomService.cs       # 房間服務
│   │   ├─UserService.cs       # 使用者服務
│   │   └─ScoreService.cs      # 分數服務
│   └─Utils/
│       ├─IdGenerator.cs       # ID 生成器
│       └─ImageHelper.cs       # 影像輔助工具
│
├─Dockerfile                    # Docker 容器設定檔
├─docker-compose.yml           # Docker Compose 設定檔
└─.dockerignore               # Docker 忽略檔案設定
```

### 核心功能說明

#### 遊戲流程
1. 玩家創建或加入遊戲房間
2. 房主開始遊戲，系統隨機選擇目標物
3. 玩家在限定時間內拍攝符合目標物的照片
4. 系統使用 Google Gemini API 分析照片是否符合目標
5. 根據提交時間和準確度計算分數
6. 即時顯示排行榜和遊戲結果

#### 技術特點
- 使用 Vue3 + TypeScript 開發現代化前端介面
- 使用 TailwindCSS 實現響應式設計
- 使用 SignalR 實現即時通訊
- 整合 Google Gemini API 進行影像分析
- 使用 Redis 進行資料快取和狀態管理
- 支援多人同時遊戲
- 容器化部署，方便擴展

## 系統需求

- Docker Desktop
- Node.js 18+ (前端開發)
- .NET Core SDK 7.0+ (後端開發)

## How to Run?

> 推薦使用 Docker Compose

### Docker Compose

1. 確保已安裝並啟動 Docker Desktop
2. 在專案根目錄執行：
   ```bash
   docker-compose up -d
   ```
3. 服務將在以下端口運行：
   - 前端: http://localhost:5173
   - 後端 API: ws://localhost:8080/gamehub
   - Redis: localhost:6379
4. 關閉服務：
   ```bash
   docker-compose down -v
   ```

### 本地開發

1. 克隆專案：
   ```bash
   git clone [repository-url]
   cd findone
   ```

2. 前端開發：
   ```bash
   cd client
   pnpm install
   pnpm dev
   ```

3. 後端開發：
   ```bash
   cd server
   dotnet restore
   dotnet run
   ```

## 授權

本專案採用 MIT 授權條款 - 詳見 [LICENSE](LICENSE) 文件