import type { RoomEntity, RoomJoinResultEntity } from "@/entities/roomEntity"
import type { RoundEntity } from "@/entities/roundEntity"
import type { ScoreEntity } from "@/entities/scoreEntity"
import type { UserEntity } from "@/entities/userEntity"
import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr"

const baseUrl = "http://localhost:8080"

export class SignalRService {
  private readonly timeoutMs = 5000
  private connection: HubConnection | null = null

  public async start() {
    if (this.connection && this.connection.state === "Connected") {
      return this.connection
    }

    this.connection = new HubConnectionBuilder()
      .withUrl(`${baseUrl}/gamehub`, { withCredentials: true })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build()

    try {
      await this.connection.start()
      console.log("SignalR connected")
    } catch (err) {
      console.error("SignalR connection error:", err)
      this.connection = null
      throw err
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

  // General invoke & wait for response method
  private invokeWithResponse<T = any>(
    methodName: string,
    successEvent: string,
    failureEvent: string,
    ...sendPayload: any[]
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
      }, this.timeoutMs)

      // Call server method
      this.connection!.invoke(methodName, ...sendPayload).catch((err) => {
        cleanup()
        reject(err)
      })
    })
  }

  // API methods

  public async getUser(userId: string) {
    return this.invokeWithResponse<UserEntity>(
      "GetUser",
      `UserFound:${userId}`,
      `UserNotFound:${userId}`,
      userId
    )
  }

  public async getRoom(roomId: string) {
    return this.invokeWithResponse<RoomEntity>("GetRoom", "RoomFound", "RoomNotFound", roomId)
  }

  public async createRoom(userName: string, round: number, timeLimitSec: number) {
    return this.invokeWithResponse<RoomJoinResultEntity>(
      "CreateRoom",
      "RoomCreated",
      "RoomCreatedFailed",
      ...[userName, round, timeLimitSec]
    )
  }

  // Cannot be called after the game starts
  public async joinRoom(roomId: string, userName: string) {
    return this.invokeWithResponse<RoomJoinResultEntity>(
      "GameJoin",
      "GameJoined",
      "GameJoinFailed",
      ...[roomId, userName]
    )
  }

  // Only for game host to call, all players will receive the info
  public async startGame(roomId: string, userId: string) {
    return this.invokeWithResponse<null>(
      "GameStart",
      "GameStarted",
      "GameStartFailed",
      ...[roomId, userId]
    )
  }

  // Only for game host to call, all players will receive the info
  public async getRound(roomId: string, userId: string, roundIndex: number) {
    return this.invokeWithResponse<RoundEntity>(
      "GetRound",
      "RoundInfo",
      "RoundInfoFailed",
      ...[roomId, userId, roundIndex]
    )
  }

  // Only for game host to call, all players will receive the info
  public async getRank(roomId: string, userId: string) {
    return this.invokeWithResponse<ScoreEntity[]>(
      "GetRank",
      "RankInfo",
      "RankFailed",
      ...[roomId, userId]
    )
  }

  // Image size limit: 5MB
  public async submitImage(userId: string, base64Image: string) {
    return this.invokeWithResponse<null>(
      "SubmitImage",
      "ImageAnalysisSuccessed",
      "ImageAnalysisFailed",
      ...[userId, base64Image]
    )
  }

  // Listen to a SignalR event once, then automatically unsubscribe
  public onEventOnce<T = void>(eventName: string, callback: (data: T) => void) {
    if (!this.connection) {
      return
    }

    const handler = (data: T) => {
      this.connection?.off(eventName, handler)
      callback(data)
    }

    this.connection.on(eventName, handler)
  }
}
