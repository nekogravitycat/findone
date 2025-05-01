<script setup lang="ts">
import type { RoomJoinResultEntity } from "@/entities/roomEntity"
import router from "@/services/router"
import { useGameStore } from "@/stores/gameStore"
import { ref } from "vue"

const game = useGameStore()

const name = ref("")
const joinRoomId = ref("")

async function createRoom() {
  try {
    let res = await game.api.createRoom(name.value, 5, 100)
    console.log(`Room created: room=${res.roomId}, user=${res.user}`)
    toRoomLobby(res)
  } catch (e) {
    console.error(e)
    return
  }
}

async function joinRoom() {
  try {
    let res = await game.api.joinRoom(joinRoomId.value, name.value)
    console.log(`Room joined: room=${res.roomId}, user=${res.user}`)
    toRoomLobby(res)
  } catch (e) {
    console.error(e)
    return
  }
}

async function toRoomLobby(roomJoinResult: RoomJoinResultEntity) {
  game.room = await game.api.getRoom(roomJoinResult.roomId)
  game.userId = roomJoinResult.user.userId
  game.usersCache.set(game.userId, roomJoinResult.user)
  router.push({ name: "lobby" })
}
</script>

<template>
  <div class="flex flex-col space-y-2">
    <!-- Input textbox for name -->
    <input v-model="name" type="text" placeholder="Name" class="border px-3 py-2 rounded" />
    <!-- Button for create room -->
    <button @click="createRoom" class="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600">
      Create Room
    </button>
    <!-- Input textbox for join room id -->
    <input
      v-model="joinRoomId"
      type="text"
      placeholder="Room ID"
      class="border px-3 py-2 rounded"
    />
    <!-- Button for join room -->
    <button @click="joinRoom" class="bg-green-500 text-white px-4 py-2 rounded hover:bg-green-600">
      Join Room
    </button>
  </div>
</template>
