export interface ScoreEntity {
  userId: string
  userName: string
  totalRoundScore: number
  currentRoundScore: number
  base64Image?: string
  comment?: string
}
