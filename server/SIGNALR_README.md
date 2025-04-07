# SignalR README

## 測試指南

### 使用 Postman 測試 SignalR

> WARNING: 請在每個 request 後面加上 [Record Separator](https://symbl.cc/en/001E/)

#### 1. 建立連接
1. 開啟 Postman
2. 建立新的 WebSocket 請求
3. 輸入 SignalR 連接 URL：`ws://localhost:5432/gamehub`
4. 在連接訊息中輸入：
```json
{
    "protocol": "json",
    "version": 1
} 
```

#### 2. 建立用戶
```json
{
    "type": 1,
    "invocationId": "1",
    "target": "CreateUser",
    "arguments": [
        "userName"
    ]
}
```

#### 3. 建立房間
```json
{
    "type": 1,
    "invocationId": "2",
    "target": "CreateRoom",
    "arguments": [
        "userName",
        5,
        60
    ]
}
```

#### 4. 加入房間
```json
{
    "type": 1,
    "invocationId": "3",
    "target": "JoinRoom",
    "arguments": [
        "roomId",
        "userName"
    ]
}
```

#### 5. 開始遊戲
```json
{
    "type": 1,
    "invocationId": "4",
    "target": "StartGame",
    "arguments": [
        "roomId"
    ]
}
```

#### 6. 檢查答案
```json
{
    "type": 1,
    "invocationId": "5",
    "target": "CheckAnswer",
    "arguments": [
        "roomId",
        "userId",
        1,
        "base64Image"
    ]
}
```

#### 7. 發送訊息
```json
{
    "type": 1,
    "invocationId": "6",
    "target": "SendMessage",
    "arguments": [
        "userName",
        "message"
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
#### 建立用戶
- **方法名稱**: `CreateUser`
- **參數**:
  - `userName`: string
- **回傳事件**: `UserCreated`
  - 參數: `userId`: string

### 2. 房間相關
#### 建立房間
- **方法名稱**: `CreateRoom`
- **參數**:
  - `userName`: string
  - `round`: number (回合數)
  - `timeLimit`: number (時間限制，秒)
- **回傳事件**: `RoomCreated`
  - 參數: `roomId`: string

#### 加入房間
- **方法名稱**: `JoinRoom`
- **參數**:
  - `roomId`: string
  - `userName`: string
- **回傳事件**: `GameJoined`
  - 參數: 
    - `roomId`: string
    - `user`: User object

#### 開始遊戲
- **方法名稱**: `StartGame`
- **參數**:
  - `roomId`: string
- **回傳事件**: `GameStarted`
  - 參數:
    - `roomId`: string
    - `room`: Room object

### 3. 遊戲相關
#### 檢查答案
- **方法名稱**: `CheckAnswer`
- **參數**:
  - `roomId`: string
  - `userId`: string
  - `roundIndex`: number
  - `base64Image`: string
- **回傳事件**: 待補充

### 4. 聊天相關
#### 發送訊息
- **方法名稱**: `SendMessage`
- **參數**:
  - `user`: string
  - `message`: string
- **回傳事件**: `ReceiveMessage`
  - 參數:
    - `user`: string
    - `message`: string

## 事件回調

### 客戶端接收事件
1. `UserCreated`: 用戶建立成功
2. `RoomCreated`: 房間建立成功
3. `GameJoined`: 成功加入遊戲
4. `GameStarted`: 遊戲開始
5. `ReceiveMessage`: 接收新訊息
6. `Error`: 錯誤訊息

## 錯誤處理
```javascript
connection.onclose((error) => {
    console.error("連接關閉: " + error);
});

connection.onreconnecting((error) => {
    console.log("重新連接中...");
});

connection.onreconnected((connectionId) => {
    console.log("重新連接成功");
});
```

## 注意事項
1. 確保在連接前已經正確配置了 SignalR 服務
2. 處理連接斷開和重連的情況
3. 適當處理錯誤情況
4. 注意訊息大小限制
5. 考慮網路延遲和連接狀態
6. 房間資料會在 Redis 中保存 2 小時
7. 用戶資料會在 Redis 中保存 2 小時

## 範例代碼
```javascript
// 完整的連接示例
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/gamehub")
    .withAutomaticReconnect()
    .build();

// 註冊事件處理
connection.on("UserCreated", (userId) => {
    console.log("用戶建立成功: " + userId);
});

connection.on("RoomCreated", (roomId) => {
    console.log("房間建立成功: " + roomId);
});

connection.on("GameJoined", (roomId, user) => {
    console.log("加入遊戲成功: " + roomId);
});

connection.on("GameStarted", (roomId, room) => {
    console.log("遊戲開始: " + roomId);
});

connection.on("ReceiveMessage", (user, message) => {
    console.log(user + ": " + message);
});

connection.on("Error", (error) => {
    console.error("錯誤: " + error);
});

// 啟動連接
connection.start()
    .then(() => {
        console.log("連接成功");
        // 可以在這裡進行其他初始化操作
    })
    .catch((err) => {
        console.error("連接失敗: " + err);
    });
```
