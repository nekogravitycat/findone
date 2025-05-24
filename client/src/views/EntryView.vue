<script setup lang="ts">
import type { RoomJoinResultEntity } from "@/entities/roomEntity"
import router from "@/services/router"
import { useGameStore } from "@/stores/gameStore"
import { ref, computed, onMounted } from "vue"
import { useRoute } from "vue-router"

// Store
const game = useGameStore()

// UI State: User input
const name = ref("")
const joinRoomId = ref("")

// UI State: Mode toggles
const joinRoomMode = ref(false)
const showRoomConfig = ref(false)

// UI State: Validation flags
const wasTriedCreate = ref(false)
const wasTriedJoin = ref(false)

// Room settings
const rounds = ref(5)
const secondsPerRound = ref(50)

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

// Rounds input validation
const roundsError = computed(() => {
  if (rounds.value < 1 || rounds.value > 10) return "Rounds must be between 1 and 10"
  return ""
})

// Seconds per round validation
const secondsPerRoundError = computed(() => {
  if (secondsPerRound.value < 10 || secondsPerRound.value > 180)
    return "Time must be between 10 and 180 seconds"
  return ""
})

// Navigate to lobby with room data
async function toRoomLobby(roomJoinResult: RoomJoinResultEntity) {
  game.room = roomJoinResult.room
  game.userId = roomJoinResult.user.userId
  router.push({ name: "lobby" })
}

// Enter room configuration
async function enterRoomConfig() {
  wasTriedCreate.value = true
  if (nameError.value) return
  showRoomConfig.value = true
}

// Create a new room
async function createRoom() {
  wasTriedCreate.value = true
  if (nameError.value || roundsError.value || secondsPerRoundError.value) return
  try {
    const res = await game.api.createRoom(name.value.trim(), rounds.value, secondsPerRound.value)
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

// Reset state
function resetState() {
  game.room = null
  game.round = null
  game.userId = null
  game.scores = []
}

// Check for room query param on mount
onMounted(() => {
  resetState()
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
    class="flex min-h-full items-center justify-center bg-gradient-to-br from-sky-100 to-blue-100 px-4"
  >
    <div
      class="motion-safe:animate-fade-in w-full max-w-sm space-y-6 rounded-2xl bg-white p-6 shadow-xl"
    >
      <!-- Entry page -->
      <div v-if="!showRoomConfig">
        <!-- App Title -->
        <div class="text-center">
          <img
            src="@/assets/findone.png"
            alt="Findone Logo"
            class="animate-breathing mx-auto mb-4 h-32 w-32"
          />
          <h1 class="text-3xl font-extrabold tracking-wide text-blue-600">Findone</h1>
          <p class="mt-1 mb-5 text-sm text-gray-500">Fast and fun AI-powered gameplay</p>
        </div>
        <!-- Form Content -->
        <div class="space-y-4">
          <!-- Name input -->
          <div class="space-y-1">
            <input
              v-model="name"
              type="text"
              placeholder="Enter your name"
              class="w-full rounded-xl border px-4 py-2 transition duration-150 focus:ring-2 focus:outline-none"
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
              @click="enterRoomConfig"
              class="w-full rounded-xl bg-gradient-to-r from-blue-500 to-blue-600 py-2 font-semibold text-white shadow transition-transform duration-150"
              :class="{ 'hover:scale-105 active:scale-95': true }"
            >
              Create Room
            </button>
            <!-- Divider with lines -->
            <div class="m-4 flex items-center justify-between text-sm text-gray-400">
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
                class="w-full rounded-xl border px-4 py-2 transition duration-150 focus:ring-2 focus:outline-none"
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
            class="w-full rounded-xl bg-gradient-to-r from-green-500 to-green-600 py-2 font-semibold text-white shadow transition-transform duration-150"
            :class="{ 'hover:scale-105 active:scale-95': true }"
          >
            Join Room
          </button>
        </div>
      </div>
      <!-- Create room config -->
      <div v-else class="space-y-4">
        <div class="text-center text-xl font-bold text-blue-600">Room Settings</div>
        <!-- Number of rounds -->
        <div class="space-y-1">
          <label class="block text-sm font-medium text-gray-700">Number of Rounds</label>
          <input
            v-model="rounds"
            type="number"
            inputmode="numeric"
            min="1"
            max="10"
            class="w-full rounded-xl border px-4 py-2 focus:ring-2 focus:ring-blue-400 focus:outline-none"
          />
          <p v-if="wasTriedCreate && roundsError" class="text-sm text-red-500">
            {{ roundsError }}
          </p>
        </div>
        <!-- Seconds per round -->
        <div class="space-y-1">
          <label class="block text-sm font-medium text-gray-700">Time per Round (seconds)</label>
          <input
            v-model="secondsPerRound"
            type="number"
            inputmode="numeric"
            min="10"
            max="180"
            class="w-full rounded-xl border px-4 py-2 focus:ring-2 focus:ring-blue-400 focus:outline-none"
          />
          <p v-if="wasTriedCreate && secondsPerRoundError" class="text-sm text-red-500">
            {{ secondsPerRoundError }}
          </p>
        </div>
        <!-- Buttons -->
        <div class="space-y-3">
          <button
            @click="createRoom"
            class="w-full rounded-xl bg-blue-600 py-2 font-semibold text-white shadow transition-transform duration-150 hover:scale-105 active:scale-95"
          >
            Confirm & Create Room
          </button>
          <button
            @click="showRoomConfig = false"
            class="w-full rounded-xl bg-gray-300 py-2 font-medium text-gray-800 shadow transition duration-150 hover:scale-105 active:scale-95"
          >
            Cancel
          </button>
        </div>
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
