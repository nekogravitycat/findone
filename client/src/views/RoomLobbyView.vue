<script setup lang="ts">
import type { UserEntity } from "@/entities/userEntity"
import router from "@/services/router"
import { useGameStore } from "@/stores/gameStore"
import QRCode from "qrcode"
import { ref, onMounted, onUnmounted } from "vue"

// Global game state
const game = useGameStore()

// Reactive state
const players = ref<UserEntity[]>([])
const showShareModal = ref(false)
const qrCodeUrl = ref("")
const copied = ref(false)

// Validate room and user presence
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

// Fetch latest room and player list
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

// Host-only: start the game and get round info
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

// Construct room join URL
function joinUrl() {
  return `https://findone.gravitycat.tw/?room=${game.room?.roomId}`
}

// Show share modal and generate QR code
async function openShareModal() {
  if (!game.room?.roomId) return
  try {
    const url = await QRCode.toDataURL(joinUrl(), { scale: 8 })
    qrCodeUrl.value = url
    showShareModal.value = true
  } catch (err) {
    console.error("[Lobby] Failed to generate QR code:", err)
  }
}

// Close the share modal
function closeShareModal() {
  showShareModal.value = false
}

// Copy join URL to clipboard with feedback
function copyJoinUrl() {
  if (!game.room?.roomId) return
  navigator.clipboard.writeText(joinUrl()).then(() => {
    copied.value = true
    setTimeout(() => {
      copied.value = false
    }, 1500)
  })
}

// Handle keyboard events (Escape to close modal)
function onKeydown(event: KeyboardEvent) {
  if (event.key === "Escape") {
    closeShareModal()
  }
}

// Lifecycle: mount
onMounted(() => {
  updateRoom()
  game.api.onEvent("GameJoined", updateRoom)

  window.addEventListener("keydown", onKeydown)

  // Go to game view when round info is received
  game.api.onRoundInfo((info) => {
    info.endTime = new Date(info.endTime)
    game.round = info
    router.push({ name: "game" })
  })
})

// Lifecycle: unmount
onUnmounted(() => {
  game.api.offEvent("GameJoined", updateRoom)
  window.removeEventListener("keydown", onKeydown)
})
</script>

<template>
  <div
    class="min-h-full flex items-center justify-center bg-gradient-to-br from-blue-100 to-sky-200 px-4"
  >
    <div
      class="w-full max-w-md bg-white rounded-2xl shadow-xl p-6 space-y-6 motion-safe:animate-fade-in"
    >
      <!-- Room Title + Copy Button -->
      <div class="text-center space-y-1">
        <div class="flex items-center justify-center space-x-2">
          <h2 class="text-2xl font-bold text-blue-600">Room {{ game.room?.roomId }}</h2>
          <!-- Share Button -->
          <button
            @click="openShareModal"
            class="text-sm text-blue-500 hover:underline active:scale-95 transition"
          >
            Share
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
  <!-- Share Modal -->
  <div
    v-if="showShareModal"
    class="fixed inset-0 z-50 flex items-center justify-center bg-black/50"
    @click.self="closeShareModal"
  >
    <div class="bg-white rounded-2xl p-6 w-80 relative shadow-xl text-center space-y-4">
      <!-- Close button -->
      <button
        @click="closeShareModal"
        class="absolute top-3 right-3 text-gray-400 hover:text-gray-600"
      >
        ✕
      </button>
      <h3 class="text-lg font-semibold text-gray-700">Share Room</h3>
      <div v-if="qrCodeUrl" class="flex justify-center">
        <img :src="qrCodeUrl" alt="QR Code" class="w-40 h-40" />
      </div>
      <button
        @click="copyJoinUrl"
        class="w-full bg-blue-500 text-white font-semibold py-2 rounded-xl shadow hover:scale-105 active:scale-95 transition-transform duration-150"
      >
        {{ copied ? "Copied!" : "Copy Join URL" }}
      </button>
    </div>
  </div>
</template>
