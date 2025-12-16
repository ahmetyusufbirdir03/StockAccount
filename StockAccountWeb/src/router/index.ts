import { createRouter, createWebHistory, type RouteRecordRaw } from "vue-router";
import Auth from "@/views/auth/Auth.vue";

const routes: Array<RouteRecordRaw> = [
  {
    path: "/",
    name: "Auth",
    component: Auth,
  },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

export default router;
