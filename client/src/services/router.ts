import EntryView from "@/views/EntryView.vue"
import GameRankView from "@/views/GameRankView.vue"
import GameResultView from "@/views/GameResultView.vue"
import GameRoundView from "@/views/GameRoundView.vue"
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
    {
      path: "/game",
      name: "game",
      component: GameRoundView,
    },
    {
      path: "/rank",
      name: "rank",
      component: GameRankView,
    },
    {
      path: "/result",
      name: "result",
      component: GameResultView,
    },
  ],
})

export default router
