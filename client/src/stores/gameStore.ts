import type { RoomEntity } from "@/entities/roomEntity"
import type { UserEntity } from "@/entities/userEntity"
import { defineStore } from "pinia"
import { ref } from "vue"

export const useGameStore = defineStore("game", () => {
  const room = ref<RoomEntity | null>(null)
  const users = ref<UserEntity[]>([])
  const userId = ref<string | null>(null)

  return { room, users, userId }
})
