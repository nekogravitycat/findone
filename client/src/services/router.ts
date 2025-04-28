import EntryView from "@/views/EntryView.vue"
import { createRouter, createWebHistory } from "vue-router"

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: "/",
      name: "entry",
      component: EntryView,
    },
  ],
})

export default router
