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
    router.replace({ name: "entry" })
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
  if (!ensureRoomAndUser()) {
    router.replace({ name: "entry" })
    return
  }
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
    router.replace({ name: "game" })
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
    class="flex min-h-full items-center justify-center bg-gradient-to-br from-blue-100 to-sky-200 px-4"
  >
    <div
      class="motion-safe:animate-fade-in w-full max-w-md space-y-6 rounded-2xl bg-white p-6 shadow-xl"
    >
      <!-- Room Title + Copy Button -->
      <div class="space-y-1 text-center">
        <div class="flex items-center justify-center space-x-2">
          <h2 class="text-2xl font-bold text-blue-600">Room {{ game.room?.roomId }}</h2>
          <!-- Share Button -->
          <button
            @click="openShareModal"
            class="text-sm text-blue-500 transition hover:underline active:scale-95"
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
            class="rounded-xl border border-gray-300 bg-gray-100 px-4 py-2 font-medium text-gray-800 shadow-sm"
          >
            {{ player.userName }}
          </div>
        </transition-group>
      </div>

      <!-- Buttons -->
      <div class="space-y-3">
        <button
          @click="updateRoom"
          class="w-full rounded-xl bg-gradient-to-r from-blue-500 to-blue-600 py-2 font-semibold text-white shadow transition-transform duration-150 hover:scale-105 active:scale-95"
        >
          Refresh Players
        </button>

        <button
          v-if="game.isHost"
          @click="startGame"
          class="w-full rounded-xl bg-gradient-to-r from-green-500 to-green-600 py-2 font-semibold text-white shadow transition-transform duration-150 hover:scale-105 active:scale-95"
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
    <div class="relative w-80 space-y-4 rounded-2xl bg-white p-6 text-center shadow-xl">
      <!-- Close button -->
      <button
        @click="closeShareModal"
        class="absolute top-3 right-3 text-gray-400 hover:text-gray-600"
      >
        ✕
      </button>
      <h3 class="text-lg font-semibold text-gray-700">Share Room</h3>
      <div v-if="qrCodeUrl" class="flex justify-center">
        <img :src="qrCodeUrl" alt="QR Code" class="h-40 w-40" />
      </div>
      <button
        @click="copyJoinUrl"
        class="w-full rounded-xl bg-blue-500 py-2 font-semibold text-white shadow transition-transform duration-150 hover:scale-105 active:scale-95"
      >
        {{ copied ? "Copied!" : "Copy Join URL" }}
      </button>
    </div>
  </div>
</template>
