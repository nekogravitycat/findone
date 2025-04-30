<script setup lang="ts">
import { SignalRService } from "@/services/signalr"
import { onMounted, ref } from "vue"

let api = new SignalRService()

const name = ref("")

async function createRoom() {
  let res = await api.createRoom(name.value, 5, 100)
  console.log(`Room created: room=${res.roomId}, user=${res.user}`)
}

onMounted(async () => {
  let connection = await api.start()
  console.log("SignalR connection started", connection)
})
</script>

<template>
  <!-- Input textbox for name -->
  <input v-model="name" type="text" placeholder="Name" />
  <!-- Button for create room -->
  <button @click="createRoom">Create Room</button>
</template>
