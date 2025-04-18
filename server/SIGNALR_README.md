# SignalR README

## 測試指南

### 使用 Postman 測試 SignalR

> WARNING: 請在每個 request 後面加上 [Record Separator](https://symbl.cc/en/001E/)

#### 1. 建立連接
1. 開啟 Postman
2. 建立新的 WebSocket 請求
3. 輸入 SignalR 連接 URL：`ws://localhost:5432/gamehub` (未來會拆成不同的 endpoint url)
4. 在連接訊息中輸入：
```json
{
    "protocol": "json",
    "version": 1
} 
```

#### 2. 獲取用戶資訊
```json
{
    "type": 1,
    "invocationId": "1",
    "target": "GetUser",
    "arguments": [
        "userId"
    ]
}
```

#### 3. 獲取房間資訊
```json
{
    "type": 1,
    "invocationId": "2",
    "target": "GetRoom",
    "arguments": [
        "roomId"
    ]
}
```

#### 4. 建立房間
```json
{
    "type": 1,
    "invocationId": "3",
    "target": "CreateRoom",
    "arguments": [
        "userName",
        5,
        60
    ]
}
```

#### 5. 加入房間
```json
{
    "type": 1,
    "invocationId": "4",
    "target": "GameJoin",
    "arguments": [
        "roomId",
        "userName"
    ]
}
```

#### 6. 開始遊戲 ** 尚未完成
```json
{
    "type": 1,
    "invocationId": "5",
    "target": "GameStart",
    "arguments": [
        "roomId"
    ]
}
```

#### 7. 提交圖片

> **base64Image 圖片大小僅支援至 5MB !**

```json
{
    "type": 1,
    "invocationId": "6",
    "target": "SubmitImage",
    "arguments": [
        "userId",
        "roundIndex",
        "base64Image"
    ]
}
```

### 訊息類型說明
- `type: 1` - 調用方法
- `type: 2` - 串流項目
- `type: 3` - 完成串流
- `type: 4` - 取消串流
- `type: 5` - Ping
- `type: 6` - Close

### 測試注意事項
1. 確保 `invocationId` 每次請求都是唯一的
2. 連接後需要等待服務器返回連接成功的訊息
3. 所有方法調用都需要包含 `type` 和 `invocationId`
4. 注意檢查服務器返回的錯誤訊息

### 常見錯誤訊息
```json
{
    "type": 7,
    "error": "錯誤訊息",
    "invocationId": "1"
}
```

## Endpoints

### 1. 用戶相關
#### 獲取用戶資訊
- **方法名稱**: `GetUser`
- **參數**:
  - `userId`: string
- **回傳事件**: `UserFound`
  - 參數: `user`: User object
- **回傳事件**: `UserNotFound`
  - 參數: `userId`: string
  - 參數: `error message`: string

### 2. 房間相關
#### 獲取房間資訊
- **方法名稱**: `GetRoom`
- **參數**:
  - `roomId`: string
- **回傳事件**: `RoomFound`
  - 參數: `room`: Room object
- **回傳事件**: `RoomNotFound`
  - 參數: `roomId`: string
  - 參數: `error message`: string

#### 建立房間
- **方法名稱**: `CreateRoom`
- **參數**:
  - `userName`: string
  - `round`: number (回合數)
  - `timeLimit`: number (時間限制，秒)
- **回傳事件**: `RoomCreated`
  - 參數: `roomId`: string
  - 參數: `userId`: string

#### 加入房間
- **方法名稱**: `GameJoin`
- **參數**:
  - `roomId`: string
  - `userName`: string
- **回傳事件**: `GameJoined`
  - 參數: 
    - `roomId`: string
    - `user`: User object

#### 開始遊戲
- **方法名稱**: `GameStart`
- **參數**:
  - `roomId`: string
- **回傳事件**: `GameStarted`
  - 參數:
    - `roomId`: string
    - `room`: Room object

### 3. 遊戲相關
#### 提交圖片
- **方法名稱**: `SubmitImage`
- **參數**:
  - `userId`: string
  - `roundIndex`: number
  - `base64Image`: string
- **回傳事件**: `ImageAnalysisSuccessed`
  - 參數: 無
- **回傳事件**: `ImageAnalysisFailed`
  - 參數:
    - `error message`: string

### 4. Testing
#### 發送訊息
- **方法名稱**: `SendMessage`
- **參數**:
  - `user`: string
  - `message`: string
- **回傳事件**: `ReceiveMessage`
  - 參數:
    - `user`: string
    - `message`: string

## 注意事項
1. 確保在連接前已經正確配置了 SignalR 服務
2. 處理連接斷開和重連的情況
3. 適當處理錯誤情況
4. 注意訊息大小限制
5. 考慮網路延遲和連接狀態
6. 房間資料會在 Redis 中保存 2 小時
7. 用戶資料會在 Redis 中保存 2 小時
8. roundIndex 為 indexed-0
