<script setup lang="ts">
import { onMounted } from "vue"
import { RouterView } from "vue-router"
import { useGameStore } from "./stores/gameStore"

const game = useGameStore()

onMounted(() => {
  game.api.start()
})
</script>

<template>
  <RouterView />
</template>
