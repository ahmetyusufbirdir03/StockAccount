<template>
  <div class="container">
    <!-- Slider -->
    <div class="slider" :class="{ right: isRight }" @click="toggle">
      <span class="slider-text">
        {{ isRight ? 'Login' : 'Register' }}
      </span>
    </div>

    <!-- REGISTER -->
    <div class="panel left">
      <Transition name="form" mode="in-out">
        <form v-show="isRight" class="authForm">
          <h2>Register</h2>

          <input type="text" placeholder="Name" />
          <input type="text" placeholder="Surname" />
          <input type="email" placeholder="Email" />
          <input type="password" placeholder="Password" />

          <button type="button">Register</button>
        </form>
      </Transition>
    </div>

    <!-- LOGIN -->
    <div class="panel right">
      <Transition name="form" mode="out-in">
        <form v-show="!isRight" class="authForm">
          <h2>Login</h2>

          <input type="email" placeholder="Email" />
          <input type="password" placeholder="Password" />

          <button type="button">Login</button>
        </form>
      </Transition>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const isRight = ref(false)

const toggle = () => {
  isRight.value = !isRight.value
}
</script>

<style>
.container {
  width: 1280px;
  height: 720px;
  position: relative;
  overflow: hidden;
  border-radius: 16px;
}

/* PANELS */
.panel {
  position: absolute;
  width: 50%;
  height: 100%;
  top: 0;
  display: flex;
  justify-content: center;
  align-items: center;
}

.panel.left {
  left: 0;
  background: var(--bg-left);
}

.panel.right {
  right: 0;
  background: var(--bg-right);
}

/* SLIDER */
.slider {
  position: absolute;
  width: 50%;
  height: 100%;
  left: 0;
  top: 0;
  background: var(--primary);
  transition: transform 0.5s ease;
  z-index: 10;
  display: flex;
  justify-content: center;
  align-items: center;
  cursor: pointer;
}

.slider.right {
  transform: translateX(100%);
}

.slider-text {
  color: white;
  font-size: 28px;
  font-weight: 600;
}

/* FORM */
.authForm {
  width: 70%;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.authForm h2 {
  text-align: center;
}

.authForm input {
  padding: 10px;
  border-radius: 8px;
  border: 1px solid #ccc;
  height: 50px;
  font-size: larger;
}

.authForm button {
  padding: 10px;
  border-radius: 8px;
  border: none;
  background: #111827;
  color: white;
  cursor: pointer;
  height: 50px;
  font-size: larger;
}

/* FORM ANIMATION */
.form-enter-active,
.form-leave-active {
  transition: all 0.35s ease;
}

.form-enter-from {
  opacity: 0;
  transform: translateY(20px);
}

.form-leave-to {
  opacity: 0;
  transform: translateY(-20px);
}
</style>