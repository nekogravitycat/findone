# SignalR README

## 測試指南

### 使用 Postman 測試 SignalR

> WARNING: 請在每個 request 後面加上 [Record Separator](https://symbl.cc/en/001E/)

請根據 [Postman Documents](https://warped-robot-79802.postman.co/workspace/My-Workspace~3161c694-30f3-4b7e-8bb0-23d06a01cf20/ws-raw-request/67ffb3afac0e77435e200472) 進行測試 

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
- **回傳事件**: `UserNotFound`
  - 參數:
    - `userId`: string
    - `error message`: string

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
- **回傳事件**: `RoomNotFound`
  - 參數:
    - `roomId`: string
    - `error message`: string

#### 建立房間
- **方法名稱**: `CreateRoom`
- **參數**:
  - `userName`: string
  - `round`: number (回合數)
  - `timeLimit`: number (時間限制，秒)
- **回傳事件**: `RoomCreated`
  - 參數: `roomId`: string
  - 參數: `userId`: string
- **回傳事件**: `RoomCreatedFailed`
  - 參數:
    - `error message`: string

#### 加入房間

> 該房間開始遊戲後，便無法讓後來使用者加入

- **方法名稱**: `GameJoin`
- **參數**:
  - `roomId`: string
  - `userName`: string
- **回傳事件**: `GameJoined`
  - 參數: 
    - `roomId`: string
    - `user`: User object
- **回傳事件**: `GameJoinFailed`
  - 參數:
    - `error message`: string

#### 開始遊戲

> 僅需由 host 發出訊息

- **方法名稱**: `GameStart`
- **參數**:
  - `roomId`: string
  - `userId`: string
- **回傳事件**: `GameStarted` (發送給所有房間使用者)
  - 參數: 無
- **回傳事件**: `GameStartFailed`
  - 參數:
    - `error message`: string

### 3. 遊戲相關
#### 取得該輪題目和結束時間資訊

> 僅需由 host 發出訊息

1. 會根據 `round` 參數自動更新 `CurrentRound` 欄位

- **方法名稱**: `GetRound`
- **參數**:
  - `roomId`: string
  - `userId`: string
  - `roundIndex`: number
- **回傳事件**: `RoundInfo`
  - 參數: round: Round
- **回傳事件**: `RoundInfoFailed`
  - 參數:
    - `error message`: string

#### 取得該輪成果

> 僅需由 host 發出訊息

1. TotalRoundScore 為使用者的總得分
2. CurrentRoundScore 為使用者該輪得分
3. Base64Image 以及 Comment 僅有前三名擁有該屬性 (optional)
4. Round Index 是取決於該房間的 `CurrentRound` 欄位，若沒有先使用 `GetRound` endpoint 來取得題目，便無法取得該輪成果

- **方法名稱**: `GetRank`
- **參數**:
  - `roomId`: string
  - `userId`: string
- **回傳事件**: `RankInfo`
  - 參數: scores: List<Score>
- **回傳事件**: `RankFailed`
  - 參數:
    - `error message`: string

#### 提交圖片

> 目前限制圖片檔案大小為 5MB

- **方法名稱**: `SubmitImage`
- **參數**:
  - `userId`: string
  - `base64Image`: string
- **回傳事件**: `ImageAnalysisSuccessed`
  - 參數: 無
- **回傳事件**: `ImageAnalysisFailed`
  - 參數:
    - `error message`: string


## 注意事項
1. 確保在連接前已經正確配置了 SignalR 服務
2. 處理連接斷開和重連的情況
3. 適當處理錯誤情況
4. 注意訊息大小限制
5. 考慮網路延遲和連接狀態
6. 房間資料會在 Redis 中保存 2 小時
7. 用戶資料會在 Redis 中保存 2 小時
8. roundIndex 為 indexed-0