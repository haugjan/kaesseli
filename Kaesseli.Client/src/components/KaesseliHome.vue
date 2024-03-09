<template>
  <div class="q-pa-md row items-start">

    <div v-for="type in accountTypes" v-bind:key="type" class="q-pa-md col-lg-4 col-md-6 col-sm-12 col-xs-12">
      <q-card bordered>
        <span class="header">
          <q-card-section> <q-avatar size="md" text-color="white" :color="type.color" :icon="type.icon" class="q-mr-sm shadow-3" />{{type.name}}</q-card-section>
        </span>
        <q-separator dark inset />
        <q-card-section v-if="type.data !== undefined">
          <table>
            <tr >
              <td class="text-grey-6">Kontostand:</td>
              <td>
                {{formatNumber(type.data.accountBalance)}}
              </td>
            </tr>
            <tr v-if="type.id === 3 || type.id===4">
              <td class="text-grey-6">Σ Budget:</td>
              <td>
                {{formatNumber(type.data.budget)}}
              </td>
            </tr>
            <tr v-if="type.id === 3 || type.id===4">
              <td class="text-grey-6">📅 Budget:</td>
              <td>
                {{formatNumber(type.data.currentBudget)}}
              </td>
            </tr>
            <tr v-if="type.id === 3 || type.id===4">
              <td class="text-grey-6">Budgetsaldo:</td>
              <td class="text-bold balance" :data-fldval="type.data.budgetBalance">
                {{formatNumber(type.data.budgetBalance)}}
              </td>
            </tr>
          </table>
        </q-card-section>
      </q-card>
    </div>


  </div>
</template>

<script lang="ts">
  import { defineComponent, ref, inject, onMounted, Ref, watch } from 'vue';
  import axios from 'axios';
  import { useQuasar } from 'quasar';

  interface IFinancialCategory {
    accountBalance: number;
    budget: number | null;
    currentBudget: number | null;
    budgetBalance: number | null;
  }

  interface IFinancialOverview {
    expense: IFinancialCategory;
    revenue: IFinancialCategory;
    liability: IFinancialCategory;
    asset: IFinancialCategory;
  }


  export default defineComponent({
    setup() {

      const overview = ref<IFinancialOverview>();
      const $q = useQuasar();
      const fetchOverview = async () => {
        try {
          const savedPeriodId: string | null = localStorage.getItem('selectedPeriod');
          if (savedPeriodId === null) {
            return;
          }
          const response = await axios.get(`https://localhost:7123/accountingPeriod/${savedPeriodId}/overView`);
          overview.value = response.data;
          accountTypes.value = [
            { id: 3, name: 'Einkommen', icon: 'attach_money', color: 'green', data: overview.value?.revenue },
            { id: 4, name: 'Ausgaben', icon: 'money_off', color: 'red', data: overview.value?.expense },
            { id: 2, name: 'Vermögen', icon: 'account_balance', color: 'blue', data: overview.value?.asset },
          ];
        } catch (error) {
          $q.notify({
            type: 'negative',
            message: 'There was an error fetching the overview',
            caption: error
          });
        }
      };

      const accountTypes = ref([
        { name: 'Einkommen', icon: 'attach_money', color: 'green', data: undefined },
        { name: 'Ausgaben', icon: 'money_off', color: 'red', data: undefined },
        { name: 'Vermögen', icon: 'account_balance', color: 'blue', data: undefined },
      ]);

      function formatNumber(value: number, locale: string = navigator.language): string {
        return new Intl.NumberFormat(locale, {
          style: 'decimal',
          minimumFractionDigits: 2,
          maximumFractionDigits: 2,
        }).format(value);
      }

      onMounted(() => {
        fetchOverview();
      });

      return {
        accountTypes,
        formatNumber
      };
    }
  });
</script>

<style>
  .balance[data-fldval^='-'] {
    color: red;
    font-weight: bold;
  }

  .balance:not([data-fldval^='-']) {
    color: green;
    font-weight: bold;
  }
</style>
