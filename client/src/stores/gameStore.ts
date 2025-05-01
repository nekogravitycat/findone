import type { RoomEntity } from "@/entities/roomEntity"
import type { UserEntity } from "@/entities/userEntity"
import { SignalRService } from "@/services/signalr"
import { defineStore } from "pinia"
import { ref } from "vue"

export const useGameStore = defineStore("game", () => {
  const api = ref(new SignalRService())
  const room = ref<RoomEntity | null>(null)
  const userId = ref<string | null>(null)
  const usersCache = ref(new Map<string, UserEntity>())

  return { api, room, usersCache, userId }
})

export async function updateRoomInfo(roomId: string) {
  const game = useGameStore()
  // Get room info from the server
  game.room = await game.api.getRoom(roomId)
  // Update the users cache
  for (const usrId of game.room.userIds) {
    try {
      console.log("Fetching user:", usrId)
      const user = await game.api.getUser(usrId)
      console.log(`User fetched: ${usrId} - ${user.userId}`)
      if (user) {
        game.usersCache.set(usrId, user)
      }
    } catch (error) {
      console.error("Error fetching user:", error)
    }
  }
  return game.room
}
