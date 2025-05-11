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
    class="min-h-screen bg-gradient-to-b from-blue-50 to-white flex flex-col items-center px-6 py-8"
  >
    <!-- Game Rankings -->
    <h1 class="text-3xl font-semibold text-blue-600 tracking-wide mb-8">🏆 Ranking</h1>
    <div class="w-full space-y-4">
      <template v-for="(score, idx) in sortedScores" :key="score.userId">
        <!-- Top 3: Display with avatar on top and info below -->
        <div
          v-if="idx < 3"
          class="rounded-2xl overflow-hidden shadow-xl transform transition-all duration-300 ease-in-out"
        >
          <!-- Image Section with 4:3 aspect ratio -->
          <div class="w-full aspect-[4/3] overflow-hidden">
            <img
              v-if="score.base64Image"
              :src="addBase64Prefix(score.base64Image)"
              alt="avatar"
              class="w-full h-full object-cover"
            />
            <div v-else class="w-full h-full flex items-center justify-center bg-gray-300">
              <span class="text-5xl font-bold text-gray-800">{{ idx + 1 }}</span>
            </div>
          </div>

          <!-- Info Section -->
          <div class="p-4 bg-white" :class="getRankColor(idx)">
            <div class="flex items-baseline justify-between">
              <span
                class="text-xl font-bold truncate overflow-hidden whitespace-nowrap block"
                :title="score.userName"
              >
                {{ score.userName }}
              </span>
              <span class="ml-3 px-3 py-1 rounded text-xs bg-blue-100 text-blue-800 font-semibold">
                # {{ idx + 1 }}
              </span>
            </div>
            <div class="text-base text-slate-700 mt-2">
              Score: {{ score.totalRoundScore.toFixed(2) }}
            </div>
            <div v-if="score.comment" class="text-sm text-slate-900 mt-2 italic opacity-80">
              「{{ score.comment }}」
            </div>
          </div>
        </div>

        <!-- Other rankings -->
        <div
          v-else
          class="rounded-xl px-4 py-3 flex items-center bg-white shadow-sm hover:shadow-lg transition-all duration-200 hover:scale-105"
        >
          <span class="w-10 text-center text-lg font-bold text-gray-500">
            {{ idx + 1 }}
          </span>
          <div class="flex-1 ml-4 truncate">
            <div class="font-medium truncate">{{ score.userName }}</div>
          </div>
          <span class="ml-3 text-slate-700 font-bold">
            {{ score.totalRoundScore.toFixed(2) }}
          </span>
        </div>
      </template>

      <!-- No submissions message -->
      <div
        v-if="sortedScores.length === 0"
        class="text-lg font-semibold text-red-600 mt-6 text-center"
      >
        Threre's no submission in this round.
      </div>
    </div>

    <!-- Button to next round -->
    <div v-if="game.isHost && !isFinal" class="flex justify-center m-5">
      <button
        @click="toNextRound"
        class="mb-6 bg-blue-600 hover:bg-blue-700 text-white px-6 py-3 rounded-lg shadow-lg transition-all duration-200 hover:scale-105"
      >
        Next Round
      </button>
    </div>

    <!-- Exit room button -->
    <div v-if="isFinal" class="mt-8 flex justify-center">
      <button
        @click="toEntry"
        class="bg-red-600 hover:bg-red-700 text-white px-6 py-3 rounded-full shadow-xl transition-all duration-300 hover:scale-105"
      >
        Exit Room
      </button>
    </div>
  </div>
</template>
