import EntryView from "@/views/EntryView.vue"
import RoomLobbyView from "@/views/RoomLobbyView.vue"
import { createRouter, createWebHistory } from "vue-router"

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: "/",
      name: "entry",
      component: EntryView,
    },
    {
      path: "/lobby",
      name: "lobby",
      component: RoomLobbyView,
    },
  ],
})

export default router
