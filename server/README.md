# FindOne Backend

## 技術架構

- **後端框架**: ASP.NET Core
- **即時通訊**: SignalR
- **快取與訊息佇列**: Redis
- **容器化**: Docker & Docker Compose

## 專案結構

```
├─server/
│   ├─Controllers/
│   │   └─RedisController.cs      # Redis 快取控制器，提供基本的快取操作 API
│   ├─Data/
│   │   └─targets.txt            # 遊戲目標物清單檔案
│   ├─Hubs/
│   │   └─GameHub.cs             # SignalR 遊戲中樞，處理即時通訊和遊戲邏輯
│   ├─Models/
│   │   ├─Room.cs                # 房間模型，定義遊戲房間的結構和狀態
│   │   ├─Image.cs               # 影像模型，處理影像相關的資料結構
│   │   ├─Round.cs               # 回合模型，定義遊戲回合的結構
│   │   ├─Score.cs               # 分數模型，處理玩家分數相關的資料結構
│   │   └─User.cs                # 使用者模型，定義玩家資料結構
│   ├─Services/
│   │   ├─GameService.cs         # 遊戲服務，處理核心遊戲邏輯
│   │   ├─GoogleAIService.cs     # Google AI 服務，整合 Google Gemini API 進行影像分析
│   │   ├─ImageService.cs        # 影像服務，處理影像分析和處理
│   │   ├─RoomService.cs         # 房間服務，管理遊戲房間的狀態和操作
│   │   ├─UserService.cs         # 使用者服務，處理玩家資料的管理
│   │   └─ScoreService.cs        # 分數服務，計算和管理玩家分數
│   ├─Models/
│       ├─IdGenerator.cs         # ID 生成器，用於生成唯一識別碼
│       └─ImageHelper.cs         # 影像輔助工具，處理影像格式轉換和驗證
├─Dockerfile                     # Docker 容器設定檔
├─docker-compose.yml             # Docker Compose 設定檔，定義服務編排
└─.dockerignore                  # Docker 忽略檔案設定
```

## 系統需求

- Docker Desktop
- .NET Core SDK (用於本地開發)

## How to Run?

> 推薦使用 Docker Compose

### Docker Compose

1. 確保已安裝並啟動 Docker Desktop
2. 在專案根目錄執行：
   ```bash
   docker-compose up -d
   ```
3. 服務將在以下端口運行：
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

2. 安裝依賴：
   ```bash
   cd server
   dotnet restore
   ```

3. 運行專案：
   ```bash
   dotnet run
   ```

## 授權

本專案採用 MIT 授權條款 - 詳見 [LICENSE](LICENSE) 文件