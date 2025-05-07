import type { RoomEntity, RoomJoinResultEntity } from "@/entities/roomEntity"
import type { RoundEntity } from "@/entities/roundEntity"
import type { ScoreEntity } from "@/entities/scoreEntity"
import type { UserEntity } from "@/entities/userEntity"
import { SignalRService } from "./signalr"

export class GameAPI extends SignalRService {
  /* ------------------------------ User / Room ----------------------------- */

  public getUser(userId: string) {
    return this.invokeWithResponse<UserEntity>(
      "GetUser",
      `UserFound:${userId}`,
      `UserNotFound:${userId}`,
      userId
    )
  }

  public getRoom(roomId: string) {
    return this.invokeWithResponse<RoomEntity>("GetRoom", "RoomFound", "RoomNotFound", roomId)
  }

  public createRoom(userName: string, round: number, timeLimitSec: number) {
    return this.invokeWithResponse<RoomJoinResultEntity>(
      "CreateRoom",
      "RoomCreated",
      "RoomCreatedFailed",
      userName,
      round,
      timeLimitSec
    )
  }

  // Cannot be called after the game starts.
  public joinRoom(roomId: string, userName: string) {
    return this.invokeWithResponse<RoomJoinResultEntity>(
      "GameJoin",
      "GameJoined",
      "GameJoinFailed",
      roomId,
      userName
    )
  }

  /* ----------------------------- Game control ----------------------------- */

  // Host-only: fire-and-forget.
  public async gameStartInvoke(roomId: string, userId: string): Promise<void> {
    if (!this.connection) return
    this.connection.invoke("GameStart", roomId, userId).catch((err) => {
      console.error("Error starting game:", err)
    })
  }

  // Fired for ALL players when game actually starts.
  public onGameStart(callback: () => void) {
    this.onEventOnceWithResult("GameStarted", "GameStartFailed", callback).catch(() => {})
  }

  /* ----------------------------- Round / Rank ----------------------------- */

  // Host-only: ask server to broadcast round info.
  public async getRoundInvoke(roomId: string, userId: string, roundIndex: number) {
    if (!this.connection) return
    this.connection.invoke("GetRound", roomId, userId, roundIndex).catch((err) => {
      console.error("Error getting round:", err)
    })
  }

  public onRoundInfo(callback: (round: RoundEntity) => void) {
    this.onEventOnceWithResult<RoundEntity>("RoundInfo", "RoundInfoFailed", callback).catch(
      () => {}
    )
  }

  // Host-only: ask server to broadcast rank info.
  public async getRankInvoke(roomId: string, userId: string) {
    if (!this.connection) return
    this.connection.invoke("GetRank", roomId, userId).catch((err) => {
      console.error("Error getting rank:", err)
    })
  }

  public onRankInfo(callback: (scores: ScoreEntity[]) => void) {
    this.onEventOnceWithResult<ScoreEntity[]>("RankInfo", "RankFailed", callback).catch(() => {})
  }

  /* ------------------------------ Image upload ---------------------------- */

  /**
   * Submit an image (Base64).
   * Returns `true` if the image passes analysis, `false` otherwise.
   * Throws on size overflow / connection not ready.
   */
  public async submitImage(roomId: string, userId: string, base64Image: string): Promise<boolean> {
    const MAX_SIZE_BYTES = 5 * 1024 * 1024 // 5 MB

    // Rough Base64 size check.
    const byteLength =
      Math.floor((base64Image.length * 3) / 4) -
      (base64Image.endsWith("==") ? 2 : base64Image.endsWith("=") ? 1 : 0)
    if (byteLength > MAX_SIZE_BYTES) throw new Error("Image exceeds 5 MB size limit.")

    try {
      await this.invokeWithResponse<void>(
        "SubmitImage",
        "ImageAnalysisSucceeded",
        "ImageAnalysisFailed",
        roomId,
        userId,
        base64Image
      )
      return true
    } catch {
      return false
    }
  }
}
