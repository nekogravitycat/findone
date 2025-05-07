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
let countdownInterval: ReturnType<typeof setInterval> | null = null

function invokeGetRound() {
  if (!game.room?.roomId) {
    console.error("Room ID is not available")
    return
  }
  if (!game.userId) {
    console.error("User ID is not available")
    return
  }
  game.api.getRoundInvoke(game.room.roomId, game.userId, game.room.currentRound ?? 0)
}

function invokeGetRankOnTime(time: Date) {
  const getRank = () => {
    if (!game.room?.roomId) {
      console.error("Room ID is not available")
      return
    }
    if (!game.userId) {
      console.error("User ID is not available")
      return
    }
    game.api.getRankInvoke(game.room?.roomId, game.userId)
  }

  const delay = time.getTime() - Date.now()

  if (delay <= 0) {
    // If the time is already passed, call getRank immediately
    getRank()
  } else {
    // If the time is in the future, set a timeout to call getRank
    setTimeout(getRank, delay)
  }
}

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
  countdownInterval = setInterval(updateCountdown, 1000)
}

async function submitImage(image: string) {
  if (!game.room?.roomId) {
    console.error("Room ID is not available")
    return
  }
  if (!game.userId) {
    console.error("Room ID is not available")
    return
  }
  try {
    const result = await game.api.submitImage(game.room.roomId, game.userId, image)
    if (!result) {
      throw new Error("Image analysis failed")
    }
    console.log("Image submitted successfully")
  } catch (error) {
    console.error("Failed to submit image:", error)
  }
}

onMounted(async () => {
  if (game.isHost) {
    invokeGetRound()
  }
  game.api.onRoundInfo((info: RoundEntity) => {
    info.endTime = new Date(info.endTime)
    round.value = info
    startCountdown(round.value.endTime)
    if (game.isHost) {
      invokeGetRankOnTime(round.value.endTime)
    }
  })
  game.api.onRankInfo((scores: ScoreEntity[]) => {
    game.scores = scores
    console.log("Scores updated:", scores)
    router.push({ name: "rank" })
  })
})

onUnmounted(() => {
  if (countdownInterval) {
    clearInterval(countdownInterval)
  }
})
</script>

<template>
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
</template>
