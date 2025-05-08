<script setup lang="ts">
import type { RoundEntity } from "@/entities/roundEntity"
import { addBase64Prefix } from "@/lib/b64img"
import router from "@/services/router"
import { useGameStore } from "@/stores/gameStore"
import { computed, onMounted } from "vue"

const game = useGameStore()

// Helpers
function ensureRoomAndUser(): boolean {
  if (!game.room?.roomId) {
    console.error("[Rank] Missing room ID")
    return false
  }
  if (!game.userId) {
    console.error("[Rank] Missing user ID")
    return false
  }
  return true
}

// Sort scores by total, then by userName
const sortedScores = computed(() => {
  return [...game.scores].sort((a, b) =>
    b.totalRoundScore !== a.totalRoundScore
      ? b.totalRoundScore - a.totalRoundScore
      : a.userName.localeCompare(b.userName)
  )
})

// Determine if the game is over
const isFinal = computed(() => {
  return (game.room?.currentRound ?? 0) + 1 >= (game.room?.round ?? 0)
})

// Host starts next round
function toNextRound(): void {
  if (!ensureRoomAndUser()) return

  game.room!.currentRound = (game.room?.currentRound ?? 0) + 1
  game.api.getRoundInvoke(game.room!.roomId, game.userId!, game.room!.currentRound)
}

// Game over - return to entry page
function toEntry() {
  // Clear game state
  game.room = null
  game.round = null
  game.userId = null
  game.scores = []
  router.push({ name: "entry" })
}

// Rank badge color based on index
function getRankColor(idx: number): string {
  switch (idx) {
    case 0:
      return "bg-yellow-400 text-yellow-900"
    case 1:
      return "bg-gray-300 text-gray-900"
    case 2:
      return "bg-yellow-700 text-yellow-100"
    default:
      return "bg-slate-100 text-slate-800"
  }
}

// Listen for next round data
onMounted(() => {
  game.api.onRoundInfo((info: RoundEntity) => {
    info.endTime = new Date(info.endTime)
    game.round = info
    router.push({ name: "game" })
  })
})
</script>

<template>
  <div
    class="min-h-screen bg-gradient-to-b from-blue-50 to-white flex flex-col items-center px-2 py-6"
  >
    <!-- Button to next round -->
    <button
      v-if="game.isHost && !isFinal"
      @click="toNextRound"
      class="mb-4 bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded shadow transition"
    >
      Next Round
    </button>
    <!-- Game Rankings -->
    <h1 class="text-xl font-bold mb-6 tracking-wide text-blue-600">🏆 Game Rankings</h1>
    <div class="w-full max-w-md space-y-3">
      <template v-for="(score, idx) in sortedScores" :key="score.userId">
        <!-- Top 3: Display with avatar and comment card -->
        <div
          v-if="idx < 3"
          class="rounded-xl p-4 flex items-center shadow-md"
          :class="getRankColor(idx)"
        >
          <div class="w-14 h-14 overflow-hidden rounded-full border-2 border-white shrink-0">
            <img
              v-if="score.base64Image"
              :src="addBase64Prefix(score.base64Image)"
              alt="avatar"
              class="w-full h-full object-cover"
            />
            <div v-else class="w-full h-full flex items-center justify-center bg-gray-200">
              <span class="text-2xl">{{ idx + 1 }}</span>
            </div>
          </div>
          <div class="ml-4 flex-1">
            <div class="flex items-baseline">
              <span class="text-lg font-bold">{{ score.userName }}</span>
              <span class="ml-2 px-2 py-0.5 rounded text-xs bg-white/60 text-blue-800 font-medium">
                #{{ idx + 1 }}
              </span>
            </div>
            <div class="text-sm text-slate-700 mt-1">
              Score: {{ score.totalRoundScore.toFixed(2) }}
            </div>
            <div v-if="score.comment" class="text-xs text-slate-900 mt-1 italic opacity-80">
              "{{ score.comment }}"
            </div>
          </div>
        </div>
        <!-- Others -->
        <div v-else class="rounded-xl px-4 py-2 flex items-center bg-white shadow-sm">
          <span class="w-8 text-center text-lg text-slate-500 font-bold">
            {{ idx + 1 }}
          </span>
          <div class="flex-1 ml-2 truncate">
            <div class="font-medium truncate">{{ score.userName }}</div>
          </div>
          <span class="ml-1 text-slate-700 font-bold">
            {{ score.totalRoundScore.toFixed(2) }}
          </span>
        </div>
      </template>
    </div>
    <!-- Exit room -->
    <div class="mt-6" v-if="isFinal">
      <button
        @click="toEntry"
        class="bg-red-600 hover:bg-red-700 text-white px-6 py-3 rounded-full shadow-lg transition"
      >
        Exit Room
      </button>
    </div>
  </div>
</template>
