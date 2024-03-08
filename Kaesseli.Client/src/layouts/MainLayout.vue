<template>
  <q-layout view="hHh lpR fFf">

    <q-header class="bg-indigo-9" elevated height-hint="98">
      <q-toolbar>
        <q-btn dense flat round icon="menu" @click="toggleLeftDrawer" />

        <q-toolbar-title>
          <!-- Klickbare Region, die zur Startseite navigiert, ohne das Aussehen zu ändern -->
          <div @click="router.push('/')" style="cursor: pointer;">
            <q-avatar>
              <img src="https://cdn.quasar.dev/logo-v2/svg/logo-mono-white.svg">
            </q-avatar>
            Kässeli
          </div>
        </q-toolbar-title>

        <q-select v-model="selectedPeriod"
                  :options="accountingPeriods"
                  option-value="id"
                  option-label="description"
                  emit-value
                  dense
                  standout
                  map-options
                  @update:model-value="onAccountingPeriodSelect" />
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

<script lang="ts">
  import { ref, Ref, watch, onMounted } from 'vue';
  import { useQuasar } from 'quasar';
  import { useRouter } from 'vue-router';
  import axios from 'axios';
  import { IAccountingPeriod } from '../interfaces/IAccountingPeriod'; 


  export default {
    setup() {
      const leftDrawerOpen = ref(false)
      const darkMode = ref(false);
      const $q = useQuasar()
      const totalOpenTransaction = ref(0);
      const accountingPeriods: Ref<IAccountingPeriod[] | null> = ref(null);
      const selectedPeriod: Ref<IAccountingPeriod | null | undefined> = ref(null)
      const router = useRouter();

      const fetchTotalOpen = async () => {
        try {
          const response = await axios.get('https://localhost:7123/transaction/totalOpen');
          totalOpenTransaction.value = response.data;
        } catch (error) {
          console.error('There was an error fetching the accounts:', error);
        }
      };

      const fetchAccountingPeriods = async () => {
        try {
          const response = await axios.get('https://localhost:7123/accountingPeriod');
          accountingPeriods.value = response.data;
          if (accountingPeriods.value === null) {
            return;
          }
          if (selectedPeriod.value == null) {
            const savedPeriodId = localStorage.getItem('selectedPeriod');
            if (savedPeriodId === null) {
              selectedPeriod.value = accountingPeriods.value[accountingPeriods.value.length - 1];
            } else {
              selectedPeriod.value = accountingPeriods.value.find(period => period.id === savedPeriodId);
            }
          }
        } catch (error) {
          console.error('There was an error fetching the accounts:', error);
        }
      };

      onMounted(() => {
        fetchTotalOpen();
        fetchAccountingPeriods();
      });

      function onAccountingPeriodSelect(value: string) {
        console.log('Ausgewähltes Konto:', value);
        localStorage.setItem('selectedPeriod', value);
        window.location.reload();
      }

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
        totalOpenTransaction,
        accountingPeriods,
        onAccountingPeriodSelect,
        router

      }
    }
  }
</script>
