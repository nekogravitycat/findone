<script setup lang="ts">
import type { RoundEntity } from "@/entities/roundEntity"
import { addBase64Prefix } from "@/lib/b64img"
import router from "@/services/router"
import { useGameStore } from "@/stores/gameStore"
import { computed, onMounted } from "vue"

const game = useGameStore()

// Sorted scores: higher total first, then by userName
const sortedScores = computed(() => {
  return [...game.scores].sort((a, b) =>
    b.totalRoundScore !== a.totalRoundScore
      ? b.totalRoundScore - a.totalRoundScore
      : a.userName.localeCompare(b.userName)
  )
})

// Check if this is the final round
const isFinal = computed(() => {
  return (game.room?.currentRound ?? 0) + 1 >= (game.room?.round ?? 0)
})

// Ensure room ID and user ID are present
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

// Host: move to next round
function toNextRound(): void {
  if (!ensureRoomAndUser()) return
  const nextRound = (game.room?.currentRound ?? 0) + 1
  game.api.getRoundInvoke(game.room!.roomId, game.userId!, nextRound)
}

// End game and return to entry page
function toEntry(): void {
  game.room = null
  game.round = null
  game.userId = null
  game.scores = []
  router.push({ name: "entry" })
}

// Get Tailwind class for top ranks (Gold, Silver, Bronze, Default)
function getRankColor(idx: number): string {
  switch (idx) {
    case 0: // Gold
      return "bg-yellow-500 text-yellow-900"
    case 1: // Silver
      return "bg-zinc-400 text-zinc-900"
    case 2: // Bronze
      return "bg-yellow-600 text-amber-900"
    default: // White
      return "bg-white text-gray-800"
  }
}

// Listen for next round info
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
    class="flex min-h-screen flex-col items-center bg-gradient-to-b from-blue-50 to-white px-6 py-8"
  >
    <!-- Game Rankings -->
    <h1 class="mb-8 text-3xl font-semibold tracking-wide text-blue-600">🏆 Ranking</h1>
    <div class="w-full space-y-4">
      <template v-for="(score, idx) in sortedScores" :key="score.userId">
        <!-- Top 3: Display with avatar on top and info below -->
        <div
          v-if="idx < 3"
          class="transform overflow-hidden rounded-2xl shadow-xl transition-all duration-300 ease-in-out"
        >
          <!-- Image Section with 4:3 aspect ratio -->
          <div class="aspect-[4/3] w-full overflow-hidden">
            <img
              v-if="score.base64Image"
              :src="addBase64Prefix(score.base64Image)"
              alt="avatar"
              class="h-full w-full object-cover"
            />
            <div v-else class="flex h-full w-full items-center justify-center bg-gray-300">
              <span class="text-5xl font-bold text-gray-800">{{ idx + 1 }}</span>
            </div>
          </div>

          <!-- Info Section -->
          <div class="bg-white p-4" :class="getRankColor(idx)">
            <div class="flex items-baseline justify-between">
              <span
                class="block truncate overflow-hidden text-xl font-bold whitespace-nowrap"
                :title="score.userName"
              >
                {{ score.userName }}
              </span>
              <span class="ml-3 rounded bg-blue-100 px-3 py-1 text-xs font-semibold text-blue-800">
                # {{ idx + 1 }}
              </span>
            </div>
            <div class="mt-2 text-base text-slate-700">
              Score: {{ score.totalRoundScore.toFixed(2) }}
            </div>
            <div v-if="score.comment" class="mt-2 text-sm text-slate-900 italic opacity-80">
              「{{ score.comment }}」
            </div>
          </div>
        </div>

        <!-- Other rankings -->
        <div
          v-else
          class="flex items-center rounded-xl bg-white px-4 py-3 shadow-sm transition-all duration-200 hover:scale-105 hover:shadow-lg"
        >
          <span class="w-10 text-center text-lg font-bold text-gray-500">
            {{ idx + 1 }}
          </span>
          <div class="ml-4 flex-1 truncate">
            <div class="truncate font-medium">{{ score.userName }}</div>
          </div>
          <span class="ml-3 font-bold text-slate-700">
            {{ score.totalRoundScore.toFixed(2) }}
          </span>
        </div>
      </template>

      <!-- No submissions message -->
      <div
        v-if="sortedScores.length === 0"
        class="mt-6 text-center text-lg font-semibold text-red-600"
      >
        Threre's no submission in this round.
      </div>
    </div>

    <!-- Button to next round -->
    <div v-if="game.isHost && !isFinal" class="m-5 flex justify-center">
      <button
        @click="toNextRound"
        class="mb-6 rounded-lg bg-blue-600 px-6 py-3 text-white shadow-lg transition-all duration-200 hover:scale-105 hover:bg-blue-700"
      >
        Next Round
      </button>
    </div>

    <!-- Exit room button -->
    <div v-if="isFinal" class="mt-8 flex justify-center">
      <button
        @click="toEntry"
        class="rounded-full bg-red-600 px-6 py-3 text-white shadow-xl transition-all duration-300 hover:scale-105 hover:bg-red-700"
      >
        Exit Room
      </button>
    </div>
  </div>
</template>
