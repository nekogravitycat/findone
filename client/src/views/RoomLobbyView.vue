<script setup lang="ts">
import { updateRoomInfo, useGameStore } from "@/stores/gameStore"

const game = useGameStore()

function updateRoom() {
  if (!game.room?.roomId) {
    console.error("Room ID is not available")
    return
  }
  updateRoomInfo(game.room.roomId)
}
</script>

<template>
  <div class="p-4 space-y-4">
    <!-- User list -->
    <div class="space-y-2">
      <h2 class="text-xl font-semibold">Players in Room {{ game.room?.roomId }}</h2>
      <div
        v-for="userId in game.room?.userIds"
        :key="userId"
        class="flex items-center justify-between p-3 bg-white rounded-lg shadow border"
      >
        <p class="text-gray-800 font-medium">{{ game.usersCache.get(userId)?.userName }}</p>
        <p class="text-gray-800 font-medium">{{ game.usersCache.get(userId)?.userId }}</p>
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
  </div>
</template>
