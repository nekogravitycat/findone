import type { RoomEntity } from "@/entities/roomEntity"
import type { RoundEntity } from "@/entities/roundEntity"
import type { ScoreEntity } from "@/entities/scoreEntity"
import { GameAPI } from "@/services/game"
import { defineStore } from "pinia"
import { computed, ref } from "vue"

export const useGameStore = defineStore(
  "game",
  () => {
    const api = ref(new GameAPI())
    const room = ref<RoomEntity | null>(null)
    const round = ref<RoundEntity | null>(null)
    const userId = ref<string | null>(null)
    const scores = ref<ScoreEntity[]>([])
    const isHost = computed(() => room.value?.hostUserId === userId.value)
    const cameraId = ref<string | null>(null)
    const facingMode = ref<"user" | "environment">("user")

    return { api, room, round, userId, scores, isHost, cameraId, facingMode }
  },
  {
    persist: {
      afterHydrate: (ctx) => {
        ctx.store.api = new GameAPI()
      },
    },
  }
)
