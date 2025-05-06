<script setup lang="ts">
import { addBase64Prefix } from "@/lib/b64img"
import { useGameStore } from "@/stores/gameStore"
import { computed } from "vue"

const game = useGameStore()

const sortedScores = computed(() => {
  return [...game.scores].sort((a, b) =>
    b.totalRoundScore !== a.totalRoundScore
      ? b.totalRoundScore - a.totalRoundScore
      : a.userName.localeCompare(b.userName)
  )
})

function getRankColor(idx: number) {
  if (idx === 0) return "bg-yellow-400 text-yellow-900"
  if (idx === 1) return "bg-gray-300 text-gray-900"
  if (idx === 2) return "bg-yellow-700 text-yellow-100"
  return "bg-slate-100 text-slate-800"
}
</script>

<template>
  <div
    class="min-h-screen bg-gradient-to-b from-blue-50 to-white flex flex-col items-center px-2 py-6"
  >
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
  </div>
</template>
