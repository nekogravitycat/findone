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

const copied = ref(false)

function copyRoomId() {
  if (!game.room?.roomId) return
  navigator.clipboard.writeText(game.room.roomId).then(() => {
    copied.value = true
    setTimeout(() => {
      copied.value = false
    }, 1500)
  })
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
  <div
    class="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-100 to-sky-200 px-4"
  >
    <div
      class="w-full max-w-md bg-white rounded-2xl shadow-xl p-6 space-y-6 motion-safe:animate-fade-in"
    >
      <!-- Room Title + Copy Button -->
      <div class="text-center space-y-1">
        <div class="flex items-center justify-center space-x-2">
          <h2 class="text-2xl font-bold text-blue-600">Room {{ game.room?.roomId }}</h2>
          <button
            @click="copyRoomId"
            class="text-sm text-blue-500 hover:underline active:scale-95 transition"
          >
            {{ copied ? "Copied!" : "Copy" }}
          </button>
        </div>
        <p class="text-sm text-gray-500">Share the room ID with friends to join</p>
      </div>

      <!-- Players List -->
      <div class="space-y-2">
        <h3 class="text-lg font-semibold text-gray-700">Player list</h3>
        <transition-group name="fade" tag="div" class="space-y-2">
          <div
            v-for="player in players"
            :key="player.userId"
            class="px-4 py-2 bg-gray-100 rounded-xl border border-gray-300 shadow-sm text-gray-800 font-medium"
          >
            {{ player.userName }}
          </div>
        </transition-group>
      </div>

      <!-- Buttons -->
      <div class="space-y-3">
        <button
          @click="updateRoom"
          class="w-full bg-gradient-to-r from-blue-500 to-blue-600 text-white font-semibold py-2 rounded-xl shadow hover:scale-105 active:scale-95 transition-transform duration-150"
        >
          Refresh Players
        </button>

        <button
          v-if="game.isHost"
          @click="startGame"
          class="w-full bg-gradient-to-r from-green-500 to-green-600 text-white font-semibold py-2 rounded-xl shadow hover:scale-105 active:scale-95 transition-transform duration-150"
        >
          Start Game
        </button>
      </div>
    </div>
  </div>
</template>
