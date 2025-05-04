<script setup lang="ts">
import type { RoundEntity } from "@/entities/roundEntity"
import type { ScoreEntity } from "@/entities/scoreEntity"
import router from "@/services/router"
import { useGameStore } from "@/stores/gameStore"
import { onMounted, ref } from "vue"

const game = useGameStore()

const round = ref<RoundEntity | null>(null)

async function getRound() {
  if (!game.room?.roomId) {
    console.error("Room ID is not available")
    return
  }
  if (!game.userId) {
    console.error("User ID is not available")
    return
  }
  const roundInfo = await game.api.getRound(
    game.room.roomId,
    game.userId,
    game.room.currentRound ?? 0
  )
  if (!roundInfo) {
    console.error("Failed to fetch round data")
    return
  }
  round.value = roundInfo
  console.log("Round data updated:", roundInfo)
  const endTime = new Date(roundInfo.endTime).getTime() // timestamp where the round ends
  const now = Date.now()
  const timeLeft = endTime - now

  if (timeLeft > 0) {
    setTimeout(() => {
      onRoundEnd()
    }, timeLeft)
  } else {
    // Round has already ended
    onRoundEnd()
  }
}

async function onRoundEnd() {
  if (!game.room?.roomId) {
    console.error("Room ID is not available")
    return
  }
  if (!game.userId) {
    console.error("User ID is not available")
    return
  }
  const scores = await game.api.getRank(game.room.roomId, game.userId)
  if (!scores) {
    console.error("Failed to fetch scores")
    return
  }
  console.log("Scores data updated:", scores)
  goRankView()
}

function goRankView() {
  router.push({ name: "rank" })
}

onMounted(() => {
  if (game.isHost) {
    getRound()
  } else {
    game.api.onEvent("RoundInfo", (roundInfo: RoundEntity) => {
      console.log("Round info received:", roundInfo)
      round.value = roundInfo
    })
    game.api.onEvent("RankInfo", (rankInfo: ScoreEntity[]) => {
      console.log("Rank info received:", rankInfo)
    })
  }
})
</script>

<template>
  <!-- Button to Get Rank -->
  <div v-if="game.isHost" class="flex justify-center">
    <button @click="onRoundEnd" class="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600">
      Get Rank
    </button>
  </div>
</template>
