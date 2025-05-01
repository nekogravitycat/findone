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
  game.room.userIds.forEach(async (usrId) => {
    const user = await game.api.getUser(usrId)
    if (user) {
      game.usersCache.set(usrId, user)
    }
  })
  return game.room
}
