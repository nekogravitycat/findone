# SignalR README

## 測試指南

### 使用 Postman 測試 SignalR

> **警告**: 請在每個 request 後面加上 [Record Separator](https://symbl.cc/en/001E/)

1. 請根據 [Postman Documents](https://warped-robot-79802.postman.co/workspace/My-Workspace~3161c694-30f3-4b7e-8bb0-23d06a01cf20/ws-raw-request/67ffb3afac0e77435e200472) 進行測試。
2. 在測試時，確保在每個 request 之後加上 Record Separator。

---

## API Endpoints

### 1. 用戶相關

#### 獲取用戶資訊

- **方法名稱**: `GetUser`
- **參數**:
  - `userId`: string
- **回傳事件**:
  - `UserFound`:
    - 參數: `user`: User object
  - `UserNotFound`:
    - 參數: `userId`: string
    - 參數: `errorMessage`: string

### 2. 房間相關

#### 獲取房間資訊

- **方法名稱**: `GetRoom`
- **參數**:
  - `roomId`: string
- **回傳事件**:
  - `RoomFound`:
    - 參數: `room`: Room object
  - `RoomNotFound`:
    - 參數: `roomId`: string
    - 參數: `errorMessage`: string

#### 建立房間

- **方法名稱**: `CreateRoom`
- **參數**:
  - `userName`: string
  - `round`: number (回合數)
  - `timeLimitSec`: number (時間限制，單位秒)
- **回傳事件**:
  - `RoomCreated`:
    - 參數: `room`: Room
    - 參數: `user`: User
  - `RoomCreatedFailed`:
    - 參數: `errorMessage`: string

#### 加入房間

> 注意：該房間開始遊戲後，後來的使用者將無法加入。

- **方法名稱**: `GameJoin`
- **參數**:
  - `roomId`: string
  - `userName`: string
- **回傳事件**:
  - `GameJoined`:
    - 參數: `room`: Room
    - 參數: `user`: User
  - `GameJoinFailed`:
    - 參數:
      - `errorMessage`: string

#### 開始遊戲

> 僅能由 host 發出訊息。

- **方法名稱**: `GameStart`
- **參數**:
  - `roomId`: string
  - `userId`: string
- **回傳事件**:
  - `GameStarted` (發送給所有房間使用者):
    - 無參數
  - `GameStartFailed`:
    - 參數:
      - `errorMessage`: string

### 3. 遊戲相關

#### 取得該輪題目和結束時間資訊

> 僅能由 host 發出訊息。

1. 根據 `round` 參數自動更新 `CurrentRound` 欄位。

- **方法名稱**: `GetRound`
- **參數**:
  - `roomId`: string
  - `userId`: string
  - `roundIndex`: number
- **回傳事件**:
  - `RoundInfo`:
    - 參數: `round`: Round object
  - `RoundInfoFailed`:
    - 參數:
      - `errorMessage`: string

#### 取得該輪成果

> 僅能由 host 發出訊息。

1. `TotalRoundScore` 代表使用者的總得分。
2. `CurrentRoundScore` 代表使用者該輪得分。
3. `Base64Image` 和 `Comment` 只有前三名玩家擁有（可選）。
4. `roundIndex` 由房間的 `CurrentRound` 欄位決定，若沒有，需先使用 `GetRound` 端點來取得題目，才能取得該輪成果。

- **方法名稱**: `GetRank`
- **參數**:
  - `roomId`: string
  - `userId`: string
- **回傳事件**:
  - `RankInfo`:
    - 參數: `scores`: List of Score objects
  - `RankFailed`:
    - 參數:
      - `errorMessage`: string

#### 提交圖片

> 限制圖片檔案大小為 5MB。

- **方法名稱**: `SubmitImage`
- **參數**:
  - `userId`: string
  - `base64Image`: string
- **回傳事件**:
  - `ImageAnalysisSucceeded`:
    - 無參數
  - `ImageAnalysisFailed`:
    - 參數:
      - `errorMessage`: string

---

## 注意事項

1. 確保在連接前已經正確配置了 SignalR 服務。
2. 處理連接斷開和重連的情況。
3. 適當處理錯誤情況，並提供有用的錯誤訊息。
4. 留意訊息大小限制。
5. 考慮網路延遲和連接狀態，並適當實現超時機制。
6. 房間資料將在 Redis 中保存 2 小時。
7. 用戶資料將在 Redis 中保存 2 小時。
8. `roundIndex` 參數為 zero-based index（從 0 開始）。

---

| GameFlow | DataSchema |
|----------|------------|
| ![GameFlow](https://github.com/user-attachments/assets/03092c45-147f-4a85-b49a-f13e9bc58e59) | ![DataSchema](https://github.com/user-attachments/assets/7cb9b966-3b2e-4666-8f7d-2e29e8296965) |

