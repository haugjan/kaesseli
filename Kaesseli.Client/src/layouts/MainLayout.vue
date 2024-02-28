<template>
  <q-layout view="hHh lpR fFf">

    <q-header elevated height-hint="98">
      <q-toolbar>
        <q-btn dense flat round icon="menu" @click="toggleLeftDrawer" />

        <q-toolbar-title>
          <q-avatar>
            <img src="https://cdn.quasar.dev/logo-v2/svg/logo-mono-white.svg">
          </q-avatar>
          Kässeli
        </q-toolbar-title>
      </q-toolbar>

    </q-header>

    <q-drawer show-if-above v-model="leftDrawerOpen" side="left" elevated>
      <q-list>
        <q-item clickable v-ripple to="/accounts">
          <q-item-section avatar>
            <q-icon name="account_balance" />
          </q-item-section>
          <q-item-section>
            <q-item-label>Kontoübersicht</q-item-label>
          </q-item-section>
        </q-item>
        <q-item clickable v-ripple to="/transactions">
          <q-item-section avatar>
            <q-icon name="receipt_long" />
          </q-item-section>
          <q-item-section>
            <q-item-label>Kontobewegungen</q-item-label>
          </q-item-section>
        </q-item>
        <q-item clickable v-ripple to="/assign">
          <q-item-section avatar>
            <q-icon name="assignment_turned_in" />
          </q-item-section>
          <q-item-section>
            <q-item-label>Zuordnen</q-item-label>
          </q-item-section>
        </q-item>
      </q-list>
    </q-drawer>

    <q-page-container>
      <router-view />
    </q-page-container>

  </q-layout>
</template>

<script>
  import { ref } from 'vue'

  export default {
    setup() {
      const leftDrawerOpen = ref(false)

      return {
        leftDrawerOpen,
        toggleLeftDrawer() {
          leftDrawerOpen.value = !leftDrawerOpen.value
        }
      }
    }
  }
</script>
