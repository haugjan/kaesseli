<template>
  <q-layout view="hHh lpR fFf">

    <q-header class="bg-indigo-9" elevated height-hint="98">
      <q-toolbar>
        <q-btn dense flat round icon="menu" @click="toggleLeftDrawer" />

        <q-toolbar-title>
          <q-avatar>
            <img src="https://cdn.quasar.dev/logo-v2/svg/logo-mono-white.svg">
          </q-avatar>
          Kässeli
        </q-toolbar-title>
        <q-toggle v-model="darkMode" icon="contrast" color="black" />
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
        <q-item clickable v-ripple to="/import">
          <q-item-section avatar>
            <q-icon name="folder_open" />
          </q-item-section>
          <q-item-section>
            <q-item-label>Import</q-item-label>
          </q-item-section>
        </q-item>
        <q-item clickable v-ripple to="/assign">
          <q-item-section avatar>
            <q-icon name="assignment_turned_in" />
          </q-item-section>
          <q-item-section>
            <!--<q-item-label>Zuordnen</q-item-label>
            <q-badge color="red" floating>4</q-badge>-->
            <div>
              Zuordnen
              <q-badge color="red" rounded align="top">{{totalOpenTransaction}}</q-badge>
            </div>
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
  import { ref, watch, onMounted } from 'vue';
  import { useQuasar } from 'quasar';
  import axios from 'axios';

  export default {
    setup() {
      const leftDrawerOpen = ref(false)
      const darkMode = ref(false);
      const $q = useQuasar()
      const totalOpenTransaction = ref(0);

      const fetchTotalOpen = async () => {
        try {
          const response = await axios.get('https://localhost:7123/transaction/totalOpen');
          totalOpenTransaction.value = response.data;
        } catch (error) {
          console.error('There was an error fetching the accounts:', error);
        }
      };

      onMounted(() => {
        fetchTotalOpen();
      });

      watch(darkMode, (newValue) => {
        $q.dark.set(newValue)
      });

      return {
        leftDrawerOpen,
        toggleLeftDrawer() {
          leftDrawerOpen.value = !leftDrawerOpen.value
        },
        darkMode,
        fetchTotalOpen,
        totalOpenTransaction
      }
    }
  }
</script>
