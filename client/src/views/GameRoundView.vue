<script setup lang="ts">
import Camera from "@/components/camera.vue"
import type { RoundEntity } from "@/entities/roundEntity"
import type { ScoreEntity } from "@/entities/scoreEntity"
import { addBase64Prefix } from "@/lib/b64img"
import router from "@/services/router"
import { useGameStore } from "@/stores/gameStore"
import { ref, onMounted, onUnmounted } from "vue"

const game = useGameStore()

// State
const round = ref<RoundEntity | null>(null)
const countdown = ref("")
const isSubmitting = ref(false)
const submittedImage = ref<string | null>(null)
const showToast = ref<{ message: string; type: "success" | "error" } | null>(null)
let countdownInterval: ReturnType<typeof setInterval> | null = null

// Validate required state
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

// Display toast message
function showToastMessage(message: string, type: "success" | "error" = "success") {
  showToast.value = { message, type }
  setTimeout(() => {
    showToast.value = null
  }, 3000)
}

// Host: auto get rank after round ends
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

// Start countdown timer
function startCountdown(targetTime: Date) {
  if (countdownInterval) {
    clearInterval(countdownInterval)
  }

  const update = () => {
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

  update()
  countdownInterval = setInterval(update, 500)
}

// Player image submission
async function submitImage(image: string): Promise<void> {
  if (!ensureRoomAndUser()) return

  try {
    isSubmitting.value = true
    const result = await game.api.submitImage(game.room!.roomId, game.userId!, image)
    if (!result) {
      throw new Error("Image analysis failed")
    }

    submittedImage.value = image
    showToastMessage("照片提交成功！", "success")
    console.log("[Game] Image submitted successfully")
  } catch (error) {
    console.error("[Game] Failed to submit image:", error)
    showToastMessage("提交失敗，請再試一次！", "error")
  } finally {
    isSubmitting.value = false
  }
}

// Setup round state and timers
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
    router.push({ name: "rank" })
  })
})

onUnmounted(() => {
  if (countdownInterval) {
    clearInterval(countdownInterval)
    countdownInterval = null
  }
})
</script>

<template>
  <div
    class="w-full h-[100dvh] bg-white rounded-2xl shadow-xl p-6 space-y-4 motion-safe:animate-fade-in flex flex-col"
    style="box-sizing: border-box"
  >
    <!-- Round Info & Countdown (left and right aligned) -->
    <div class="flex justify-between items-center w-full">
      <!-- Round Info (aligned to the right) -->
      <div class="text-left space-y-1">
        <p class="text-gray-500 text-sm">
          Round {{ (game.room?.currentRound ?? 0) + 1 }} / {{ game.room?.round }}
        </p>
        <h1 class="text-2xl font-bold text-blue-600">📸 {{ round?.targetName }}</h1>
      </div>

      <!-- Countdown (aligned to the left) -->
      <div v-if="countdown" class="text-right space-y-1">
        <h2 class="text-gray-500 text-sm">Time remaining</h2>
        <p class="text-2xl font-mono text-red-600">{{ countdown }}</p>
      </div>
    </div>

    <!-- Camera or Submitted Photo -->
    <div class="flex-1 flex flex-col items-center justify-center min-h-0 space-y-4">
      <template v-if="!submittedImage">
        <!-- Submitting view -->
        <div v-if="isSubmitting" class="flex items-center justify-center h-full">
          <div class="flex flex-col items-center justify-center space-y-4">
            <div
              class="h-12 w-12 animate-spin rounded-full border-4 border-blue-500 border-t-transparent"
            ></div>
            <p class="text-lg font-semibold text-gray-500">Submitting image...</p>
          </div>
        </div>
        <!-- Camera view -->
        <Camera v-else @photo-taken="submitImage" class="rounded-xl" />
      </template>
      <div v-else class="text-center">
        <!-- Submitted view -->
        <img
          :src="addBase64Prefix(submittedImage)"
          alt="Submitted photo"
          class="rounded-xl max-h-[60vh] mx-auto"
        />
        <p class="mt-4 text-lg text-gray-700 font-semibold">Waiting for other players...</p>
      </div>
    </div>
  </div>

  <!-- Toast Notification -->
  <transition name="fade">
    <div
      v-if="showToast"
      class="fixed top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2 px-6 py-4 rounded-2xl shadow-xl text-white text-2xl font-bold z-50"
      :class="showToast.type === 'success' ? 'bg-green-500' : 'bg-red-500'"
    >
      {{ showToast.message }}
    </div>
  </transition>
</template>

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
