#  Backend

## 專案簡介

本專案是基於 ASP.NET Core 和 Redis 的後端應用程式，主要功能包括影像分析與即時互動遊戲。使用 SignalR 進行即時通訊，Redis 用於數據快取和訊息傳遞。

## 技術架構

- **ASP.NET Core**: 提供 Web API 和即時通訊支援。
- **SignalR**: 實現即時雙向通訊。
- **Redis**: 使用 StackExchange.Redis 作為快取和訊息分發。

## 專案結構

```
├─Controllers
│   ├─ ImageAnalysisController.cs
│   └─ RedisController.cs
├─Data
│   └─ images
│       └─ glasses
├─Hubs
│   └─ GameHub.cs
├─Models
│   ├─ Room.cs
│   └─ User.cs
```

### Controllers

- **ImageAnalysisController.cs**: 負責影像分析處理。
- **RedisController.cs**: 管理 Redis 快取和資料操作。

### Hubs

- **GameHub.cs**: 使用 SignalR 進行即時遊戲互動。

### Models

- **Room.cs**: 定義房間模型。
- **User.cs**: 定義使用者模型。

## 安裝與執行

### 需求

- .NET 6 或以上
- Redis 伺服器（WSL 安裝）
- Visual Studio (建議)
- Microsoft.AspNetCore.SignalR 套件
- StackExchange.Redis 套件

### 步驟

1. 安裝 .NET SDK
   ```bash
   dotnet --version
   ```
2. 使用 WSL 安裝 Redis
   ```bash
   # 更新軟體包
   sudo apt update
   # 安裝 Redis
   sudo apt install redis-server
   # 啟動 Redis 服務
   sudo service redis-server start
   redis-cli
   # 驗證 Redis 是否正在運行
   ping
   ```
3. 安裝必要套件
   ```bash
   dotnet add package Microsoft.AspNetCore.SignalR
   dotnet add package StackExchange.Redis
   ```
4. 使用 Visual Studio 打開專案
5. 執行專案

## Redis cmd

```bash
KEYS *                         # 列出所有 keys（不建議在生產環境使用）
KEYS user:*                    # 篩選 keys（例如以 user: 開頭的 key）
DEL mykey                      # 刪除 key
```

```bash
SET mykey "hello"              # 設定值
GET mykey                      # 取得值
```

```bash
FLUSHDB                       # 清除目前資料庫所有資料
FLUSHALL                      # 清除所有資料庫
DBSIZE                        # 當前資料庫 key 數量
INFO                          # 顯示伺服器資訊
```

## 設定

請確保在 `appsettings.json` 中正確配置 Redis 連線字串：

```json
"Redis": {
    "ConnectionString": "localhost:6379"
}
```

