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

export type RoomStatus = "Waiting" | "InProgress" | "Finished"
