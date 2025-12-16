<template>
  <div class="container-auth">
    <div class="container-body">
      <div class="AuthView">
        <AuthBar />
      </div>
    </div>
  </div>
</template>

<script>
import AuthBar from "@/components/AuthBar.vue";

export default {
  name: "AuthView",
  components: {
    AuthBar,
  },
};
</script>

<style scoped>
.container-auth {
  display: grid;
  height: 100%;
  justify-content: center;
  align-items: center;
  width: 100%;
}

.container-body {
  display: flex;
  justify-content: center;
  align-items: center;
  width: 100%;
  flex: 1;
}
</style>
