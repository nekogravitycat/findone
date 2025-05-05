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
        const errorMessage = error as { errorMessage?: string }
        const normalizedError = new Error(errorMessage.errorMessage || error.toString())
        reject(normalizedError)
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

  // General onEvent method with success and failure events and optional callbacks
  private onEventOnceWithResult<T = any>(
    successEvent: string,
    failureEvent: string,
    onSuccess?: (data: T) => void,
    onFailure?: (error: Error) => void
  ): Promise<T> {
    return new Promise((resolve, reject) => {
      if (!this.connection) {
        const err = new Error("SignalR connection is not established")
        onFailure?.(err)
        reject(err)
        return
      }

      const cleanup = () => {
        this.connection?.off(successEvent, successHandler)
        this.connection?.off(failureEvent, failureHandler)
      }

      const successHandler = (data: T) => {
        cleanup()
        onSuccess?.(data)
        resolve(data)
      }

      const failureHandler = (data: T) => {
        cleanup()
        const errorMessage = data as { errorMessage?: string }
        const normalizedError = new Error(errorMessage.errorMessage || "An unknown error occurred")
        onFailure?.(normalizedError)
        reject(normalizedError)
      }

      this.connection.on(successEvent, successHandler)
      this.connection.on(failureEvent, failureHandler)
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
    return this.invokeWithResponse<RoomEntity>("GetRoom", `RoomFound`, `RoomNotFound`, roomId)
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

  /*
  // Only for game host to call, all players will receive the info
  public async startGame(roomId: string, userId: string) {
    return this.invokeWithResponse<null>(
      "GameStart",
      "GameStarted",
      "GameStartFailed",
      ...[roomId, userId]
    )
  }
  */

  // Only for game host to call, all players will receive the info
  // Invoke GameStart without waiting for response
  public async gameStartInvoke(roomId: string, userId: string) {
    if (!this.connection) {
      return
    }

    this.connection.invoke("GameStart", roomId, userId).catch((err) => {
      console.error("Error starting game:", err)
    })
  }

  // Listen to game start event, automatically unsubscribe
  public onGameStart(callback: () => any) {
    if (!this.connection) {
      return
    }
    this.onEventOnceWithResult("GameStarted", "GameStartFailed", callback)
  }

  /*
  // Only for game host to call, all players will receive the info
  public async getRound(roomId: string, userId: string, roundIndex: number) {
    return this.invokeWithResponse<RoundEntity>(
      "GetRound",
      "RoundInfo",
      "RoundInfoFailed",
      ...[roomId, userId, roundIndex]
    )
  }
  */

  // Only for game host to call, all players will receive the info
  // Invoke getRound without waiting for response
  public async getRoundInvoke(roomId: string, userId: string, roundIndex: number) {
    if (!this.connection) {
      return
    }
    this.connection.invoke("GetRound", roomId, userId, roundIndex).catch((err) => {
      console.error("Error getting round:", err)
    })
  }

  // Listen to round info event, automatically unsubscribe
  public onRoundInfo(callback: (roundInfo: RoundEntity) => any) {
    if (!this.connection) {
      return
    }
    this.onEventOnceWithResult("RoundInfo", "RoundInfoFailed", callback)
  }

  /*
  // Only for game host to call, all players will receive the info
  public async getRank(roomId: string, userId: string) {
    return this.invokeWithResponse<ScoreEntity[]>(
      "GetRank",
      "RankInfo",
      "RankFailed",
      ...[roomId, userId]
    )
  }
  */

  // Only for game host to call, all players will receive the info
  // Invoke getRank without waiting for response
  public async getRankInvoke(roomId: string, userId: string) {
    if (!this.connection) {
      return
    }
    this.connection.invoke("GetRank", roomId, userId).catch((err) => {
      console.error("Error getting rank:", err)
    })
  }

  // Listen to rank info event, automatically unsubscribe
  public onRankInfo(callback: (scores: ScoreEntity[]) => any) {
    if (!this.connection) {
      return
    }
    this.onEventOnceWithResult("RankInfo", "RankFailed", callback)
  }

  // Image size limit: 5MB
  // return true if the image is valid, false otherwise
  public async submitImage(roomId: string, userId: string, base64Image: string) {
    const maxSizeBytes = 5 * 1024 * 1024 // 5MB

    const base64Length = base64Image.length
    const byteLength =
      Math.floor((base64Length * 3) / 4) -
      (base64Image.endsWith("==") ? 2 : base64Image.endsWith("=") ? 1 : 0)

    if (byteLength > maxSizeBytes) {
      throw new Error("Image exceeds 5MB size limit.")
    }

    try {
      await this.invokeWithResponse<null>(
        "SubmitImage",
        "ImageAnalysisSucceeded",
        "ImageAnalysisFailed",
        ...[roomId, userId, base64Image]
      )
      return true
    } catch (err) {
      return false
    }
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

  // Listen to a SignalR event, no auto-unsubscribe
  public onEvent<T = void>(eventName: string, callback: (data: T) => void) {
    if (!this.connection) {
      return
    }

    this.connection.on(eventName, callback)
  }

  // Unsubscribe from a SignalR event
  public offEvent(eventName: string, callback: (data: any) => void) {
    if (!this.connection) {
      return
    }

    this.connection.off(eventName, callback)
  }
}
