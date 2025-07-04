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

// Start countdown timer
function startCountdown(targetTime: Date, onEnd?: () => void) {
  if (countdownInterval) {
    clearInterval(countdownInterval)
  }

  const update = () => {
    const diff = targetTime.getTime() - Date.now()
    if (diff <= 0) {
      countdown.value = "00:00"
      clearInterval(countdownInterval!)
      onEnd?.()
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
  if (!ensureRoomAndUser()) {
    router.replace({ name: "entry" })
    return
  }

  try {
    isSubmitting.value = true
    const result = await game.api.submitImage(game.room!.roomId, game.userId!, image)
    if (!result) {
      throw new Error("Image analysis failed")
    }

    submittedImage.value = image
    showToastMessage("Image submitted!", "success")
    console.log("[Game] Image submitted successfully")
  } catch (error) {
    console.error("[Game] Failed to submit image:", error)
    showToastMessage("Image recognition failed. Please resubmit", "error")
  } finally {
    isSubmitting.value = false
  }
}

// Setup round state and timers
function setupRound() {
  round.value = game.round
  if (!round.value) {
    console.error("[Game] Round data is not available")
    router.replace({ name: "entry" })
    return
  }

  const onCountdownEnds = game.isHost
    ? () => game.api.getRankInvoke(game.room!.roomId, game.userId!)
    : undefined

  startCountdown(round.value.endTime, onCountdownEnds)
}

// Lifecycle hooks
onMounted(async () => {
  setupRound()

  game.api.onRankInfo((scores: ScoreEntity[]) => {
    game.scores = scores
    console.log("[Game] Scores received:", scores)
    router.replace({ name: "rank" })
  })

  game.room = await game.api.getRoom(game.room!.roomId)
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
    class="motion-safe:animate-fade-in flex h-[100dvh] w-full flex-col space-y-4 rounded-2xl bg-white p-6 shadow-xl"
    style="box-sizing: border-box"
  >
    <!-- Round Info & Countdown -->
    <div class="flex w-full items-center justify-between">
      <!-- Round Info -->
      <div class="space-y-1 text-left">
        <p class="text-sm text-gray-500">
          Round {{ (game.room?.currentRound ?? 0) + 1 }} / {{ game.room?.round }}
        </p>
        <h1 class="text-2xl font-bold text-blue-600">📸 {{ round?.targetName }}</h1>
      </div>

      <!-- Countdown -->
      <div v-if="countdown" class="space-y-1 text-right">
        <h2 class="text-sm text-gray-500">Time remaining</h2>
        <p class="font-mono text-2xl text-red-600">{{ countdown }}</p>
      </div>
    </div>

    <!-- Camera or Submitted Photo -->
    <div class="flex min-h-0 flex-1 flex-col items-center justify-center space-y-4">
      <template v-if="!submittedImage">
        <!-- Submitting view -->
        <div v-if="isSubmitting" class="flex h-full items-center justify-center">
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
          class="mx-auto max-h-[60vh] rounded-xl"
        />
        <p class="mt-4 text-lg font-semibold text-gray-700">Waiting for other players...</p>
      </div>
    </div>
  </div>

  <!-- Toast Notification -->
  <transition name="fade">
    <div
      v-if="showToast"
      class="fixed top-1/2 left-1/2 z-50 -translate-x-1/2 -translate-y-1/2 transform rounded-2xl px-6 py-4 text-2xl font-bold text-white shadow-xl"
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
