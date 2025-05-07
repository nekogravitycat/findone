<script setup lang="ts">
import Camera from "@/components/camera.vue"
import type { RoundEntity } from "@/entities/roundEntity"
import type { ScoreEntity } from "@/entities/scoreEntity"
import router from "@/services/router"
import { useGameStore } from "@/stores/gameStore"
import { onMounted, onUnmounted, ref } from "vue"

const game = useGameStore()
const round = ref<RoundEntity | null>(null)
const countdown = ref("")
const showToast = ref<string | null>(null) // Toast message

let countdownInterval: ReturnType<typeof setInterval> | null = null

// Helpers
function ensureRoomAndUser(): boolean {
  if (!game.room?.roomId) {
    console.error("[Game] Missing room ID")
    return false
  }
  if (!game.userId) {
    console.error("[Game] Missing user ID")
    return false
  }
  return true
}

// Show a toast message to the user
function showToastMessage(message: string) {
  showToast.value = message
  setTimeout(() => {
    showToast.value = null // Hide toast after 3 seconds
  }, 3000)
}

// Get rank after round ends (host only)
function invokeGetRankOnTime(endTime: Date) {
  if (!ensureRoomAndUser()) return

  const delay = endTime.getTime() - Date.now()

  const getRank = () => {
    game.api.getRankInvoke(game.room!.roomId, game.userId!)
    console.log("[Game] getRank invoked")
  }

  if (delay <= 0) {
    getRank()
  } else {
    setTimeout(getRank, delay)
  }
}

// Countdown display
function startCountdown(targetTime: Date) {
  if (countdownInterval) {
    clearInterval(countdownInterval)
  }

  const updateCountdown = () => {
    const diff = targetTime.getTime() - Date.now()
    if (diff <= 0) {
      countdown.value = "00:00"
      clearInterval(countdownInterval!)
      return
    }
    const minutes = Math.floor(diff / 60000)
    const seconds = Math.floor((diff % 60000) / 1000)
    countdown.value = `${String(minutes).padStart(2, "0")}:${String(seconds).padStart(2, "0")}`
  }

  updateCountdown()
  countdownInterval = setInterval(updateCountdown, 500)
}

// Player submits image
async function submitImage(image: string): Promise<void> {
  if (!ensureRoomAndUser()) return

  try {
    const result = await game.api.submitImage(game.room!.roomId, game.userId!, image)
    if (!result) {
      throw new Error("Image analysis failed")
    }
    console.log("[Game] Image submitted successfully")
    // Show a toast message to the user
    showToastMessage("Image submitted successfully!")
  } catch (error) {
    console.error("[Game] Failed to submit image:", error)
  }
}

// Setup round info on enter
function setupRound() {
  round.value = game.round
  if (!round.value) {
    console.error("[Game] Round data is not available")
    return
  }

  startCountdown(round.value.endTime)

  if (game.isHost) {
    invokeGetRankOnTime(round.value.endTime)
  }
}

// Lifecycle hooks
onMounted(() => {
  setupRound()

  game.api.onRankInfo((scores: ScoreEntity[]) => {
    game.scores = scores
    console.log("[Game] Scores received:", scores)
    // Check if the game is over
    if ((game.room?.currentRound ?? 0) + 1 < (game.room?.round ?? 0)) {
      router.push({ name: "rank" })
    } else {
      router.push({ name: "result" })
    }
  })
})

onUnmounted(() => {
  if (countdownInterval) {
    clearInterval(countdownInterval)
  }
})
</script>

<template>
  <!-- Round number -->
  <div class="text-center">
    <p class="mt-2 text-lg">
      Round: {{ (game.room?.currentRound ?? 0) + 1 }} / {{ game.room?.round }}
    </p>
  </div>
  <!-- Game Objetive -->
  <div class="text-center">
    <h1 class="text-2xl font-bold">Game Objective:</h1>
    <p class="mt-2 text-lg">{{ round?.targetName }}</p>
  </div>
  <!-- Countdown Timer -->
  <div v-if="countdown" class="text-center mt-2">
    <h2 class="text-lg font-semibold text-gray-700">Time Remaining:</h2>
    <p class="text-xl font-mono text-red-600">{{ countdown }}</p>
  </div>
  <!-- Camera View -->
  <Camera @photo-taken="submitImage"></Camera>
  <!-- Button to Get Rank -->
  <div v-if="game.isHost" class="flex justify-center">
    <button
      @click="invokeGetRankOnTime(new Date())"
      class="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600"
    >
      Get Rank Now
    </button>
  </div>
  <!-- Toast Notification -->
  <div
    v-if="showToast"
    class="fixed bottom-4 left-1/2 transform -translate-x-1/2 bg-green-500 text-white px-4 py-2 rounded-lg shadow-md"
  >
    {{ showToast }}
  </div>
</template>
