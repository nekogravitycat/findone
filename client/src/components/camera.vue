<script setup lang="ts">
import { onMounted, onUnmounted, ref } from "vue"

const emit = defineEmits<{
  (e: "photoTaken", photo: string): void
}>()

const video = ref<HTMLVideoElement | null>(null)
const canvas = ref<HTMLCanvasElement | null>(null)
const photo = ref<string | null>(null)

const videoDevices = ref<MediaDeviceInfo[]>([])
const selectedDeviceId = ref<string | null>(null)
let currentStream: MediaStream | null = null

async function startCamera() {
  if (currentStream) {
    currentStream.getTracks().forEach((track) => track.stop())
  }
  try {
    const constraints: MediaStreamConstraints = {
      video: {
        deviceId: selectedDeviceId.value ? { exact: selectedDeviceId.value } : undefined,
      },
    }
    const stream = await navigator.mediaDevices.getUserMedia(constraints)
    currentStream = stream
    if (video.value) {
      video.value.srcObject = stream
    }
  } catch (err) {
    console.error("Cannot access camera", err)
  }
}

function stopCamera() {
  if (currentStream) {
    currentStream.getTracks().forEach((track) => track.stop())
    currentStream = null
  }
}

async function getVideoDevices() {
  try {
    const devices = await navigator.mediaDevices.enumerateDevices()
    videoDevices.value = devices.filter((d) => d.kind === "videoinput")
    if (videoDevices.value.length > 0 && !selectedDeviceId.value) {
      selectedDeviceId.value = videoDevices.value[0].deviceId
      await startCamera()
    }
  } catch (err) {
    console.error("Cannot list cameras", err)
  }
}

function takePhoto() {
  const vid = video.value
  const can = canvas.value
  if (vid && can) {
    const context = can.getContext("2d")
    if (context) {
      can.width = vid.videoWidth
      can.height = vid.videoHeight
      context.drawImage(vid, 0, 0, can.width, can.height)
      photo.value = can.toDataURL("image/jpeg", 0.8)
      emit("photoTaken", photo.value.split(",")[1]) // Send only the base64 part
    }
  }
}

onMounted(getVideoDevices)
onUnmounted(stopCamera)
</script>

<template>
  <div class="flex flex-col items-center justify-center h-screen bg-black relative overflow-hidden">
    <!-- Camera list -->
    <select
      v-if="videoDevices.length > 1"
      v-model="selectedDeviceId"
      @change="startCamera"
      class="absolute top-4 left-4 bg-white text-black rounded px-3 py-1 text-sm z-10"
    >
      <option v-for="device in videoDevices" :key="device.deviceId" :value="device.deviceId">
        {{ device.label || "Camera " + device.deviceId }}
      </option>
    </select>

    <!-- Viewfinder -->
    <video
      ref="video"
      autoplay
      playsinline
      class="w-full h-auto max-h-[80vh] object-cover rounded-lg"
    ></video>

    <!-- Shutter -->
    <button
      @click="takePhoto"
      class="absolute bottom-6 w-16 h-16 rounded-full bg-white border-4 border-gray-300 active:scale-95 transition z-10"
    ></button>

    <!-- Captured result -->
    <canvas ref="canvas" class="hidden"></canvas>
    <img v-if="photo" :src="photo" class="absolute top-4 right-4 w-20 rounded-lg shadow-md" />
  </div>
</template>
