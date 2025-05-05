import type { RoomEntity } from "@/entities/roomEntity"
import type { RoundEntity } from "@/entities/roundEntity"
import { SignalRService } from "@/services/signalr"
import { defineStore } from "pinia"
import { computed, ref } from "vue"

export const useGameStore = defineStore("game", () => {
  const api = ref(new SignalRService())
  const room = ref<RoomEntity | null>(null)
  const round = ref<RoundEntity | null>(null)
  const userId = ref<string | null>(null)
  const isHost = computed(() => room.value?.hostUserId === userId.value)

  return { api, room, round, userId, isHost }
})
