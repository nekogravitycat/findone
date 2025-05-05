export interface ScoreEntity {
  userId: string
  userName: string
  totalRoundScore: number
  currentRoundScore: number
  base64Image?: string // Only the first three ranked player
  comment?: string // Only the first three ranked player
}
