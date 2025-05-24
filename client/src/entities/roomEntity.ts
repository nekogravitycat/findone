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
  // target items for identification
  targets: RoomTargetEntity[]
  // relation to users
  userIds: string[]
  userConnections: string[]
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
  room: RoomEntity
  user: UserEntity
}

export type RoomStatus = "Waiting" | "InProgress" | "Finished"
