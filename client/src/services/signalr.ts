import type { RoomEntity } from "@/entities/roomEntity"
import type { UserEntity } from "@/entities/userEntity"
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr"

const baseUrl = "http://localhost:8080"

export class SignalRService {
  private connection: HubConnection | null = null
  private reconnecting = false

  constructor() {}

  public async start() {
    if (this.connection && this.connection.state === "Connected") {
      return this.connection
    }

    this.connection = new HubConnectionBuilder().withUrl(`${baseUrl}/gamehub`).build()

    // Auto reconnect
    this.connection.onclose(async () => {
      console.warn("SignalR connection closed, trying to reconnect...")
      await this.tryReconnect()
    })

    try {
      await this.connection.start()
      console.log("SignalR connected")
    } catch (err) {
      console.error("SignalR connection error:", err)
      this.connection = null
    }

    return this.connection
  }

  public async stop() {
    if (!this.connection) {
      return
    }

    try {
      await this.connection.stop()
      console.log("SignalR disconnected")
    } catch (err) {
      console.error("SignalR disconnection error:", err)
    }
  }

  private async tryReconnect() {
    if (this.reconnecting) {
      return
    }

    this.reconnecting = true

    while (this.connection) {
      try {
        await this.connection.start()
        console.log("SignalR reconnected")
        this.reconnecting = false
        return
      } catch (err) {
        console.warn("Reconnect failed, retrying in 3s...")
        await new Promise((res) => setTimeout(res, 3000))
      }
    }
  }

  /** General invoke & wait for response method */
  private invokeWithResponse<T = any>(
    methodName: string,
    sendPayload: any,
    successEvent: string,
    failureEvent: string,
    timeoutMs = 5000
  ): Promise<T> {
    if (!this.connection) {
      return Promise.reject(new Error("SignalR connection is not established"))
    }

    return new Promise((resolve, reject) => {
      let timeoutId: ReturnType<typeof setTimeout> | null = null

      const cleanup = () => {
        if (timeoutId) {
          clearTimeout(timeoutId)
          timeoutId = null
        }
        this.connection?.off(successEvent, successHandler)
        this.connection?.off(failureEvent, failureHandler)
      }

      const successHandler = (response: T) => {
        cleanup()
        resolve(response)
      }

      const failureHandler = (error: any) => {
        cleanup()
        reject(typeof error === "string" ? new Error(error) : error)
      }

      this.connection!.on(successEvent, successHandler)
      this.connection!.on(failureEvent, failureHandler)

      // Set timeout
      timeoutId = setTimeout(() => {
        cleanup()
        reject(new Error(`Timeout waiting for ${successEvent}/${failureEvent}`))
      }, timeoutMs)

      // Call server method
      this.connection!.invoke(methodName, sendPayload).catch((err) => {
        cleanup()
        reject(err)
      })
    })
  }

  // API methods

  public async getUser(userId: string, timeoutMs = 5000) {
    return this.invokeWithResponse<UserEntity>(
      "GetUser",
      userId,
      "UserFound",
      "UserNotFound",
      timeoutMs
    )
  }

  public async getRoom(roomId: string, timeoutMs = 5000) {
    return this.invokeWithResponse<RoomEntity>(
      "GetRoom",
      roomId,
      "RoomFound",
      "RoomNotFound",
      timeoutMs
    )
  }

  public async createRoom(userName: string, round: number, timeLimit: number, timeoutMs = 5000) {
    return this.invokeWithResponse(
      "CreateRoom",
      { userName, round, timeLimit },
      "RoomCreated",
      "RoomCreatedFailed",
      timeoutMs
    )
  }

  public async joinRoom(roomId: string, userName: string, timeoutMs = 5000) {
    return this.invokeWithResponse(
      "GameJoin",
      { roomId, userName },
      "GameJoined",
      "GameJoinFailed",
      timeoutMs
    )
  }

  public async startGame(roomId: string, userId: string, timeoutMs = 5000) {
    return this.invokeWithResponse(
      "GameStart",
      { roomId, userId },
      "GameStarted",
      "GameStartFailed",
      timeoutMs
    )
  }

  public async getRound(roomId: string, userId: string, roundIndex: number, timeoutMs = 5000) {
    return this.invokeWithResponse(
      "GetRound",
      { roomId, userId, roundIndex },
      "RoundInfo",
      "RoundInfoFailed",
      timeoutMs
    )
  }

  public async getRank(roomId: string, userId: string, timeoutMs = 5000) {
    return this.invokeWithResponse(
      "GetRank",
      { roomId, userId },
      "RankInfo",
      "RankFailed",
      timeoutMs
    )
  }

  public async submitImage(userId: string, base64Image: string, timeoutMs = 5000) {
    return this.invokeWithResponse(
      "SubmitImage",
      { userId, base64Image },
      "ImageAnalysisSuccessed",
      "ImageAnalysisFailed",
      timeoutMs
    )
  }
}
