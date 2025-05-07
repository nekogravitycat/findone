export interface UserEntity {
  userId: string
  userName: string
  roomId: string
  joinedAt: Date
  scores: UserScoreEntity[]
}

export interface UserScoreEntity {
  id: string
  dateTime: Date
  userId: string
  roundIndex: number
  base64Image: string
  comment: string
  score: number
}
