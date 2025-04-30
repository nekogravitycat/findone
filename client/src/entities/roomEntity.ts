import type { UserEntity } from "./userEntity"

export interface RoomEntity {
  roomId: string
  hostUserId: string
  createdAt: Date
  round: number
  currentRound?: number
  timeLimit: number
  endTime?: Date
  status: RoomStatus
  targets: RoomTargetEntity[]
  userIds: Set<string>
  roomSubmits: RoomSubmitEntity[][]
}

export interface RoomTargetEntity {
  id: string
  roomId: string
  targetName: string
}

export interface RoomSubmitEntity {
  dateTime: Date
  userId: string
}

export interface RoomCreateResultEntity {
  roomId: string
  user: UserEntity
}

export interface RoomJoinResultEntity {
  roomId: string
  user: UserEntity
}

export type RoomStatus = "Waiting" | "InProgress" | "Finished"
