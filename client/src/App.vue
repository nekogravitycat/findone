<script setup lang="ts">
import { useGameStore } from "@/stores/gameStore"
import { onMounted } from "vue"
import { RouterView } from "vue-router"

const game = useGameStore()

onMounted(() => {
  game.api.start()
})
</script>

<template>
  <RouterView />
</template>
