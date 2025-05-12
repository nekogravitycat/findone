<script setup lang="ts">
import type { RoomJoinResultEntity } from "@/entities/roomEntity"
import router from "@/services/router"
import { useGameStore } from "@/stores/gameStore"
import { ref, computed, onMounted } from "vue"
import { useRoute } from "vue-router"

// Store
const game = useGameStore()

// UI state
const name = ref("")
const joinRoomId = ref("")
const joinRoomMode = ref(false)

const wasTriedCreate = ref(false)
const wasTriedJoin = ref(false)

// Name input validation
const nameError = computed(() => {
  if (!name.value.trim()) return "Name is required"
  if (name.value.length > 20) return "Name must be 20 characters or less"
  return ""
})

// Room ID input validation
const joinRoomIdError = computed(() => {
  if (!joinRoomId.value.trim()) return "Room ID is required"
  if (!/^[a-zA-Z0-9]{6}$/.test(joinRoomId.value)) return "Room ID must be 6 alphanumeric characters"
  return ""
})

// Navigate to lobby with room data
async function toRoomLobby(roomJoinResult: RoomJoinResultEntity) {
  game.room = roomJoinResult.room
  game.userId = roomJoinResult.user.userId
  router.push({ name: "lobby" })
}

// Create a new room
async function createRoom() {
  wasTriedCreate.value = true
  if (nameError.value) return
  try {
    const res = await game.api.createRoom(name.value.trim(), 2, 40)
    toRoomLobby(res)
  } catch (e) {
    console.error(e)
  }
}

// Join an existing room
async function joinRoom() {
  wasTriedJoin.value = true
  if (nameError.value || joinRoomIdError.value) return
  try {
    const res = await game.api.joinRoom(joinRoomId.value.trim(), name.value.trim())
    toRoomLobby(res)
  } catch (e) {
    console.error(e)
  }
}

// Check for room query param on mount
onMounted(() => {
  const route = useRoute()
  if (route.query.room) {
    joinRoomId.value = route.query.room as string
    joinRoomMode.value = true
    // Remove 'room' query from URL without reloading the page
    router.replace({ path: route.path, query: { ...route.query, room: undefined } })
  }
})
</script>

<template>
  <div
    class="min-h-full flex items-center justify-center bg-gradient-to-br from-sky-100 to-blue-100 px-4"
  >
    <div
      class="w-full max-w-sm bg-white rounded-2xl shadow-xl p-6 space-y-6 motion-safe:animate-fade-in"
    >
      <!-- App Title -->
      <div class="text-center">
        <img
          src="@/assets/findone.png"
          alt="Findone Logo"
          class="w-32 h-32 mx-auto mb-4 animate-breathing"
        />
        <h1 class="text-3xl font-extrabold text-blue-600 tracking-wide">Findone</h1>
        <p class="text-gray-500 mt-1 text-sm">Fast and fun AI-powered gameplay</p>
      </div>

      <!-- Form Content -->
      <div class="space-y-4">
        <!-- Name input -->
        <div class="space-y-1">
          <input
            v-model="name"
            type="text"
            placeholder="Enter your name"
            class="w-full border rounded-xl px-4 py-2 focus:outline-none focus:ring-2 transition duration-150"
            :class="
              (wasTriedCreate || wasTriedJoin) && nameError
                ? 'border-red-400 focus:ring-red-400'
                : 'border-gray-300 focus:ring-blue-400'
            "
          />
          <p v-if="(wasTriedCreate || wasTriedJoin) && nameError" class="text-sm text-red-500">
            {{ nameError }}
          </p>
        </div>

        <!-- Hide when join code is provided in the URL -->
        <div v-if="!joinRoomMode">
          <!-- Create Room button -->
          <button
            @click="createRoom"
            class="w-full bg-gradient-to-r from-blue-500 to-blue-600 text-white font-semibold py-2 rounded-xl shadow transition-transform duration-150"
            :class="{ 'hover:scale-105 active:scale-95': true }"
          >
            Create Room
          </button>

          <!-- Divider with lines -->
          <div class="flex items-center justify-between text-gray-400 text-sm m-4">
            <div class="flex-grow border-t border-gray-300"></div>
            <span class="px-3">or join existing</span>
            <div class="flex-grow border-t border-gray-300"></div>
          </div>

          <!-- Room ID input -->
          <div class="space-y-1">
            <input
              v-model="joinRoomId"
              type="text"
              placeholder="Enter Room ID"
              class="w-full border rounded-xl px-4 py-2 focus:outline-none focus:ring-2 transition duration-150"
              :class="
                wasTriedJoin && joinRoomIdError
                  ? 'border-red-400 focus:ring-red-400'
                  : 'border-gray-300 focus:ring-green-400'
              "
            />
            <p v-if="wasTriedJoin && joinRoomIdError" class="text-sm text-red-500">
              {{ joinRoomIdError }}
            </p>
          </div>
        </div>

        <!-- Join Room button -->
        <button
          @click="joinRoom"
          class="w-full bg-gradient-to-r from-green-500 to-green-600 text-white font-semibold py-2 rounded-xl shadow transition-transform duration-150"
          :class="{ 'hover:scale-105 active:scale-95': true }"
        >
          Join Room
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
@keyframes breathing {
  0% {
    transform: scale(1);
  }
  50% {
    transform: scale(1.05);
  }
  100% {
    transform: scale(1);
  }
}

.animate-breathing {
  animation: breathing 3s ease-in-out infinite;
}
</style>
