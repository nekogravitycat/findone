<script setup lang="ts">
import type { UserEntity } from "@/entities/userEntity"
import router from "@/services/router"
import { useGameStore } from "@/stores/gameStore"
import { onMounted, onUnmounted, ref } from "vue"

const game = useGameStore()
const players = ref<UserEntity[]>([])

async function updateRoom() {
  if (!game.room?.roomId) {
    console.error("Room ID is not available")
    return
  }
  const room = await game.api.getRoom(game.room.roomId)
  if (!room) {
    console.error("Failed to fetch room data")
    return
  }
  game.room = room
  console.log("Room data updated:", room)
  // Fetching user data for each userId in the room
  const userPromises = game.room.userIds.map((userId) => game.api.getUser(userId))
  const fetchedUsers = await Promise.all(userPromises)
  players.value = fetchedUsers
}

async function startGame() {
  if (!game.room?.roomId) {
    console.error("Room ID is not available")
    return
  }
  if (!game.userId) {
    console.error("User ID is not available")
    return
  }
  try {
    await game.api.gameStartInvoke(game.room.roomId, game.userId)
    console.log("Game started successfully")
  } catch (error) {
    console.error("Failed to start game:", error)
  }
}

function toGameRound() {
  router.push({ name: "game" })
}

onMounted(() => {
  updateRoom()
  game.api.onEvent("GameJoined", updateRoom)
  game.api.onGameStart(toGameRound)
})

onUnmounted(() => {
  game.api.offEvent("GameJoined", updateRoom)
})
</script>

<template>
  <div class="p-4 space-y-4">
    <!-- User list -->
    <div class="space-y-2">
      <h2 class="text-xl font-semibold">Players in Room {{ game.room?.roomId }}</h2>
      <div
        v-for="player in players"
        :key="player.userId"
        class="flex items-center justify-between p-3 bg-white rounded-lg shadow border"
      >
        <p class="text-gray-800 font-medium">{{ player.userName }}</p>
        <p class="text-gray-800 font-medium">{{ player.userId }}</p>
      </div>
    </div>

    <!-- Update Room Info Button -->
    <div>
      <button
        @click="updateRoom"
        class="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded shadow transition"
      >
        Update Room Info
      </button>
    </div>

    <!-- Start Game Button -->
    <div v-if="game.isHost">
      <button
        @click="startGame"
        class="bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded shadow transition"
      >
        Start Game
      </button>
    </div>
  </div>
</template>
