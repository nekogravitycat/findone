<script setup lang="ts">
import type { UserEntity } from "@/entities/userEntity"
import router from "@/services/router"
import { useGameStore } from "@/stores/gameStore"
import { onMounted, onUnmounted, ref } from "vue"

const game = useGameStore()
const players = ref<UserEntity[]>([])

// Helpers
function ensureRoomAndUser(): boolean {
  if (!game.room?.roomId) {
    console.error("[Lobby] Missing room ID")
    return false
  }
  if (!game.userId) {
    console.error("[Lobby] Missing user ID")
    return false
  }
  return true
}

// Fetch room & players
async function updateRoom() {
  if (!game.room?.roomId) {
    console.error("[Lobby] Cannot update room: room ID is not available")
    return
  }
  try {
    const room = await game.api.getRoom(game.room.roomId)
    if (!room) {
      console.error("[Lobby] Failed to fetch room data")
      return
    }
    game.room = room
    console.log("[Lobby] Room data updated:", room)

    const userPromises = room.userIds.map((id) => game.api.getUser(id))
    const users = await Promise.all(userPromises)
    players.value = users.filter((u): u is UserEntity => !!u)
  } catch (err) {
    console.error("[Lobby] Error while updating room:", err)
  }
}

// Only host can call this
async function startGame() {
  if (!ensureRoomAndUser()) return
  try {
    await game.api.gameStartInvoke(game.room!.roomId, game.userId!)
    console.log("[Lobby] Game started by host")
    game.api.getRoundInvoke(game.room!.roomId, game.userId!, game.room!.currentRound ?? 0)
  } catch (error) {
    console.error("[Lobby] Failed to start game:", error)
  }
}

// Lifecycle hooks
onMounted(() => {
  updateRoom()
  game.api.onEvent("GameJoined", updateRoom)

  // Go to game view when round info is received
  game.api.onRoundInfo((info) => {
    info.endTime = new Date(info.endTime)
    game.round = info
    router.push({ name: "game" })
  })
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
